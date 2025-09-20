namespace LiteDatabase.Sql.Ast.Expressions;

public class UnaryExpression : Expression {
    
    public UnaryOperatorType Operator { get; }
    public Expression Operand { get; }

    public UnaryExpression(UnaryOperatorType op, Expression operand) {
        Operator = op;
        Operand = operand;
    }

    public override void Accept(IVisitor visitor) => visitor.Visit(this);
    
    public override string ToString() => $"{OperatorToSql(Operator)} {Operand}";

    private string OperatorToSql(UnaryOperatorType op) => op switch {
        UnaryOperatorType.Not => "NOT",
        UnaryOperatorType.Plus => "+",
        UnaryOperatorType.Minus => "-",
        _ => throw new NotImplementedException()
    };
}