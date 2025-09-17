namespace LiteDatabase.Sql.Ast.Expressions;

public class StarExpression : Expression {

    public string? TableName { get; }

    public StarExpression(string? tableName) {
        TableName = tableName;
    }
    public override string ToString() => TableName == null ? "*": $"{TableName}.*";
}