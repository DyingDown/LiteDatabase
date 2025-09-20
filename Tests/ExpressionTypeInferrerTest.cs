using LiteDatabase.Catalog;
using LiteDatabase.Sql;
using LiteDatabase.Sql.Ast;
using LiteDatabase.Sql.Ast.Expressions;

namespace LiteDatabase.Tests;

/// <summary>
/// 测试表达式类型推断和缓存功能
/// </summary>
public class ExpressionTypeInferrerTest
{
    public static void TestTypeCaching()
    {
        Console.WriteLine("Testing expression type caching...");

        // 创建模拟的目录管理器
        var catalog = new MockCatalogManager();
        var tableSchemas = new Dictionary<string, Dictionary<string, ColumnDefinition>>
        {
            ["users"] = new Dictionary<string, ColumnDefinition>
            {
                ["id"] = new ColumnDefinition("id", ColumnType.Int, false, true),
                ["name"] = new ColumnDefinition("name", ColumnType.String, false, false),
                ["age"] = new ColumnDefinition("age", ColumnType.Int, true, false)
            }
        };

        var inferrer = new ExpressionTypeInferrer(catalog, tableSchemas);

        // 测试字面量表达式
        var intLiteral = new LiteralExpression(42);
        Console.WriteLine($"Before inference - InferredType: {intLiteral.InferredType}");
        
        var type1 = inferrer.InferType(intLiteral);
        Console.WriteLine($"First inference - Type: {type1}, Cached: {intLiteral.InferredType}");
        
        var type2 = inferrer.InferType(intLiteral);
        Console.WriteLine($"Second inference - Type: {type2}, Cached: {intLiteral.InferredType}");
        
        // 验证两次推断结果相同且使用了缓存
        if (type1 == type2 && intLiteral.InferredType == type1)
        {
            Console.WriteLine("✓ Type caching works correctly for literal expressions");
        }
        else
        {
            Console.WriteLine("✗ Type caching failed for literal expressions");
        }

        // 测试复杂表达式（二元表达式）
        var columnRef = new ColumnRefExpression("age");
        var binaryExpr = new BinaryExpression(columnRef, BinaryOperatorType.Add, intLiteral);
        
        Console.WriteLine($"\nBefore binary expression inference - InferredType: {binaryExpr.InferredType}");
        
        var binaryType1 = inferrer.InferType(binaryExpr);
        Console.WriteLine($"First binary inference - Type: {binaryType1}, Cached: {binaryExpr.InferredType}");
        Console.WriteLine($"Column ref cached type: {columnRef.InferredType}");
        Console.WriteLine($"Literal cached type: {intLiteral.InferredType}");
        
        var binaryType2 = inferrer.InferType(binaryExpr);
        Console.WriteLine($"Second binary inference - Type: {binaryType2}, Cached: {binaryExpr.InferredType}");
        
        if (binaryType1 == binaryType2 && binaryExpr.InferredType == binaryType1)
        {
            Console.WriteLine("✓ Type caching works correctly for binary expressions");
        }
        else
        {
            Console.WriteLine("✗ Type caching failed for binary expressions");
        }

        // 测试缓存清除
        Console.WriteLine("\nTesting cache clearing...");
        inferrer.ClearTypeCache(binaryExpr);
        Console.WriteLine($"After clearing - Binary: {binaryExpr.InferredType}, Column: {columnRef.InferredType}, Literal: {intLiteral.InferredType}");
        
        if (binaryExpr.InferredType == null && columnRef.InferredType == null)
        {
            Console.WriteLine("✓ Cache clearing works correctly");
        }
        else
        {
            Console.WriteLine("✗ Cache clearing failed");
        }

        Console.WriteLine("\nType caching test completed!");
    }
}

/// <summary>
/// 模拟的目录管理器用于测试
/// </summary>
public class MockCatalogManager : ICatalogManager
{
    public bool TableExists(string tableName) => tableName == "users";
    public void CreateTable(string tableName, List<ColumnDefinition> columns) { }
    public void DropTable(string tableName) { }
    public void InsertData(string tableName, Dictionary<string, object> data) { }
    public List<Dictionary<string, object>> SelectData(string tableName, List<string> columns, Func<Dictionary<string, object>, bool> whereClause = null) => new();
    public void UpdateData(string tableName, Dictionary<string, object> updates, Func<Dictionary<string, object>, bool> whereClause) { }
    public void DeleteData(string tableName, Func<Dictionary<string, object>, bool> whereClause) { }
    public List<ColumnDefinition> GetTableSchema(string tableName) => 
        tableName == "users" ? new List<ColumnDefinition>
        {
            new("id", ColumnType.Int, false, true),
            new("name", ColumnType.String, false, false),
            new("age", ColumnType.Int, true, false)
        } : new();
}