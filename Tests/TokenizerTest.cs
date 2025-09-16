using LiteDatabase.Sql.Token;
using Microsoft.Extensions.Logging;

namespace LiteDatabase.Tests;

/// <summary>
/// Tokenizer 单元测试类
/// </summary>
public static class TokenizerTest {
    
    /// <summary>
    /// 测试基本的SQL语句解析
    /// </summary>
    public static void TestBasicSQL() {
        Console.WriteLine("=== 测试基本SQL语句 ===");
        
        try {
            var tokenizer = new Tokenizer("SELECT * FROM users");
            
            Console.WriteLine("测试SQL: SELECT * FROM users");
            
            Token? token;
            while ((token = tokenizer.GetNextToken()) != null) {
                Console.WriteLine($"Token: {token.Type} - '{token.Lexeme}'");
                if (token.Type == TokenType.END) break;
            }
            
            Console.WriteLine("✅ 基本SQL测试通过");
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ 基本SQL测试失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 测试包含字符串的SQL
    /// </summary>
    public static void TestStringSQL() {
        Console.WriteLine("\n=== 测试字符串SQL ===");
        
        try {
            var tokenizer = new Tokenizer("SELECT name FROM users WHERE name = 'John'");
            
            Console.WriteLine("测试SQL: SELECT name FROM users WHERE name = 'John'");
            
            Token? token;
            while ((token = tokenizer.GetNextToken()) != null) {
                Console.WriteLine($"Token: {token.Type} - '{token.Lexeme}'");
                if (token.Type == TokenType.END) break;
            }
            
            Console.WriteLine("✅ 字符串SQL测试通过");
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ 字符串SQL测试失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 测试包含数字的SQL
    /// </summary>
    public static void TestNumberSQL() {
        Console.WriteLine("\n=== 测试数字SQL ===");
        
        try {
            var tokenizer = new Tokenizer("SELECT * FROM users WHERE age > 18 AND score = -1.22323.879");
            
            Console.WriteLine("测试SQL: SELECT * FROM users WHERE age > 18 AND score = 95.5");
            
            Token? token;
            while ((token = tokenizer.GetNextToken()) != null) {
                Console.WriteLine($"Token: {token.Type} - '{token.Lexeme}'");
                if (token.Type == TokenType.END) break;
            }
            
            Console.WriteLine("✅ 数字SQL测试通过");
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ 数字SQL测试失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 测试空SQL
    /// </summary>
    public static void TestEmptySQL() {
        Console.WriteLine("\n=== 测试空SQL ===");
        
        try {
            var tokenizer = new Tokenizer("");
            
            Console.WriteLine("测试SQL: (空字符串)");
            
            Token? token = tokenizer.GetNextToken();
            Console.WriteLine($"Token: {token?.Type} - '{token?.Lexeme}'");
            
            Console.WriteLine("✅ 空SQL测试通过");
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ 空SQL测试失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 运行所有测试
    /// </summary>
    public static void RunAllTests() {
        Console.WriteLine("开始运行 Tokenizer 单元测试...\n");
        
        TestBasicSQL();
        TestStringSQL();
        TestNumberSQL();
        TestEmptySQL();
        
        Console.WriteLine("\n=== 所有测试完成 ===");
    }
}