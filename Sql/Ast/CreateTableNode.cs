namespace LiteDatabase.Sql.Ast;

public class CreateTableNode : SqlNode {
    public string TableName { get; set; } = "";

    public List<ColumnDefinition> Columns { get; set; } = new();

    public override void Accept(IVisitor visitor) => visitor.Visit(this);

    public override string ToString() => $"CREATE TABLE {TableName} (\n  {string.Join(",\n  ", Columns)}\n)";

}