using System.Linq.Expressions;
using LiteDatabase.Sql.Ast;

namespace LiteDatabase.Table;

public interface ITableManager {
    // void CreateTable(string tableName, List<ColumnDefinition> columns);
    // void DropTable(string tableName);
    // bool TableExists(string tableName);

    // List<ColumnDefinition> GetTableColumns(string tableName);
    // ColumnDefinition? GetColumn(string tableName, string columnName);

    // void InsertRow(string tableName, Dictionary<string, object> values);
    // List<Dictionary<string, object>> SelectRows(string tableName, Expression? whereClause);
    // void UpdateRows(string tableName, Dictionary<string, object> updates, Expression? whereClause);
    // void DeleteRows(string tableName, Expression? whereClause);
}