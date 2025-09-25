using LiteDatabase.Catalog;
using LiteDatabase.Sql.Ast;
using LiteDatabase.Sql.Ast.Expressions;

namespace LiteDatabase.Sql.Semantic;

class SemanticAnalyzer : IVisitor {
    private readonly ICatalogManager catalog;
    // 当前上下文中的表信息，用于类型推断
    private Dictionary<string, Dictionary<string, ColumnDefinition>> _currentTableSchemas = new();

    public SemanticAnalyzer(ICatalogManager catalogManager) {
        catalog = catalogManager;
    }

    /// <summary>
    /// 创建类型推断器
    /// </summary>
    private ExpressionTypeInferrer CreateTypeInferrer() {
        return new ExpressionTypeInferrer(catalog, _currentTableSchemas);
    }

    public void Visit(InsertNode node) {
        if (!catalog.TableExists(node.TableName)) {
            throw new Exception($"Table '{node.TableName}' does not exist.");
        }

        var columns = catalog.GetTableColumns(node.TableName);

        foreach (var valueList in node.Values) {
            List<string> colNames = node.ColumnNames?.Count > 0 ? node.ColumnNames : columns.Select(c => c.Key).ToList();
            if (valueList.Count != colNames.Count)
                throw new Exception($"Column count mismatch in INSERT INTO {node.TableName}.");

            for (int i = 0; i < colNames.Count; i++) {
                var colName = colNames[i];
                if (!columns.TryGetValue(colName, out var colSchema))
                    throw new Exception($"Column '{colName}' does not exist in table '{node.TableName}'.");
                if (!TypeSystem.IsCompatible(valueList[i].Type, colSchema.ColumnType))
                    throw new Exception($"Type mismatch for column '{colName}' in table '{node.TableName}'. Expected {colSchema.ColumnType}, got {valueList[i].Type}.");
            }
        }
    }

    public void Visit(UpdateNode node) { // TODO:
        if (!catalog.TableExists(node.TableName))
            throw new Exception($"Table '{node.TableName}' does not exist.");

        var columns = catalog.GetTableColumns(node.TableName);

        foreach (var kv in node.Assigns) {
            if (columns.All(c => !c.Key.Equals(kv.ColumnName, StringComparison.OrdinalIgnoreCase)))
                throw new Exception($"Column '{kv.ColumnName}' does not exist in table '{node.TableName}'.");
        }
    }

    public void Visit(DeleteNode node) {
        if (!catalog.TableExists(node.TableName))
            throw new Exception($"Table '{node.TableName}' does not exist.");
    }

    public void Visit(DropTableNode node) {
        foreach (var tableName in node.TableNameList) {
            if (!catalog.TableExists(tableName)) {
                throw new Exception($"Table '{tableName}' does not exist.");
            }
        }
    }

    public void Visit(CreateTableNode node) {
        if (catalog.TableExists(node.TableName)) {
            throw new Exception($"Table '{node.TableName}' already exists.");
        }
        var colNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var col in node.Columns) {
            if (!colNames.Add(col.ColumnName)) {
                throw new Exception($"Duplicate column name '{col.ColumnName}' in table '{node.TableName}'.");
            }
        }
    }

    public void Visit(SelectNode node) {
        // Step 1. 检查 FROM 表是否存在，并设置当前表上下文
        _currentTableSchemas.Clear();
        var usedAliases = new HashSet<string>();
        
        foreach (var (alias, tableName) in node.TableNamesWithAlias) {
            // Check for duplicate aliases
            if (usedAliases.Contains(alias)) {
                throw new Exception($"Duplicate table alias '{alias}' in FROM clause");
            }
            usedAliases.Add(alias);
            
            // Verify table exists
            if (!catalog.TableExists(tableName)) {
                throw new Exception($"Table '{tableName}' does not exist.");
            }

            // 关键修复：只添加别名到 schema，不添加原表名
            // 如果 alias == tableName，说明没有设置别名，使用原表名
            // 如果 alias != tableName，说明设置了别名，只能通过别名访问
            _currentTableSchemas[alias] = catalog.GetTableColumns(tableName);
        }

        // Step 2. 检查 SELECT 的列
        foreach (var selectItem in node.SelectList) {
            if (selectItem.IsStar) {
                // 处理 * 或 table.* 的情况
                if (selectItem.Expr is StarExpression starExpr && !string.IsNullOrEmpty(starExpr.TableName)) {
                    // table.* 情况：验证表名是否存在于 FROM 子句中
                    if (!_currentTableSchemas.ContainsKey(starExpr.TableName)) {
                        throw new Exception($"Table '{starExpr.TableName}' in '{starExpr.TableName}.*' not found in FROM clause");
                    }
                }
                // 纯 * 情况：无需额外验证，只要有表在 FROM 中即可
                continue;
            }

            if (selectItem.Expr != null) {
                // 先用访问者模式检查语义合法性
                selectItem.Expr.Accept(this);
                
                // 再检查类型约束
                var inferrer = CreateTypeInferrer();
                try {
                    var exprType = inferrer.InferType(selectItem.Expr);
                    if (exprType.Kind != TypeKind.Scalar) {
                        throw new Exception($"SELECT list expression must be scalar, got {exprType.Kind}");
                    }
                } catch (Exception ex) {
                    throw new Exception($"Invalid SELECT list expression type: {ex.Message}");
                }
            }
        }
        
        // Step 3. 检查 WHERE 子句（如果有的话）
        if (node.WhereClause != null) {
            // 先检查语义合法性
            node.WhereClause.Accept(this);
            
            // 再检查类型约束
            var inferrer = CreateTypeInferrer();
            try {
                var whereType = inferrer.InferType(node.WhereClause);
                if (whereType.Kind != TypeKind.Scalar || whereType.BaseType != ColumnType.Bool) {
                    throw new Exception($"WHERE clause must be boolean expression, got {whereType}");
                }
            } catch (Exception ex) {
                throw new Exception($"Invalid WHERE clause type: {ex.Message}");
            }
        }
        
        // Step 4. 检查 GROUP BY 子句
        foreach (var groupCol in node.GroupByColumns) {
            if (!string.IsNullOrEmpty(groupCol.TableName)) {
                if (!_currentTableSchemas.ContainsKey(groupCol.TableName))
                    throw new Exception($"Table '{groupCol.TableName}' not found in FROM clause.");

                if (!_currentTableSchemas[groupCol.TableName].ContainsKey(groupCol.ColumnName))
                    throw new Exception($"Column '{groupCol.ColumnName}' does not exist in table '{groupCol.TableName}'.");
            } else {
                var matches = _currentTableSchemas
                    .Where(ts => ts.Value.ContainsKey(groupCol.ColumnName))
                    .ToList();

                if (matches.Count == 0)
                    throw new Exception($"Column '{groupCol.ColumnName}' does not exist in any table in FROM clause.");

                if (matches.Count > 1)
                    throw new Exception($"Ambiguous column '{groupCol.ColumnName}' found in multiple tables.");
            }
        }
        
        // Step 5. 检查 ORDER BY 子句
        foreach (var orderItem in node.OrderItems) {
            var orderCol = orderItem.ColumnRef;
            if (!string.IsNullOrEmpty(orderCol.TableName)) {
                if (!_currentTableSchemas.ContainsKey(orderCol.TableName))
                    throw new Exception($"Table '{orderCol.TableName}' not found in FROM clause.");

                if (!_currentTableSchemas[orderCol.TableName].ContainsKey(orderCol.ColumnName))
                    throw new Exception($"Column '{orderCol.ColumnName}' does not exist in table '{orderCol.TableName}'.");
            } else {
                var matches = _currentTableSchemas
                    .Where(ts => ts.Value.ContainsKey(orderCol.ColumnName))
                    .ToList();

                if (matches.Count == 0)
                    throw new Exception($"Column '{orderCol.ColumnName}' does not exist in any table in FROM clause.");

                if (matches.Count > 1)
                    throw new Exception($"Ambiguous column '{orderCol.ColumnName}' found in multiple tables.");
            }
        }
    }


    public void Visit(BetweenExpression node) {
        // 检查三个表达式的语义合法性
        node.Expression.Accept(this);
        node.LowerBound.Accept(this);
        node.UpperBound.Accept(this);
        
        // 在上层进行类型兼容性检查
        var inferrer = CreateTypeInferrer();
        var exprType = inferrer.InferType(node.Expression);
        var lowerType = inferrer.InferType(node.LowerBound);
        var upperType = inferrer.InferType(node.UpperBound);

        // 检查是否有未知类型
        if (exprType.Kind == TypeKind.Unknown || lowerType.Kind == TypeKind.Unknown || upperType.Kind == TypeKind.Unknown) {
            throw new Exception("BETWEEN operands or bounds have unknown type");
        }
        
        // 处理子查询边界
        if (lowerType.Kind == TypeKind.RowSet) {
            if (!IsSubqueryGuaranteedSingleRow((SubqueryExpression) node.LowerBound)) {
                throw new Exception("BETWEEN lower bound must be a single aggregate/function call subquery");
            }
            if (lowerType.RowSchema == null || lowerType.RowSchema.Count != 1) {
                throw new Exception("BETWEEN lower bound subquery must return exactly one column");
            }
            lowerType = lowerType.RowSchema[0];
        }
        
        if (upperType.Kind == TypeKind.RowSet) {
            if (!IsSubqueryGuaranteedSingleRow((SubqueryExpression) node.UpperBound)) {
                throw new Exception("BETWEEN upper bound must be a single aggregate/function call subquery");
            }
            if (upperType.RowSchema == null || upperType.RowSchema.Count != 1) {
                throw new Exception("BETWEEN upper bound subquery must return exactly one column");
            }
            upperType = upperType.RowSchema[0];
        }
        
        // 检查所有操作数必须是标量
        if (exprType.Kind != TypeKind.Scalar || lowerType.Kind != TypeKind.Scalar || upperType.Kind != TypeKind.Scalar) {
            throw new Exception("BETWEEN operands must be scalar expressions");
        }
        
        // 检查类型兼容性
        if (!CanCompareTypes(lowerType, upperType, requireOrdering: true)) {
            throw new Exception($"BETWEEN lower/upper bounds are not comparable: {lowerType} vs {upperType}");
        }
        if (!CanCompareTypes(exprType, lowerType, requireOrdering: true) || !CanCompareTypes(exprType, upperType, requireOrdering: true)) {
            throw new Exception($"BETWEEN operands are not comparable: {exprType} vs {lowerType}/{upperType}");
        }
        
        // 如果两个边界都是字面量，检查下界 <= 上界
        if (node.LowerBound is LiteralExpression lowLit && node.UpperBound is LiteralExpression upperLit) {
            if (lowLit.Value != null && upperLit.Value != null) {
                var cmpResult = 0;
                try {
                    cmpResult = Comparer<object>.Default.Compare(lowLit.Value, upperLit.Value);
                } catch {
                    throw new Exception($"BETWEEN lower/upper bounds cannot be compared: {lowLit.Value} vs {upperLit.Value}");
                }
                if (cmpResult > 0) {
                    throw new Exception($"BETWEEN lower bound ({lowLit.Value}) must be <= upper bound ({upperLit.Value})");
                }
            }
        }
    }
    
    /// <summary>
    /// 检查子查询是否保证单行（用于标量子查询验证）
    /// </summary>
    private bool IsSubqueryGuaranteedSingleRow(SubqueryExpression sub) {
        var node = sub.Subquery;
        // 单列且(LIMIT 1 或为聚合函数)即保证单值
        if (node.SelectList.Count == 1 && !node.SelectList[0].IsStar) {
            if ((node.Limit.HasValue && node.Limit.Value == 1) || node.SelectList[0].Expr is FunctionCallExpression)
                return true;
        }
        return false;
    }
    
    public void Visit(BinaryExpression node) {
        node.Left.Accept(this);
        node.Right.Accept(this);
        
        var inferrer = CreateTypeInferrer();
        var leftType = inferrer.InferType(node.Left);
        var rightType = inferrer.InferType(node.Right);

        // 检查是否有未知类型
        if (leftType.Kind == TypeKind.Unknown || rightType.Kind == TypeKind.Unknown) {
            throw new Exception("Binary operands have unknown type");
        }
        
        // 检查操作数必须是标量
        if (leftType.Kind != TypeKind.Scalar || rightType.Kind != TypeKind.Scalar) {
            throw new Exception($"Binary operator '{node.Operator}' requires scalar operands, got {leftType.Kind} and {rightType.Kind}");
        }
        
        switch (node.Operator) {
            case BinaryOperatorType.Equal:
            case BinaryOperatorType.NotEqual:
                // 只要求类型一致，不需要排序能力
                if (leftType.BaseType != rightType.BaseType) {
                    throw new Exception($"Cannot compare types {leftType} and {rightType} with operator {node.Operator}");
                }
                break;
                
            case BinaryOperatorType.LessOrEqual:
            case BinaryOperatorType.LessThan:
            case BinaryOperatorType.GreaterOrEqual:
            case BinaryOperatorType.GreaterThan:
                if (!CanCompareTypes(leftType, rightType, requireOrdering: true)) {
                    throw new Exception($"Cannot compare types {leftType} and {rightType} with operator {node.Operator}");
                }
                break;
                
            case BinaryOperatorType.And:
            case BinaryOperatorType.Or:
                if (leftType.BaseType != ColumnType.Bool || rightType.BaseType != ColumnType.Bool) {
                    throw new Exception($"Logical operator '{node.Operator}' requires boolean operands, got {leftType} and {rightType}");
                }
                break;
                
            case BinaryOperatorType.Add:
            case BinaryOperatorType.Multiply:
            case BinaryOperatorType.Subtract:
            case BinaryOperatorType.Divide:
                if (!IsNumericType(leftType.BaseType) || !IsNumericType(rightType.BaseType)) {
                    throw new Exception($"Arithmetic operator '{node.Operator}' requires numeric operands, got {leftType} and {rightType}");
                }
                break;
                
            default:
                throw new Exception($"Unknown binary operator: {node.Operator}");
        }
    }
    
    public void Visit(ColumnRefExpression node) {
        // 验证列引用是否存在且可访问
        if (!string.IsNullOrEmpty(node.TableName)) {
            // 如果指定了表名/别名，检查是否存在于当前上下文
            if (!_currentTableSchemas.ContainsKey(node.TableName)) {
                throw new Exception($"Table or alias '{node.TableName}' not found in FROM clause.");
            }
            
            // 检查该表/别名中是否存在指定的列
            if (!_currentTableSchemas[node.TableName].ContainsKey(node.ColumnName)) {
                throw new Exception($"Column '{node.ColumnName}' does not exist in table '{node.TableName}'.");
            }
        } else {
            // 如果没有指定表名，需要在所有表中查找该列
            var matches = _currentTableSchemas
                .Where(ts => ts.Value.ContainsKey(node.ColumnName))
                .ToList();

            if (matches.Count == 0) {
                throw new Exception($"Column '{node.ColumnName}' does not exist in any table in FROM clause.");
            }

            if (matches.Count > 1) {
                throw new Exception($"Ambiguous column '{node.ColumnName}' found in multiple tables. Please specify table name/alias.");
            }
        }
    }
    
    public void Visit(FunctionCallExpression node) {
        // 1. 检查函数调用的语义合法性（不涉及类型）
        ValidateFunctionSemantics(node);
        
        // 2. 递归检查所有参数的语义合法性
        for (int i = 0; i < node.Arguments.Count; i++) {
            node.Arguments[i].Accept(this);
        }
        
        // 3. 使用函数注册表进行完整的参数验证
        var functionDef = catalog.GetFunction(node.FunctionName);
        if (functionDef == null) {
            throw new Exception($"Unknown function: {node.FunctionName}");
        }
        
        // 4. 验证参数数量（考虑可选参数）
        var requiredParamCount = functionDef.Parameters.Count(p => !p.IsOptional);
        var totalParamCount = functionDef.Parameters.Count;
        var actualParamCount = node.Arguments.Count;
        
        // 特殊处理 COUNT(*) - 如果是 * 参数，跳过参数验证
        if (node.FunctionName.Equals("COUNT", StringComparison.OrdinalIgnoreCase) && 
            actualParamCount == 1 && node.Arguments[0] is StarExpression) {
            return; // COUNT(*) 是有效的，跳过后续验证
        }
        
        if (actualParamCount < requiredParamCount || actualParamCount > totalParamCount) {
            throw new Exception($"Function {node.FunctionName} expects {requiredParamCount}-{totalParamCount} argument(s), got {actualParamCount}");
        }
        
        // 5. 验证每个实际传入参数的类型
        var inferrer = CreateTypeInferrer();
        for (int i = 0; i < actualParamCount; i++) {
            var parameter = functionDef.Parameters[i];
            var argType = inferrer.InferType(node.Arguments[i]);
            
            // 检查函数参数的子查询约束
            if (argType.Kind == TypeKind.RowSet) {
                // 必须是保证单行的子查询
                if (!IsSubqueryGuaranteedSingleRow((SubqueryExpression) node.Arguments[i])) {
                    throw new Exception($"{node.FunctionName} argument {i + 1} subquery must be a scalar subquery (single row, single column)");
                }
                if (argType.RowSchema == null || argType.RowSchema.Count != 1) {
                    throw new Exception($"{node.FunctionName} argument {i + 1} subquery schema is invalid");
                }
                // 获取子查询的标量类型
                argType = argType.RowSchema[0];
            }
            
            if (argType.Kind != TypeKind.Scalar) {
                throw new Exception($"Function {node.FunctionName} argument {i + 1} must be scalar, got {argType.Kind}");
            }
            
            if (argType.BaseType == null) {
                throw new Exception($"Function {node.FunctionName} argument {i + 1} has unknown type");
            }
            
            if (!parameter.AcceptsType(argType.BaseType.Value)) {
                throw new Exception($"Function {node.FunctionName} argument {i + 1} type mismatch: expected {string.Join("|", parameter.AcceptedTypes)}, got {argType.BaseType}");
            }
        }
    }
    
    /// <summary>
    /// 验证函数语义合法性（不涉及类型）
    /// </summary>
    private void ValidateFunctionSemantics(FunctionCallExpression node) {
        var funcName = node.FunctionName;
        var argCount = node.Arguments.Count;
        
        // 使用函数注册表检查函数是否存在
        if (!catalog.FunctionExists(funcName)) {
            throw new Exception($"Unknown function: {funcName}");
        }
        
        // 检查特殊规则：COUNT(*) 是唯一允许使用 * 的函数
        if (argCount == 1 && node.Arguments[0] is StarExpression) {
            if (!funcName.Equals("COUNT", StringComparison.OrdinalIgnoreCase)) {
                throw new Exception($"Only COUNT(*) is allowed; {funcName}(*) is not supported");
            }
        }
    }
    
    public void Visit(InExpression node) {
        // 检查 IN 表达式的语义合法性
        node.Expression.Accept(this);
        
        // 在上层进行类型兼容性检查
        var inferrer = CreateTypeInferrer();
        var exprType = inferrer.InferType(node.Expression);
        if (exprType.Kind == TypeKind.Unknown) {
            throw new Exception("IN left operand type cannot be inferred or is not supported");
        }
        // 检查左操作数必须是标量
        if (exprType.Kind != TypeKind.Scalar) {
            throw new Exception("IN left operand must be a scalar expression");
        }
        // 检查右侧的每个值（递归语义检查+类型检查）
        foreach (var value in node.Values) {
            value.Accept(this);
            var valueType = inferrer.InferType(value);
            if (valueType.Kind == TypeKind.RowSet) {
                // 只可能是 SubqueryExpression
                if (valueType.RowSchema == null || valueType.RowSchema.Count != 1) {
                    throw new Exception("IN subquery must return exactly one column");
                }
                var rightType = valueType.RowSchema[0];
                if (rightType.Kind != TypeKind.Scalar) {
                    throw new Exception("IN subquery column must be scalar");
                }
                if (!CanCompareTypes(exprType, rightType)) {
                    throw new Exception($"IN operands are not comparable: {exprType} vs {rightType}");
                }
            }
            else if (valueType.Kind == TypeKind.Scalar) {
                // 值是标量表达式
                if (!CanCompareTypes(exprType, valueType)) {
                    throw new Exception($"IN operands are not comparable: {exprType} vs {valueType}");
                }
            }
            else {
                throw new Exception("IN values must be scalar expressions or single-column subqueries");
            }
        }
    }
    
    /// <summary>
    /// 检查两个类型是否可以比较（上层的类型兼容性逻辑）
    /// </summary>
    private bool CanCompareTypes(ExpressionType type1, ExpressionType type2, bool requireOrdering = false) {
        if (type1 == null || type2 == null) return false;
        if (type1.Kind == TypeKind.Unknown || type2.Kind == TypeKind.Unknown) return false;
        if (type1.Kind != TypeKind.Scalar || type2.Kind != TypeKind.Scalar)
            return false;
        if (type1.BaseType == null || type2.BaseType == null) return false;

        // 同类型可以比较
        if (type1.BaseType == type2.BaseType) {
            if (!requireOrdering) return true;
            // 如果需要排序，只有数值类型和字符串类型支持排序
            return IsNumericType(type1.BaseType) || type1.BaseType == ColumnType.String;
        }

        // 数值类型之间可以比较
        if (IsNumericType(type1.BaseType) && IsNumericType(type2.BaseType)) {
            return true;
        }

        return false;
    }
    
    private bool IsNumericType(ColumnType? type) {
        return type == ColumnType.Int || type == ColumnType.Float;
    }
    
    public void Visit(LiteralExpression node) {
        // 字面量不需要特殊检查
    }
    
    public void Visit(StarExpression node) {
        // * 表达式不需要验证
    }
    
    public void Visit(SubqueryExpression node) {
        // 子查询验证：保存当前上下文，然后直接调用 Visit(SelectNode) 进行验证
        var originalSchemas = _currentTableSchemas;
        try {
            // 直接调用 SelectNode 的验证逻辑，它会处理所有的验证
            node.Subquery.Accept(this);
        } finally {
            // 恢复原有的表上下文
            _currentTableSchemas = originalSchemas;
        }
    }
    
    public void Visit(UnaryExpression node) {
        // 检查操作数的语义合法性
        node.Operand.Accept(this);
        
        // 在上层进行类型检查
        var inferrer = CreateTypeInferrer();
        var operandType = inferrer.InferType(node.Operand);
        
        // 检查操作数必须是标量
        if (operandType.Kind != TypeKind.Scalar) {
            throw new Exception($"Unary operator '{node.Operator}' requires scalar operand, got {operandType.Kind}");
        }
        
        switch (node.Operator) {
            case UnaryOperatorType.Not:
                // NOT 操作符需要布尔类型操作数
                if (operandType.BaseType != ColumnType.Bool) {
                    throw new Exception($"NOT operator requires boolean operand, got {operandType.BaseType}");
                }
                break;
                
            case UnaryOperatorType.Plus:
            case UnaryOperatorType.Minus:
                // + 和 - 操作符需要数值类型操作数
                if (!IsNumericType(operandType.BaseType)) {
                    throw new Exception($"Unary {node.Operator} operator requires numeric operand, got {operandType.BaseType}");
                }
                break;
                
            default:
                throw new Exception($"Unknown unary operator: {node.Operator}");
        }
    }

}