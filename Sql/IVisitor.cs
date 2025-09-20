using LiteDatabase.Sql.Ast;
using LiteDatabase.Sql.Ast.Expressions;

public interface IVisitor {
    void Visit(InsertNode node);
    void Visit(UpdateNode node);
    void Visit(DeleteNode node);
    void Visit(DropTableNode node);
    void Visit(CreateTableNode node);
    void Visit(SelectNode node);
    void Visit(BetweenExpression node);
    void Visit(BinaryExpression node);
    void Visit(ColumnRefExpression node);
    void Visit(FunctionCallExpression node);
    void Visit(InExpression node);
    void Visit(LiteralExpression node);
    void Visit(StarExpression node);
    void Visit(SubqueryExpression node);
    void Visit(UnaryExpression node);
}