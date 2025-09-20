namespace LiteDatabase.Sql.Ast;

public class DropTableNode : SqlNode {
    public List<string> TableNameList { get; set; } = new();

    public override void Accept(IVisitor visitor) => visitor.Visit(this);

    public override string ToString() => $"DROP TABLE {string.Join(", ", TableNameList)}";
    
}