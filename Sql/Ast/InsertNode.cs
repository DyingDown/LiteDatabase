namespace LiteDatabase.Sql.Ast;

public class InsertNode : SqlNode {
    public string TableName { get; set; } = "";
    public List<string>? ColumnNames { get; set; } = [];
    public List<List<SqlValue>> Values { get; set; } = [];

    public override void Accept(IVisitor visitor) => visitor.Visit(this);

    public override string ToString() {
        var columnsStr = ColumnNames?.Count > 0
            ? $"({string.Join(", ", ColumnNames)}) "
            : "";
        var valuesStr = string.Join(", ", Values.Select(row =>
            $"({string.Join(", ", row.Select(val => val.ToString()))})"
        ));
        return $"INSERT INTO {TableName} {columnsStr}VALUES {valuesStr}";
    }
}

public enum ValueType {
    String,
    Null,
    True,
    False,
    Int,
    Float,
}

public class SqlValue {
    public ValueType Type { get; }
    public object? Value { get; }
    
    public SqlValue(ValueType type, object? value) {
        Type = type;
        Value = value;
    }
    
    public override string ToString() => Type switch {
        ValueType.String => $"'{Value}'",
        ValueType.Null => "NULL",
        ValueType.True => "TRUE",
        ValueType.False => "FALSE",
        ValueType.Int => Value?.ToString() ?? "0",
        ValueType.Float => Value?.ToString() ?? "0.0",
        _ => Value?.ToString() ?? ""
    };
}