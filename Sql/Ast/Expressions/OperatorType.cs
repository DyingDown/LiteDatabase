namespace LiteDatabase.Sql.Ast.Expressions;


public enum BinaryOperatorType {
    Equal,
    And,
    Or,
    LessThan,
    LessOrEqual,
    Between,
    GreaterOrEqual,
    GreaterThan,
    NotEqual,
    Like,
    In,
    Is,
    IsNot

}

public enum UnaryOperatorType {
    Not,
    Plus,
    Minus
}

public static class OperatorTypeExtensions {
    public static string ToSqlString(this BinaryOperatorType op) => op switch {
        BinaryOperatorType.Equal => "=",
        BinaryOperatorType.NotEqual => "!=",
        BinaryOperatorType.And => "AND",
        BinaryOperatorType.Or => "OR",
        BinaryOperatorType.LessThan => "<",
        BinaryOperatorType.LessOrEqual => "<=",
        BinaryOperatorType.GreaterThan => ">",
        BinaryOperatorType.GreaterOrEqual => ">=",
        BinaryOperatorType.Between => "BETWEEN",
        BinaryOperatorType.Like => "LIKE",
        BinaryOperatorType.In => "IN",
        BinaryOperatorType.Is => "IS",
        BinaryOperatorType.IsNot => "IS NOT",
        _ => op.ToString()
    };


}