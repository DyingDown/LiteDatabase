namespace LiteDatabase.Sql.Ast.Expressions;

/// <summary>
/// 表示 IN 表达式，例如: id IN (1, 2, 3) 或 id IN (SELECT id FROM table)
/// </summary>
public class InExpression : Expression {
    public Expression Expression { get; }
    public List<Expression> Values { get; }
    
    public InExpression(Expression expression, List<Expression> values) {
        Expression = expression;
        Values = values;
    }
    
    public override string ToString() {
        var valuesList = string.Join(", ", Values.Select(v => v.ToString()));
        return $"{Expression} IN ({valuesList})";
    }
}