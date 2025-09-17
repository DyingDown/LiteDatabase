namespace LiteDatabase.Sql.Ast;

public class ColumnDefinition {
    public string ColumnName { get; }
    public ColumnType ColumnType { get; }

    public List<ColumnConstraint>? ColumnConstraints { get; } = new();

    public int? Length { get; }

    public ColumnDefinition(string name, ColumnType type, int? length = null, List<ColumnConstraint>? constraints = null) {
        ColumnName = name;
        ColumnType = type;
        Length = length;
        ColumnConstraints = constraints;
    }

    public override string ToString()
    {
        var constraints = (ColumnConstraints ?? new List<ColumnConstraint>())
            .Select(c =>
            {
                var constraintStr = ColumnConstraintToString(c.Type);
                return c.Value != null ? $"{constraintStr}({c.Value})" : constraintStr;
            });
        return $"{ColumnName} {ColumnTypeToString(ColumnType)} {string.Join(" ", constraints)}";
    }

    public string ColumnTypeToString(ColumnType type) => type switch {
        ColumnType.Int => "INT",
        ColumnType.Float => "FLOAT",
        ColumnType.String => "STRING",
        ColumnType.Bool => "BOOL",
        _ => "INVALID",
    };

    public string ColumnConstraintToString(ColumnConstraintType type) => type switch {
        ColumnConstraintType.PrimaryKey => "Primary Key",
        ColumnConstraintType.Unique => "Unique",
        ColumnConstraintType.NotNull => "Not Null",
        ColumnConstraintType.Default => "Default",
        _ => "Invalid",
    };
}

public enum ColumnType {
    Int,
    Float,
    String,
    Bool,
}

public class ColumnConstraint {
    public ColumnConstraintType Type { get; set; }
    public object? Value { get; set; } // 用于保存 DEFAULT 的值

    public ColumnConstraint(ColumnConstraintType type, object? value = null) {
        Type = type;
        Value = value;
    }

    public ColumnConstraint() {

    }
}

public enum ColumnConstraintType {
    PrimaryKey,
    NotNull,
    Unique,
    Default,
}