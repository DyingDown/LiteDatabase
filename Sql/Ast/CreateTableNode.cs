namespace LiteDatabase.Sql.Ast;

public class CreateTableNode : SqlNode {
    public string TableName { get; set; } = "";

    public List<ColumnDefinitionNode> Columns { get; set; } = new();

    public override string ToString() => $"CREATE TABLE {TableName} (\n  {string.Join(",\n  ", Columns)}\n)";

}