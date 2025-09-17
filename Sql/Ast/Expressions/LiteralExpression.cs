namespace LiteDatabase.Sql.Ast.Expressions;

public class LiteralExpression : Expression {
    public object Value { get; }
    public LiteralExpression(object value) {
        Value = value;
    }
    public override string ToString() {
        if (Value == null) return "NULL";
        if (Value is string) return $"'{Value}'";
        if (Value is bool b) return b ? "TRUE" : "FALSE";
        return Value.ToString() ?? "NULL";
    }
}