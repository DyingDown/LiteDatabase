namespace LiteDatabase.Sql.Ast.Expressions;

public class FunctionCallExpression : Expression {
    public FunctionName FunctionName { get; }

    public List<Expression> Arguments { get; }

    public FunctionCallExpression(FunctionName functionName, List<Expression> arguments) {
        FunctionName = functionName;
        Arguments = arguments;
    }

    public override void Accept(IVisitor visitor) => visitor.Visit(this);
    
    public override string ToString() => $"{FunctionNameToString(FunctionName)}({string.Join(", ", Arguments)})";

    private string FunctionNameToString(FunctionName func) => func switch {
        FunctionName.Sum => "SUM",
        FunctionName.Count => "COUNT",
        FunctionName.Avg => "AVG",
        FunctionName.Min => "MIN",
        FunctionName.Max => "MAX",
        _ => func.ToString().ToUpper(),
    };
}

public enum FunctionName {
    Sum,
    Count,
    Avg,
    Min,
    Max
}