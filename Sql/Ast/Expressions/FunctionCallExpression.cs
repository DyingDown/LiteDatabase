namespace LiteDatabase.Sql.Ast.Expressions;

public class FunctionCallExpression : Expression {
    public string FunctionName { get; }

    public List<Expression> Arguments { get; }

    public FunctionCallExpression(string functionName, List<Expression> arguments) {
        FunctionName = functionName ?? throw new ArgumentNullException(nameof(functionName));
        Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
    }

    public override void Accept(IVisitor visitor) => visitor.Visit(this);
    
    public override string ToString() => $"{FunctionName.ToUpper()}({string.Join(", ", Arguments)})";
}