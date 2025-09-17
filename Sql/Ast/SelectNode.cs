using LiteDatabase.Sql.Ast.Expressions;
namespace LiteDatabase.Sql.Ast;

public class SelectNode : SqlNode {
    public List<SelectItem> SelectList { get; set; } = [];
    public List<string> TableNames { get; set; } = [];
    public Expression? WhereClause { get; set; } = null;

    public List<string> GroupByColumns { get; set; } = [];
    public List<OrderItem> OrderItems { get; set; } = [];

    public override string ToString()
    {
        return $"""
SELECT
  {string.Join(", ", SelectList.Select(item =>
    item.IsStar ? "*" : $"{item.TableName}.{item.ColumnName}" + (string.IsNullOrEmpty(item.AliasName) ? "" : $" AS {item.AliasName}")
  ))}
FROM {string.Join(", ", TableNames)}
{(WhereClause != null ? $"WHERE {WhereClause}" : "")}
{(GroupByColumns.Count > 0 ? $"GROUP BY {string.Join(", ", GroupByColumns)}" : "")}
{(OrderItems.Count > 0 ? $"ORDER BY {string.Join(", ", OrderItems.Select(o => $"{o.ColumnName} {o.OrderType}"))}" : "")}
""".Trim();
    }
}

public class SelectItem {
    public string? TableName { get; }
    public string ColumnName { get; }
    public string? AliasName { get; }
    public bool IsStar { get; }

    public SelectItem(string columnName, bool isStar, string? tableName = "", string? aliasName = "") {
        TableName = tableName;
        ColumnName = columnName;
        AliasName = aliasName;
        IsStar = isStar;
    }

}

public class OrderItem {
    public string ColumnName { get; }
    public OrderType OrderType { get; }
    public OrderItem(string name, OrderType type) {
        ColumnName = name;
        OrderType = type;
    }
}

public enum OrderType {
    ASC,
    DESC,
}