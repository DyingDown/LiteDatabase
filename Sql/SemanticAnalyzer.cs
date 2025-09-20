using LiteDatabase.Catalog;
using LiteDatabase.Sql.Ast;
using LiteDatabase.Sql.Ast.Expressions;

namespace LiteDatabase.Sql;

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
        foreach (var table in node.TableNamesWithAlias) {
            if (!catalog.TableExists(table.Item1))
                throw new Exception($"Table '{table.Item1}' does not exist.");

            _currentTableSchemas[table.Item1] = catalog.GetTableColumns(table.Item1);
        }

        // Step 2. 检查 SELECT 的列
        foreach (var selectItem in node.SelectList) {
            if (selectItem.IsStar) continue; // 全列选择

            // 使用访问者模式检查表达式
            selectItem.Expr?.Accept(this);
        }
        
        // Step 3. 检查 WHERE 子句（如果有的话）
        if (node.WhereClause != null) {
            node.WhereClause.Accept(this);
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
        // 创建类型推断器（需要当前上下文的表信息）
        var inferrer = CreateTypeInferrer();
        
        // 推断各个表达式的类型
        var exprType = inferrer.InferType(node.Expression);
        var lowerType = inferrer.InferType(node.LowerBound);
        var upperType = inferrer.InferType(node.UpperBound);

        // 检查类型兼容性
        if (!inferrer.CanCompare(exprType, lowerType) || !inferrer.CanCompare(exprType, upperType)) {
            throw new Exception($"Type mismatch in BETWEEN expression: cannot compare {exprType} with {lowerType} and {upperType}");
        }
    }
    public void Visit(BinaryExpression node) {
        // 递归检查子表达式
        node.Left.Accept(this);
        node.Right.Accept(this);
        
        // 使用类型推断器检查类型兼容性
        var inferrer = CreateTypeInferrer();
        try {
            inferrer.InferType(node); // 如果能推断出类型，说明表达式有效
        } catch (Exception ex) {
            throw new Exception($"Invalid binary expression: {ex.Message}");
        }
    }
    
    public void Visit(ColumnRefExpression node) {
        // 列引用的检查在 SelectNode 中已经处理
        // 这里可以添加额外的列引用验证逻辑
    }
    
    public void Visit(FunctionCallExpression node) {
        // 检查函数参数
        foreach (var arg in node.Arguments) {
            arg.Accept(this);
        }
        
        // 可以添加函数特定的验证逻辑
        // 比如检查参数数量是否正确等
    }
    
    public void Visit(InExpression node) {
        // 检查 IN 表达式的类型兼容性
        node.Expression.Accept(this);
        
        var inferrer = CreateTypeInferrer();
        var exprType = inferrer.InferType(node.Expression);
        
        foreach (var value in node.Values) {
            value.Accept(this);
            var valueType = inferrer.InferType(value);
            if (!inferrer.CanCompare(exprType, valueType)) {
                throw new Exception($"Type mismatch in IN expression: cannot compare {exprType} with {valueType}");
            }
        }
    }
    
    public void Visit(LiteralExpression node) {
        // 字面量不需要特殊检查
    }
    
    public void Visit(StarExpression node) {
        // * 表达式不需要特殊检查
    }
    
    public void Visit(SubqueryExpression node) {
        // 递归检查子查询
        node.Subquery.Accept(this);
    }
    
    public void Visit(UnaryExpression node) {
        // 检查操作数
        node.Operand.Accept(this);
        
        var inferrer = CreateTypeInferrer();
        try {
            inferrer.InferType(node);
        } catch (Exception ex) {
            throw new Exception($"Invalid unary expression: {ex.Message}");
        }
    }

}