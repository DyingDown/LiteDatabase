using LiteDatabase.Sql.Ast.Expressions;
namespace LiteDatabase.Sql.Ast;

public class UpdateNode : SqlNode {
    public string TableName { get; set; } = "";
    public List<Assign> Assigns { get; set; } = [];
    public Expression? WhereClause { get; set; } = null;

    public override void Accept(IVisitor visitor) => visitor.Visit(this);

    public override string ToString()
    {
        var assignsStr = string.Join(", ", Assigns.Select(a => $"{a.ColumnName} = {a.Expression}"));
        var whereStr = WhereClause != null ? $" WHERE {WhereClause}" : "";
        return $"UPDATE {TableName} SET {assignsStr}{whereStr}";
    }
}

public class Assign {
    public string ColumnName { get; }
    public Expression Expression { get; }

    public Assign(string name, Expression expression) {
        ColumnName = name;
        Expression = expression;
    }
}