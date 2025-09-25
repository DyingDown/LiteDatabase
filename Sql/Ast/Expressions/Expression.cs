using LiteDatabase.Sql.Semantic;

namespace LiteDatabase.Sql.Ast.Expressions;


public abstract class Expression {
    public ExpressionType? InferredType { get; set; }

    public abstract override string ToString();

    public abstract void Accept(IVisitor visitor);
}