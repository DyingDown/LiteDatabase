namespace LiteDatabase.Sql.Ast;

public abstract class SqlNode {
    public abstract override string ToString();

    public abstract void Accept(IVisitor visitor);
}
