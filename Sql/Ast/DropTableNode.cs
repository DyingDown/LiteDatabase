namespace LiteDatabase.Sql.Ast;

public class DropTableNode : SqlNode {
    public List<string> TableNameList { get; set; } = new();

    public override string ToString() => $"DROP TABLE {string.Join(", ", TableNameList)}";
    
}