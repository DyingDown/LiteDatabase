namespace LiteDatabase.Sql.Ast.Expressions;

public class BinaryExpression : Expression {
    public Expression Left { get; }
    public BinaryOperatorType Operator { get; }
    public Expression Right { get; }
    public BinaryExpression(Expression left, BinaryOperatorType Operator, Expression right) {
        Left = left;
        this.Operator = Operator;
        Right = right;
    }

    public override string ToString() => $"({Left} {Operator.ToSqlString()} {Right})";
}

