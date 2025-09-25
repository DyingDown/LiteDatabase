// using LiteDatabase.Sql.Ast.Expressions;

// namespace LiteDatabase.Sql;

// /// <summary>
// /// 方案3：接口 + 工厂模式
// /// 每个函数都是独立的类，支持复杂的函数逻辑
// /// </summary>

// // 函数执行接口
// public interface IFunctionHandler {
//     FunctionName FunctionName { get; }
//     object Execute(List<Expression> arguments, ExecutionContext context);
// }

// // COUNT 函数处理器
// public class CountFunctionHandler : IFunctionHandler {
//     public FunctionName FunctionName => FunctionName.Count;
    
//     public object Execute(List<Expression> arguments, ExecutionContext context) {
//         if (arguments.Count == 0) {
//             // COUNT(*) - 统计所有行
//             return context.GetRowCount();
//         } else if (arguments.Count == 1) {
//             // COUNT(expression) - 统计非 NULL 值
//             return context.CountNonNullValues(arguments[0]);
//         } else {
//             throw new Exception("COUNT accepts 0 or 1 argument");
//         }
//     }
// }

// // SUM 函数处理器
// public class SumFunctionHandler : IFunctionHandler {
//     public FunctionName FunctionName => FunctionName.Sum;
    
//     public object Execute(List<Expression> arguments, ExecutionContext context) {
//         if (arguments.Count != 1) {
//             throw new Exception("SUM requires exactly one argument");
//         }
        
//         // 可以在这里实现复杂的 SUM 逻辑
//         // 比如类型检查、NULL 处理、溢出检查等
//         return context.SumValues(arguments[0]);
//     }
// }

// // 函数工厂
// public class FunctionHandlerFactory {
//     private readonly Dictionary<FunctionName, IFunctionHandler> _handlers;
    
//     public FunctionHandlerFactory() {
//         _handlers = new Dictionary<FunctionName, IFunctionHandler>();
//         RegisterBuiltinHandlers();
//     }
    
//     public void RegisterHandler(IFunctionHandler handler) {
//         _handlers[handler.FunctionName] = handler;
//     }
    
//     public IFunctionHandler GetHandler(FunctionName functionName) {
//         if (!_handlers.ContainsKey(functionName)) {
//             throw new Exception($"No handler registered for function: {functionName}");
//         }
//         return _handlers[functionName];
//     }
    
//     private void RegisterBuiltinHandlers() {
//         RegisterHandler(new CountFunctionHandler());
//         RegisterHandler(new SumFunctionHandler());
//         // 可以继续添加其他函数处理器
//     }
// }