using LiteDatabase.Sql.Ast.Expressions;
using LiteDatabase.Catalog;
using LiteDatabase.Sql.Ast;

namespace LiteDatabase.Sql.Semantic;

public class ExpressionTypeInferrer {
    private readonly ICatalogManager catalog;

    private readonly Dictionary<string, Dictionary<string, ColumnDefinition>> tableSchemas;

    public ExpressionTypeInferrer(ICatalogManager catalogManager, Dictionary<string, Dictionary<string, ColumnDefinition>> tableSchemas) {
        catalog = catalogManager;
        this.tableSchemas = tableSchemas;
    }

    public ExpressionType InferType(Expression expression) {
        if (expression.InferredType != null) {
            return expression.InferredType;
        }

        var inferredType = expression switch {
            BetweenExpression between => InferBetweenExpression(between),
            BinaryExpression binary => InferBinaryExpression(binary),
            ColumnRefExpression colRef => InferColumnRefExpression(colRef),
            FunctionCallExpression function => InferFunctionCallExpression(function),
            InExpression inExp => InferInExpression(inExp),
            LiteralExpression literal => InferLiteralExpression(literal),
            SubqueryExpression subquery => InferSubqueryExpression(subquery),
            UnaryExpression unary => InferUnaryExpression(unary),
            _ => ExpressionType.Unkown()
        };

        // cache
        expression.InferredType = inferredType;
        return inferredType;
    }

    private ExpressionType InferUnaryExpression(UnaryExpression unary) {
        var operandType = InferType(unary.Operand);

        switch (unary.Operator) {
            case UnaryOperatorType.Not:
                // NOT 操作符返回布尔类型
                return ExpressionType.Scalar(ColumnType.Bool, operandType.IsNullable);
            
            case UnaryOperatorType.Plus:
            case UnaryOperatorType.Minus:
                // + 和 - 操作符返回相同的数值类型
                if (operandType.BaseType == null) {
                    return ExpressionType.Unkown();
                }
                return ExpressionType.Scalar(operandType.BaseType.Value, operandType.IsNullable);
            
            default:
                return ExpressionType.Unkown();
        }
    }

    private ExpressionType InferSubqueryExpression(SubqueryExpression subquery) {
        // 上层已经验证了子查询的合法性，这里只负责类型推断
        var selectNode = subquery.Subquery;
        var rowSchema = new List<ExpressionType>();

        // 构建子查询的表模式作用域（用于类型推断）
        var nestedTableSchemas = new Dictionary<string, Dictionary<string, ColumnDefinition>>();
        
        // 添加子查询 FROM 子句中的表和别名到嵌套作用域
        foreach (var (alias, tableName) in selectNode.TableNamesWithAlias) {
            // 上层已经验证过表存在性和别名唯一性，这里直接添加
            nestedTableSchemas[alias] = catalog.GetTableColumns(tableName);
        }

        // 创建嵌套类型推断器
        var nestedInferrer = new ExpressionTypeInferrer(catalog, nestedTableSchemas);

        // 推断 SELECT 列表中每一项的类型
        foreach (var selectItem in selectNode.SelectList) {
            if (selectItem.IsStar) {
                // 处理 * 或 table.* 的情况
                if (selectItem.Expr is StarExpression starExpr && !string.IsNullOrEmpty(starExpr.TableName)) {
                    // table.* 情况：只展开指定表的列
                    var tableColumns = nestedTableSchemas[starExpr.TableName];
                    foreach (var (columnName, columnDef) in tableColumns) {
                        rowSchema.Add(ExpressionType.Scalar(columnDef.ColumnType, IsNullable(columnDef)));
                    }
                } else {
                    // 纯 * 情况：展开所有表的所有列
                    foreach (var tableName in nestedTableSchemas.Keys) {
                        var tableColumns = nestedTableSchemas[tableName];
                        foreach (var (columnName, columnDef) in tableColumns) {
                            rowSchema.Add(ExpressionType.Scalar(columnDef.ColumnType, IsNullable(columnDef)));
                        }
                    }
                }
            }
            else if (selectItem.Expr != null) {
                // SELECT expr - 推断表达式类型
                var columnType = nestedInferrer.InferType(selectItem.Expr);
                rowSchema.Add(columnType);
            }
        }

        return ExpressionType.RowSet(rowSchema);
    }

    private ExpressionType InferLiteralExpression(LiteralExpression literal) {
        return literal.Value switch {
            int => ExpressionType.Scalar(ColumnType.Int, false),
            long => ExpressionType.Scalar(ColumnType.Int, false),
            float => ExpressionType.Scalar(ColumnType.Float, false),
            double => ExpressionType.Scalar(ColumnType.Float, false),
            decimal => ExpressionType.Scalar(ColumnType.Float, false),
            string => ExpressionType.Scalar(ColumnType.String, false),
            bool => ExpressionType.Scalar(ColumnType.Bool, false),
            null => ExpressionType.Unkown(),
            _ => ExpressionType.Unkown()
        };
    }

    private ExpressionType InferInExpression(InExpression inExpr) {
        var leftType = InferType(inExpr.Expression);
        return ExpressionType.Scalar(ColumnType.Bool, leftType.IsNullable);
    }
    

    private ExpressionType InferFunctionCallExpression(FunctionCallExpression function) {
        // 上层已经验证了函数的合法性，这里只负责类型推断
        var functionDef = catalog.GetFunction(function.FunctionName);
        if (functionDef == null) {
            // 这是一个后备检查，正常情况下上层应该已经验证过
            throw new Exception($"Unknown function: {function.FunctionName}");
        }
        
        // 特殊处理 COUNT(*) - 直接返回类型
        bool isCountStar = function.FunctionName.Equals("COUNT", StringComparison.OrdinalIgnoreCase) && 
                          function.Arguments.Count == 1 && function.Arguments[0] is StarExpression;
        if (isCountStar) {
            return ExpressionType.Scalar(functionDef.ReturnType, false);
        }
        
        // 检查参数的可空性（用于返回类型推断）
        bool hasNullableArgs = false;
        for (int i = 0; i < function.Arguments.Count; i++) {
            var argType = InferType(function.Arguments[i]);
            
            // 如果是子查询，获取其标量类型
            if (argType.Kind == TypeKind.RowSet && argType.RowSchema?.Count == 1) {
                argType = argType.RowSchema[0];
            }
            
            // 记录参数的可空性（用于返回类型推断）
            if (argType.IsNullable) {
                hasNullableArgs = true;
            }
        }
        
        // 确定返回类型的可空性
        bool resultIsNullable = false;
        
        // 对于聚合函数，如果有可空参数，结果可能为空
        if (functionDef.IsAggregate && hasNullableArgs) {
            resultIsNullable = true;
        }
        
        // 对于特殊返回类型逻辑（如 MIN/MAX 返回参数类型）
        if (function.FunctionName.Equals("MIN", StringComparison.OrdinalIgnoreCase) ||
            function.FunctionName.Equals("MAX", StringComparison.OrdinalIgnoreCase)) {
            // MIN/MAX 返回与第一个参数相同的类型
            var firstArgType = InferType(function.Arguments[0]);
            
            // 如果参数类型无法确定，返回 Unknown
            if (firstArgType.BaseType == null) {
                return ExpressionType.Unkown();
            }
            
            return ExpressionType.Scalar(firstArgType.BaseType.Value, firstArgType.IsNullable);
        }
        
        // 默认情况：使用函数定义的返回类型
        return ExpressionType.Scalar(functionDef.ReturnType, resultIsNullable);
    }

    private ExpressionType InferBetweenExpression(BetweenExpression between) {
        // 上层已经验证了所有操作数类型和兼容性
        // BETWEEN 总是返回布尔类型，可空性基于表达式可空性
        var exprType = InferType(between.Expression);
        return ExpressionType.Scalar(ColumnType.Bool, exprType.IsNullable);
    }

    private ExpressionType InferColumnRefExpression(ColumnRefExpression colRef) {
        if (!string.IsNullOrEmpty(colRef.TableName)) {
            // 有表名限定的列引用
            var columnDef = tableSchemas[colRef.TableName][colRef.ColumnName];
            return ExpressionType.Scalar(columnDef.ColumnType, IsNullable(columnDef));
        }
        
        // 无表名限定的列引用，查找第一个匹配的列
        foreach (var (tableName, schema) in tableSchemas) {
            if (schema.TryGetValue(colRef.ColumnName, out var columnDef)) {
                return ExpressionType.Scalar(columnDef.ColumnType, IsNullable(columnDef));
            }
        }
        
        // 理论上不应该到达这里，因为上层已经验证过了
        throw new Exception($"Column '{colRef.ColumnName}' not found");
    }
    private ExpressionType InferBinaryExpression(BinaryExpression binary) {
        // 上层已经验证了操作数类型和兼容性
        // 这里只需要计算返回类型
        var left = InferType(binary.Left);
        var right = InferType(binary.Right);

        switch (binary.Operator) {
            case BinaryOperatorType.Equal:
            case BinaryOperatorType.NotEqual:
            case BinaryOperatorType.LessOrEqual:
            case BinaryOperatorType.LessThan:
            case BinaryOperatorType.GreaterOrEqual:
            case BinaryOperatorType.GreaterThan:
                // 比较操作符返回布尔类型
                return ExpressionType.Scalar(ColumnType.Bool, left.IsNullable || right.IsNullable);
                
            case BinaryOperatorType.And:
            case BinaryOperatorType.Or:
                // 逻辑操作符返回布尔类型
                return ExpressionType.Scalar(ColumnType.Bool, left.IsNullable || right.IsNullable);
                
            case BinaryOperatorType.Add:
            case BinaryOperatorType.Multiply:
            case BinaryOperatorType.Subtract:
            case BinaryOperatorType.Divide:
                // 算术操作符：如果任一操作数是 Float，结果为 Float，否则为 Int
                var resultType = (left.BaseType == ColumnType.Float || right.BaseType == ColumnType.Float) 
                    ? ColumnType.Float 
                    : ColumnType.Int;
                return ExpressionType.Scalar(resultType, left.IsNullable || right.IsNullable);
                
            default:
                throw new Exception($"Unknown binary operator: {binary.Operator}");
        }
    }

    public bool CanCompare(ExpressionType left, ExpressionType right, bool requireOrdering) {
        if (left == null || right == null) return false;
        if (left.Kind != TypeKind.Scalar || right.Kind != TypeKind.Scalar)
            return false;
        if (left.BaseType == null || right.BaseType == null) return false;

        if (left.BaseType == right.BaseType) {
            if (!requireOrdering) return true;
            return IsNumeric(left) || left.BaseType == ColumnType.String;
        }

        if (IsNumeric(left) && IsNumeric(right)) {
            return true;
        }
        return false;
    }

    private bool IsNumeric(ExpressionType type) {
        if (type.Kind != TypeKind.Scalar || type.BaseType == null) return false;
        return type.BaseType == ColumnType.Int || type.BaseType == ColumnType.Float;
    }

    private bool IsNullable(ColumnDefinition colDef) {
        if (colDef == null) return true;
        return colDef.ColumnConstraints == null || !colDef.ColumnConstraints.Any(c => c.Type == ColumnConstraintType.NotNull);
    }
}