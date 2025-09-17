namespace LiteDatabase.Sql.Ast.Expressions;

public class SubqueryExpression : Expression {
    public SelectNode Subquery { get; }
    public SubqueryExpression(SelectNode subquery) {
        Subquery = subquery;
    }
    public override string ToString() => $"({Subquery})";
}