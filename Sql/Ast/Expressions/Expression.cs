using LiteDatabase.Sql.Ast;

namespace LiteDatabase.Sql.Ast.Expressions;


public abstract class Expression {
    public ColumnType? InferredType { get; set; }

    public abstract override string ToString();

    public abstract void Accept(IVisitor visitor);
}