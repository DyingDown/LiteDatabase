using LiteDatabase.Sql.Ast;

namespace LiteDatabase.Catalog;

public class TableSchema {
    public int TableId { get; }
    public string TableName { get; }
    public Dictionary<string, ColumnDefinition> Columns { get; }

    public TableSchema(int tableId, string tableName, IEnumerable<ColumnDefinition> columns) {
        TableId = tableId;
        TableName = tableName;
        Columns = new Dictionary<string, ColumnDefinition>(StringComparer.OrdinalIgnoreCase);

        foreach (var col in columns) {
            Columns[col.ColumnName] = col;
        }
    }
}
