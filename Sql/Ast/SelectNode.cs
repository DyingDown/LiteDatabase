using LiteDatabase.Sql.Ast.Expressions;
namespace LiteDatabase.Sql.Ast;

public class SelectNode : SqlNode {
    public List<SelectItem> SelectList { get; set; } = [];
    public List<(string, string)> TableNamesWithAlias { get; set; } = [];
    public Expression? WhereClause { get; set; } = null;

    public List<ColumnRefExpression> GroupByColumns { get; set; } = [];
    public List<OrderItem> OrderItems { get; set; } = [];

    public override void Accept(IVisitor visitor) => visitor.Visit(this);

    public override string ToString()
    {
        return $"""
SELECT
  {string.Join(", ", SelectList.Select(item =>
    item.IsStar ? "*" : $"{item.Expr}" + (string.IsNullOrEmpty(item.AliasName) ? "" : $" AS {item.AliasName}")
  ))}
FROM {string.Join(", ", TableNamesWithAlias.Select(t =>
    string.IsNullOrEmpty(t.Item2) ? t.Item1 : $"{t.Item1} AS {t.Item2}"
))}
{(WhereClause != null ? $"WHERE {WhereClause}" : "")}
{(GroupByColumns.Count > 0 ? $"GROUP BY {string.Join(", ", GroupByColumns)}" : "")}
{(OrderItems.Count > 0 ? $"ORDER BY {string.Join(", ", OrderItems.Select(o => $"{o.ColumnRef.ColumnName} {o.OrderType}"))}" : "")}
""".Trim();
    }
}

public class SelectItem {
    public Expression? Expr { get; }
    public string? AliasName { get; }
    public bool IsStar { get; }

    public SelectItem(bool isStar, Expression? expr, string? aliasName = "") {
        Expr = expr;
        AliasName = aliasName;
        IsStar = isStar;
    }

}

public class OrderItem {
    public ColumnRefExpression ColumnRef { get; }
    public OrderType OrderType { get; }
    public OrderItem(ColumnRefExpression columnRef, OrderType type) {
        ColumnRef = columnRef;
        OrderType = type;
    }
}

public enum OrderType {
    ASC,
    DESC,
}