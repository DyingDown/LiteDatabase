namespace LiteDatabase.Sql.Ast.Expressions;

public class ColumnRefExpression : Expression {
    public string? TableName { get; }
    public string ColumnName { get; }
    public ColumnRefExpression(string columnName, string? tableName = null) {
        ColumnName = columnName;
        TableName = tableName;
    }

    public override string ToString() => TableName == null ? ColumnName : $"{TableName}.{ColumnName}";
}