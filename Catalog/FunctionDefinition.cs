using LiteDatabase.Sql.Ast;

namespace LiteDatabase.Catalog;

/// <summary>
/// 函数参数定义
/// </summary>
public class FunctionParameter {
    public string Name { get; set; }
    public List<ColumnType> AcceptedTypes { get; set; }
    public bool IsOptional { get; set; }
    
    // 构造函数：单一类型（最常用）
    public FunctionParameter(string name, ColumnType type, bool isOptional = false) {
        Name = name;
        AcceptedTypes = new List<ColumnType> { type };
        IsOptional = isOptional;
    }
    
    // 构造函数：多种类型
    public FunctionParameter(string name, List<ColumnType> acceptedTypes, bool isOptional = false) {
        Name = name;
        AcceptedTypes = acceptedTypes;
        IsOptional = isOptional;
    }
    
    // 静态方法：接受数值类型的参数（用于 SUM、AVG）
    public static FunctionParameter NumericType(string name, bool isOptional = false) {
        return new FunctionParameter(name, new List<ColumnType> { ColumnType.Int, ColumnType.Float }, isOptional);
    }
    
    // 静态方法：接受可比较类型的参数（用于 MIN、MAX）
    public static FunctionParameter ComparableType(string name, bool isOptional = false) {
        return new FunctionParameter(name, new List<ColumnType> { ColumnType.Int, ColumnType.Float, ColumnType.String }, isOptional);
    }
    
    // 静态方法：接受任意类型的参数（用于 COUNT 等特殊函数）
    public static FunctionParameter AnyType(string name, bool isOptional = false) {
        // 简单直接：把所有类型都加到列表里！
        return new FunctionParameter(name, new List<ColumnType> { 
            ColumnType.Int, 
            ColumnType.Float, 
            ColumnType.String, 
            ColumnType.Bool 
        }, isOptional);
    }
    
    // 检查类型是否被接受
    public bool AcceptsType(ColumnType type) {
        return AcceptedTypes.Contains(type);
    }
}

/// <summary>
/// 函数定义
/// </summary>
public class FunctionDefinition {
    public string Name { get; }
    public IReadOnlyList<FunctionParameter> Parameters { get; }
    public ColumnType ReturnType { get; }
    public bool IsAggregate { get; }
    public bool AcceptsStarArgument { get; } // 是否支持 * 参数（如 COUNT(*)）
    
    // 构造函数：完整定义函数签名（推荐）
    public FunctionDefinition(string name, List<FunctionParameter> parameters, ColumnType returnType, bool isAggregate = false, bool acceptsStarArgument = false) {
        Name = name;
        Parameters = parameters.AsReadOnly();
        ReturnType = returnType;
        IsAggregate = isAggregate;
        AcceptsStarArgument = acceptsStarArgument;
    }
    
    // 构造函数：无参数函数
    public FunctionDefinition(string name, ColumnType returnType, bool isAggregate = false, bool acceptsStarArgument = false) 
        : this(name, new List<FunctionParameter>(), returnType, isAggregate, acceptsStarArgument) {
    }
    
    // 构造函数：单参数函数（常用简化版）
    public FunctionDefinition(string name, FunctionParameter parameter, ColumnType returnType, bool isAggregate = false, bool acceptsStarArgument = false) 
        : this(name, new List<FunctionParameter> { parameter }, returnType, isAggregate, acceptsStarArgument) {
    }
    
    // COUNT 函数的特殊工厂方法
    public static FunctionDefinition CreateCountFunction() {
        return new FunctionDefinition(
            "COUNT", 
            new List<FunctionParameter> {
                FunctionParameter.AnyType("expression", isOptional: true) // COUNT(*) 时参数可选，COUNT(expr) 时参数必需
            },
            ColumnType.Int, 
            isAggregate: true, 
            acceptsStarArgument: true
        );
    }
}