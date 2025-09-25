// using LiteDatabase.Sql.Ast.Expressions;

// namespace LiteDatabase.Sql;

// /// <summary>
// /// 方案2：函数注册表 + 委托
// /// 更灵活，支持动态注册用户自定义函数
// /// </summary>
// public class FunctionExecutor {
//     // 函数执行委托
//     public delegate object FunctionDelegate(List<Expression> arguments, ExecutionContext context);
    
//     // 函数执行注册表
//     private readonly Dictionary<FunctionName, FunctionDelegate> _functionExecutors;
    
//     public FunctionExecutor() {
//         _functionExecutors = new Dictionary<FunctionName, FunctionDelegate>();
//         RegisterBuiltinFunctions();
//     }
    
//     /// <summary>
//     /// 注册函数执行逻辑
//     /// </summary>
//     public void RegisterFunction(FunctionName name, FunctionDelegate executor) {
//         _functionExecutors[name] = executor;
//     }
    
//     /// <summary>
//     /// 执行函数
//     /// </summary>
//     public object ExecuteFunction(FunctionName name, List<Expression> arguments, ExecutionContext context) {
//         if (!_functionExecutors.ContainsKey(name)) {
//             throw new Exception($"Function executor not found: {name}");
//         }
        
//         return _functionExecutors[name](arguments, context);
//     }
    
//     /// <summary>
//     /// 注册内置函数的执行逻辑
//     /// </summary>
//     private void RegisterBuiltinFunctions() {
//         // COUNT 函数
//         RegisterFunction(FunctionName.Count, (args, ctx) => {
//             // COUNT(*) 或 COUNT(expression) 的实际逻辑
//             if (args.Count == 0) {
//                 // COUNT(*) - 统计所有行
//                 return ctx.GetRowCount();
//             } else {
//                 // COUNT(expression) - 统计非 NULL 值
//                 return ctx.CountNonNullValues(args[0]);
//             }
//         });
        
//         // SUM 函数
//         RegisterFunction(FunctionName.Sum, (args, ctx) => {
//             if (args.Count != 1) {
//                 throw new Exception("SUM requires exactly one argument");
//             }
//             return ctx.SumValues(args[0]);
//         });
        
//         // AVG 函数
//         RegisterFunction(FunctionName.Avg, (args, ctx) => {
//             if (args.Count != 1) {
//                 throw new Exception("AVG requires exactly one argument");
//             }
//             return ctx.AverageValues(args[0]);
//         });
        
//         // MIN 函数
//         RegisterFunction(FunctionName.Min, (args, ctx) => {
//             if (args.Count != 1) {
//                 throw new Exception("MIN requires exactly one argument");
//             }
//             return ctx.MinValue(args[0]);
//         });
        
//         // MAX 函数
//         RegisterFunction(FunctionName.Max, (args, ctx) => {
//             if (args.Count != 1) {
//                 throw new Exception("MAX requires exactly one argument");
//             }
//             return ctx.MaxValue(args[0]);
//         });
//     }
// }

// /// <summary>
// /// 执行上下文，提供数据访问接口
// /// </summary>
// public class ExecutionContext {
//     // TODO: 实现数据访问方法
//     public int GetRowCount() => throw new NotImplementedException();
//     public int CountNonNullValues(Expression expr) => throw new NotImplementedException();
//     public object SumValues(Expression expr) => throw new NotImplementedException();
//     public object AverageValues(Expression expr) => throw new NotImplementedException();
//     public object MinValue(Expression expr) => throw new NotImplementedException();
//     public object MaxValue(Expression expr) => throw new NotImplementedException();
// }