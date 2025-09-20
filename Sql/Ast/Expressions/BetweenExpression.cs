namespace LiteDatabase.Sql.Ast.Expressions;

/// <summary>
/// 表示 BETWEEN 表达式，例如: age BETWEEN 18 AND 65
/// </summary>
public class BetweenExpression : Expression {
    public Expression Expression { get; }
    public Expression LowerBound { get; }
    public Expression UpperBound { get; }
    
    public BetweenExpression(Expression expression, Expression lowerBound, Expression upperBound) {
        Expression = expression;
        LowerBound = lowerBound;
        UpperBound = upperBound;
    }
    
    public override void Accept(IVisitor visitor) => visitor.Visit(this);
    
    public override string ToString() {
        return $"{Expression} BETWEEN {LowerBound} AND {UpperBound}";
    }
}