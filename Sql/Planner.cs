using LiteDatabase.Sql.Ast;
using LiteDatabase.Sql.Ast.Expressions;

namespace LiteDatabase.Sql;

class Planner : IVisitor {

    public void Visit(InsertNode node) {

    }

    public void Visit(UpdateNode node) {

    }

    public void Visit(DeleteNode node) {

    }

    public void Visit(DropTableNode node) {

    }

    public void Visit(CreateTableNode node) {

    }

    public void Visit(SelectNode node) {

    }

    // Expression visitors (empty implementations for now)
    public void Visit(BetweenExpression node) { }
    public void Visit(BinaryExpression node) { }
    public void Visit(ColumnRefExpression node) { }
    public void Visit(FunctionCallExpression node) { }
    public void Visit(InExpression node) { }
    public void Visit(LiteralExpression node) { }
    public void Visit(StarExpression node) { }
    public void Visit(SubqueryExpression node) { }
    public void Visit(UnaryExpression node) { }

}