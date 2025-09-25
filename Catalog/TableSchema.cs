using LiteDatabase.Sql.Ast;

namespace LiteDatabase.Catalog;

public class TableSchema {
    public int TableId { get; }
    public string TableName { get; }

    public uint FirstPage { get; set; }

    public uint LastPage { get; set; }

    public int PrimaryKey { get; set; }
    
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
