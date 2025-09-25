using LiteDatabase.Sql.Ast;

namespace LiteDatabase.Sql.Semantic;

public enum TypeKind {Scalar, RowSet, Unknown};

public class ExpressionType {
    public TypeKind Kind { get; set; }
    public ColumnType? BaseType { get; set; }

    public bool IsNullable { get; set; }

    public IReadOnlyList<ExpressionType>? RowSchema { get; set; }

    public static ExpressionType Scalar(ColumnType baseType, bool isNullable) {
        return new ExpressionType { Kind = TypeKind.Scalar, BaseType = baseType, IsNullable = isNullable };
    }

    public static ExpressionType RowSet(IReadOnlyList<ExpressionType> schema) {
        return new ExpressionType { Kind = TypeKind.RowSet, RowSchema = schema };
    }

    public static ExpressionType Unkown() {
        return new ExpressionType { Kind = TypeKind.Unknown, BaseType = null, IsNullable = true };
    }
}