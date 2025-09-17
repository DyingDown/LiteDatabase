using LiteDatabase.Sql.Ast.Expressions;
namespace LiteDatabase.Sql.Ast;

public class DeleteNode : SqlNode {
    public string TableName { get; set; } = "";
    public Expression? Expression { get; set; } = null;

    public override string ToString() {
        var whereStr = Expression != null ? $" WHERE {Expression}" : "";
        return $"DELETE FROM {TableName}{whereStr}";
    }
}