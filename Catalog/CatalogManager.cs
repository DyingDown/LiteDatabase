using LiteDatabase.Sql.Ast;

namespace LiteDatabase.Catalog;

public class CatalogManager : ICatalogManager {
    // 表名 -> 表ID
    private readonly Dictionary<string, int> tableNameToId = new(StringComparer.OrdinalIgnoreCase);

    // 表ID -> 表名
    private readonly Dictionary<int, string> tableIdToName = new();

    // 表名 -> 列定义
    private readonly Dictionary<string, TableSchema> tables = new(StringComparer.OrdinalIgnoreCase);

    private int nextTableId = 1;

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
}