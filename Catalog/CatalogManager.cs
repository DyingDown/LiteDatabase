using LiteDatabase.Sql.Ast;

namespace LiteDatabase.Catalog;

public class CatalogManager : ICatalogManager {
    // 表名 -> 表ID
    private readonly Dictionary<string, int> tableNameToId = new(StringComparer.OrdinalIgnoreCase);

    // 表ID -> 表名
    private readonly Dictionary<int, string> tableIdToName = new();

    // 表名 -> 列定义
    private readonly Dictionary<string, TableSchema> tables = new(StringComparer.OrdinalIgnoreCase);
    
    // 函数名 -> 函数定义
    private readonly Dictionary<string, FunctionDefinition> functions = new(StringComparer.OrdinalIgnoreCase);

    private int nextTableId = 1;
    
    public CatalogManager() {
        // 注册内置函数
        RegisterBuiltinFunctions();
    }

    // 表结构管理
    public void CreateTable(string tableName, List<ColumnDefinition> columns) {
        if (tableNameToId.ContainsKey(tableName))
            throw new Exception($"Table '{tableName}' already exists.");

        int id = nextTableId++;
        tableNameToId[tableName] = id;
        tableIdToName[id] = tableName;
        tables[tableName] = new TableSchema(id, tableName, columns); ;
    }

    public void DropTable(string tableName) {
        if (!tableNameToId.ContainsKey(tableName)) {
            throw new Exception($"Table '{tableName}' does not exist.");
        }
        int id = tableNameToId[tableName];
        tableIdToName.Remove(id);
        tableNameToId.Remove(tableName);
        tables.Remove(tableName);
    }

    public bool TableExists(string tableName) {
        return tableNameToId.ContainsKey(tableName);
    }
    public Dictionary<string, ColumnDefinition> GetTableColumns(string tableName) {
        if (!tableNameToId.ContainsKey(tableName)) {
            throw new Exception($"Table '{tableName}' does not exist.");
        }
        return tables[tableName].Columns;
    }

    public TableSchema GetTable(string tableName) {
        
        if (!tableNameToId.ContainsKey(tableName)) {
            throw new Exception($"Table '{tableName}' does not exist.");
        }
        return tables[tableName];
    }

    // 表ID管理
    public int GetTableId(string tableName) {

        if (!tableNameToId.ContainsKey(tableName)) {
            throw new Exception($"Table '{tableName}' does not exist.");
        }
        return tableNameToId[tableName];
    }
    public string GetTableName(int tableId) {
        
        if (!tableIdToName.ContainsKey(tableId)) {
            throw new Exception($"Table with ID '{tableId}' does not exist.");
        }
        return tableIdToName[tableId];
    }
    
    // 函数管理
    public bool FunctionExists(string functionName) {
        return functions.ContainsKey(functionName);
    }
    
    public FunctionDefinition? GetFunction(string functionName) {
        return functions.TryGetValue(functionName, out var function) ? function : null;
    }
    
    public void RegisterFunction(FunctionDefinition function) {
        functions[function.Name] = function;
    }
    
    public IEnumerable<FunctionDefinition> GetAllFunctions() {
        return functions.Values;
    }
    
    /// <summary>
    /// 注册内置函数
    /// </summary>
    private void RegisterBuiltinFunctions() {
        // COUNT - 特殊函数，接受任何类型和 * 参数
        RegisterFunction(FunctionDefinition.CreateCountFunction());
        
        // SUM - 接受数值类型，返回数值类型
        RegisterFunction(new FunctionDefinition(
            "SUM", 
            FunctionParameter.NumericType("expression"), 
            Sql.Ast.ColumnType.Int, 
            isAggregate: true
        ));
        
        // AVG - 接受数值类型，返回 Float（平均值通常是浮点数）
        RegisterFunction(new FunctionDefinition(
            "AVG", 
            FunctionParameter.NumericType("expression"), 
            Sql.Ast.ColumnType.Float, 
            isAggregate: true
        ));
        
        // MIN - 接受可比较类型，返回输入类型
        RegisterFunction(new FunctionDefinition(
            "MIN", 
            FunctionParameter.ComparableType("expression"), 
            Sql.Ast.ColumnType.Int, 
            isAggregate: true
        ));
        
        // MAX - 接受可比较类型，返回输入类型
        RegisterFunction(new FunctionDefinition(
            "MAX", 
            FunctionParameter.ComparableType("expression"), 
            Sql.Ast.ColumnType.Int, 
            isAggregate: true
        ));
    }
}