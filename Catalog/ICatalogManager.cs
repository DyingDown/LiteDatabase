using LiteDatabase.Sql.Ast;

namespace LiteDatabase.Catalog;

public interface ICatalogManager {
    // 表结构管理
    void CreateTable(string tableName, List<ColumnDefinition> columns);
    void DropTable(string tableName);
    bool TableExists(string tableName);
    Dictionary<string, ColumnDefinition> GetTableColumns(string tableName);

    TableSchema GetTable(string TableName);

    // 表ID管理
    int GetTableId(string tableName);
    string GetTableName(int tableId);
    
    // 函数管理
    bool FunctionExists(string functionName);
    FunctionDefinition? GetFunction(string functionName);
    void RegisterFunction(FunctionDefinition function);
    IEnumerable<FunctionDefinition> GetAllFunctions();
}