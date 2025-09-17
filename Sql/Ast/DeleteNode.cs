using LiteDatabase.Sql.Ast.Expressions;
namespace LiteDatabase.Sql.Ast;

public class DeleteNode : SqlNode {
    public string TableName { get; set; } = "";
    public Expression? WhereClause { get; set; } = null;

    public override string ToString() {
        var whereStr = WhereClause != null ? $" WHERE {WhereClause}" : "";
        return $"DELETE FROM {TableName}{whereStr}";
    }
}