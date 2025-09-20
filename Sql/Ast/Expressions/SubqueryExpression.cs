namespace LiteDatabase.Sql.Ast.Expressions;

public class SubqueryExpression : Expression {
    public SelectNode Subquery { get; }
    public SubqueryExpression(SelectNode subquery) {
        Subquery = subquery;
    }

    public override void Accept(IVisitor visitor) => visitor.Visit(this);
    
    public override string ToString() => $"({Subquery})";
}