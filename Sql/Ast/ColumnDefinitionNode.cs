namespace LiteDatabase.Sql.Ast;

public class ColumnDefinitionNode {
    public string ColumnName { get; }
    public ColumnType ColumnType { get; }

    public List<ColumnConstraintType>? ColumnConstraints { get; } = new();

    public int? Length { get; }

    public ColumnDefinitionNode(string name, ColumnType type, int? length = null, List<ColumnConstraintType>? constraints = null) {
        ColumnName = name;
        ColumnType = type;
        Length = length;
        ColumnConstraints = constraints;
    }

    public override string ToString() => $"{ColumnName} {ColumnTypeToString(ColumnType)}";

    public string ColumnTypeToString(ColumnType type) => type switch {
        ColumnType.Int => "INT",
        ColumnType.Float => "FLOAT",
        ColumnType.String => "STRING",
        ColumnType.Bool => "BOOL",
        _ => "INVALID",
    };
}

public enum ColumnType {
    Int,
    Float,
    String,
    Bool,
}

public enum ColumnConstraintType {
    PrimaryKey,
    NotNull,
    Unique,
    Default,
}