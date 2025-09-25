using LiteDatabase.Sql.Ast;
using LiteDatabase.Sql.Ast.Expressions;

namespace LiteDatabase.Sql;

class Executor : IVisitor {
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

    // Expression visitors
    public void Visit(BetweenExpression node) {
        // TODO: Implement BETWEEN execution
    }

    public void Visit(BinaryExpression node) {
        // TODO: Implement binary operations (+, -, *, /, =, <>, etc.)
    }

    public void Visit(ColumnRefExpression node) {
        // TODO: Implement column value retrieval
    }

    public void Visit(FunctionCallExpression node) {
        // 使用字符串匹配而不是枚举
        switch (node.FunctionName.ToUpper()) {
            case "COUNT":
                ExecuteCount(node.Arguments);
                break;
            case "SUM":
                ExecuteSum(node.Arguments);
                break;
            case "AVG":
                ExecuteAverage(node.Arguments);
                break;
            case "MIN":
                ExecuteMin(node.Arguments);
                break;
            case "MAX":
                ExecuteMax(node.Arguments);
                break;
            default:
                throw new Exception($"Unknown function: {node.FunctionName}");
        }
    }

    public void Visit(InExpression node) {
        // TODO: Implement IN execution
    }

    public void Visit(LiteralExpression node) {
        // TODO: Return literal value
    }

    public void Visit(StarExpression node) {
        // TODO: Handle SELECT *
    }

    public void Visit(SubqueryExpression node) {
        // TODO: Execute subquery
    }

    public void Visit(UnaryExpression node) {
        // TODO: Implement unary operations (NOT, -, etc.)
    }

    // 函数执行的具体实现
    private void ExecuteCount(List<Expression> arguments) {
        // COUNT 的实际计算逻辑
        // 如果是 COUNT(*)，统计所有行
        // 如果是 COUNT(column)，统计非 NULL 值
    }

    private void ExecuteSum(List<Expression> arguments) {
        // SUM 的实际计算逻辑
    }

    private void ExecuteAverage(List<Expression> arguments) {
        // AVG 的实际计算逻辑
    }

    private void ExecuteMin(List<Expression> arguments) {
        // MIN 的实际计算逻辑
    }

    private void ExecuteMax(List<Expression> arguments) {
        // MAX 的实际计算逻辑
    }

}