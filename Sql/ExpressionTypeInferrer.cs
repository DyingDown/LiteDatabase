using LiteDatabase.Catalog;
using LiteDatabase.Sql.Ast;
using LiteDatabase.Sql.Ast.Expressions;

namespace LiteDatabase.Sql;

/// <summary>
/// 表达式类型推断器 - 递归推断表达式的数据类型
/// </summary>
public class ExpressionTypeInferrer
{
    private readonly ICatalogManager _catalog;
    private readonly Dictionary<string, Dictionary<string, ColumnDefinition>> _tableSchemas;

    public ExpressionTypeInferrer(ICatalogManager catalog, Dictionary<string, Dictionary<string, ColumnDefinition>> tableSchemas)
    {
        _catalog = catalog;
        _tableSchemas = tableSchemas;
    }

    /// <summary>
    /// 推断表达式的类型
    /// </summary>
    /// <param name="expression">要推断的表达式</param>
    /// <returns>推断出的列类型</returns>
    /// <exception cref="Exception">如果无法推断类型</exception>
    public ColumnType InferType(Expression expression)
    {
        // 检查是否已经缓存了类型
        if (expression.InferredType.HasValue)
        {
            return expression.InferredType.Value;
        }

        // 推断类型并缓存结果
        var inferredType = expression switch
        {
            LiteralExpression literal => InferLiteralType(literal),
            ColumnRefExpression columnRef => InferColumnRefType(columnRef),
            BinaryExpression binary => InferBinaryExpressionType(binary),
            UnaryExpression unary => InferUnaryExpressionType(unary),
            FunctionCallExpression function => InferFunctionType(function),
            SubqueryExpression subquery => InferSubqueryType(subquery),
            InExpression inExpr => ColumnType.Bool, // IN 表达式总是返回布尔值
            BetweenExpression between => ColumnType.Bool, // BETWEEN 表达式总是返回布尔值
            _ => throw new Exception($"Cannot infer type for expression: {expression.GetType().Name}")
        };

        // 缓存推断的类型
        expression.InferredType = inferredType;
        return inferredType;
    }

    /// <summary>
    /// 清除表达式树中的所有类型缓存（用于重新分析）
    /// </summary>
    /// <param name="expression">要清除缓存的表达式</param>
    public void ClearTypeCache(Expression expression)
    {
        expression.InferredType = null;

        // 递归清除子表达式的缓存
        switch (expression)
        {
            case BinaryExpression binary:
                ClearTypeCache(binary.Left);
                ClearTypeCache(binary.Right);
                break;
            case UnaryExpression unary:
                ClearTypeCache(unary.Operand);
                break;
            case FunctionCallExpression function:
                foreach (var arg in function.Arguments)
                {
                    ClearTypeCache(arg);
                }
                break;
            case InExpression inExpr:
                ClearTypeCache(inExpr.Expression);
                foreach (var value in inExpr.Values)
                {
                    ClearTypeCache(value);
                }
                break;
            case BetweenExpression between:
                ClearTypeCache(between.Expression);
                ClearTypeCache(between.LowerBound);
                ClearTypeCache(between.UpperBound);
                break;
            case SubqueryExpression subquery:
                // 子查询的缓存清除需要更复杂的处理
                break;
        }
    }

    /// <summary>
    /// 推断字面量的类型
    /// </summary>
    private ColumnType InferLiteralType(LiteralExpression literal)
    {
        return literal.Value switch
        {
            int => ColumnType.Int,
            long => ColumnType.Int,
            float => ColumnType.Float,
            double => ColumnType.Float,
            decimal => ColumnType.Float,
            string => ColumnType.String,
            bool => ColumnType.Bool,
            null => throw new Exception("Cannot infer type of NULL literal without context"),
            _ => throw new Exception($"Unknown literal type: {literal.Value?.GetType()}")
        };
    }

    /// <summary>
    /// 推断列引用的类型
    /// </summary>
    private ColumnType InferColumnRefType(ColumnRefExpression columnRef)
    {
        if (!string.IsNullOrEmpty(columnRef.TableName))
        {
            // 指定了表名
            if (!_tableSchemas.TryGetValue(columnRef.TableName, out var tableSchema))
                throw new Exception($"Table '{columnRef.TableName}' not found in context");

            if (!tableSchema.TryGetValue(columnRef.ColumnName, out var columnDef))
                throw new Exception($"Column '{columnRef.ColumnName}' not found in table '{columnRef.TableName}'");

            return columnDef.ColumnType;
        }
        else
        {
            // 没有指定表名，需要在所有表中查找
            var matches = _tableSchemas
                .Where(table => table.Value.ContainsKey(columnRef.ColumnName))
                .Select(table => table.Value[columnRef.ColumnName])
                .ToList();

            if (matches.Count == 0)
                throw new Exception($"Column '{columnRef.ColumnName}' not found in any table");

            if (matches.Count > 1)
            {
                // 检查所有匹配的列是否具有相同的类型
                var firstType = matches[0].ColumnType;
                if (matches.All(col => col.ColumnType == firstType))
                    return firstType;
                else
                    throw new Exception($"Ambiguous column '{columnRef.ColumnName}' with different types in multiple tables");
            }

            return matches[0].ColumnType;
        }
    }

    /// <summary>
    /// 推断二元表达式的类型
    /// </summary>
    private ColumnType InferBinaryExpressionType(BinaryExpression binary)
    {
        var leftType = InferType(binary.Left);
        var rightType = InferType(binary.Right);

        return binary.Operator switch
        {
            // 比较操作符总是返回布尔值
            BinaryOperatorType.Equal => ColumnType.Bool,
            BinaryOperatorType.NotEqual => ColumnType.Bool,
            BinaryOperatorType.LessThan => ColumnType.Bool,
            BinaryOperatorType.LessOrEqual => ColumnType.Bool,
            BinaryOperatorType.GreaterThan => ColumnType.Bool,
            BinaryOperatorType.GreaterOrEqual => ColumnType.Bool,

            // 逻辑操作符总是返回布尔值
            BinaryOperatorType.And => ColumnType.Bool,
            BinaryOperatorType.Or => ColumnType.Bool,

            // 算术操作符需要类型提升
            BinaryOperatorType.Add => PromoteNumericTypes(leftType, rightType),
            BinaryOperatorType.Subtract => PromoteNumericTypes(leftType, rightType),
            BinaryOperatorType.Multiply => PromoteNumericTypes(leftType, rightType),
            BinaryOperatorType.Divide => PromoteNumericTypes(leftType, rightType),
            BinaryOperatorType.Modulo => PromoteNumericTypes(leftType, rightType),

            _ => throw new Exception($"Unknown binary operator: {binary.Operator}")
        };
    }

    /// <summary>
    /// 推断一元表达式的类型
    /// </summary>
    private ColumnType InferUnaryExpressionType(UnaryExpression unary)
    {
        var operandType = InferType(unary.Operand);

        return unary.Operator switch
        {
            UnaryOperatorType.Not => ColumnType.Bool,
            UnaryOperatorType.Minus => operandType, // 负号保持原类型
            UnaryOperatorType.Plus => operandType,  // 正号保持原类型
            _ => throw new Exception($"Unknown unary operator: {unary.Operator}")
        };
    }

    /// <summary>
    /// 推断函数调用的类型
    /// </summary>
    private ColumnType InferFunctionType(FunctionCallExpression function)
    {
        return function.FunctionName switch
        {
            FunctionName.Count => ColumnType.Int,
            FunctionName.Sum => ColumnType.Float, // 聚合函数可能产生浮点数
            FunctionName.Avg => ColumnType.Float,
            FunctionName.Min => InferType(function.Arguments[0]), // MIN/MAX 保持参数类型
            FunctionName.Max => InferType(function.Arguments[0]),
            _ => throw new Exception($"Unknown function: {function.FunctionName}")
        };
    }

    /// <summary>
    /// 推断子查询的类型
    /// </summary>
    private ColumnType InferSubqueryType(SubqueryExpression subquery)
    {
        // 子查询的类型推断比较复杂，这里简化处理
        // 实际实现中需要分析子查询的 SELECT 列表
        throw new Exception("Subquery type inference not implemented yet");
    }

    /// <summary>
    /// 数值类型提升规则
    /// </summary>
    private ColumnType PromoteNumericTypes(ColumnType left, ColumnType right)
    {
        // 如果任一操作数是浮点数，结果就是浮点数
        if (left == ColumnType.Float || right == ColumnType.Float)
            return ColumnType.Float;

        // 如果两个都是整数，结果是整数
        if (left == ColumnType.Int && right == ColumnType.Int)
            return ColumnType.Int;

        throw new Exception($"Cannot perform arithmetic operation between {left} and {right}");
    }

    /// <summary>
    /// 检查两个类型是否可以进行比较
    /// </summary>
    public bool CanCompare(ColumnType left, ColumnType right)
    {
        // 相同类型可以比较
        if (left == right) return true;

        // 数值类型之间可以比较
        if (IsNumericType(left) && IsNumericType(right)) return true;

        return false;
    }

    /// <summary>
    /// 检查是否是数值类型
    /// </summary>
    private bool IsNumericType(ColumnType type)
    {
        return type == ColumnType.Int || type == ColumnType.Float;
    }
}