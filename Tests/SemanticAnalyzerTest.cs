using LiteDatabase.Catalog;
using LiteDatabase.Sql;
using LiteDatabase.Sql.Ast;
using LiteDatabase.Sql.Semantic;

namespace LiteDatabase.Tests;

public class SemanticAnalyzerTest {
    private static ICatalogManager CreateTestCatalog() {
        var catalog = new CatalogManager();
        
        // 创建测试表 Users
        var userColumns = new List<ColumnDefinition> {
            new ColumnDefinition("id", ColumnType.Int),
            new ColumnDefinition("name", ColumnType.String),
            new ColumnDefinition("age", ColumnType.Int),
            new ColumnDefinition("email", ColumnType.String)
        };
        catalog.CreateTable("users", userColumns);
        
        // 创建测试表 Orders
        var orderColumns = new List<ColumnDefinition> {
            new ColumnDefinition("id", ColumnType.Int),  // 添加id列使其与users.id产生歧义
            new ColumnDefinition("user_id", ColumnType.Int),
            new ColumnDefinition("amount", ColumnType.Float),
            new ColumnDefinition("status", ColumnType.String)
        };
        catalog.CreateTable("orders", orderColumns);
        
        return catalog;
    }
    
    public static void RunAllTests() {
        Console.WriteLine("=== 开始语义分析器测试 ===");
        
        TestValidQueries();
        TestInvalidQueries();
        
        Console.WriteLine("=== 语义分析器测试完成 ===\n");
    }
    
    private static void TestValidQueries() {
        Console.WriteLine("--- 测试合法查询 ---");
        var catalog = CreateTestCatalog();
        var analyzer = new SemanticAnalyzer(catalog);
        
        var validQueries = new[] {
            "SELECT * FROM users;",
            "SELECT users.name, users.age FROM users;",
            "SELECT u.name, u.age FROM users u;",
            "SELECT name FROM users WHERE age > 18;",
            "SELECT COUNT(*) FROM users;",
            "SELECT u.name, o.amount FROM users u, orders o WHERE u.id = o.user_id;",
            "SELECT name FROM users WHERE id IN (SELECT user_id FROM orders WHERE amount > 100);"
        };
        
        foreach (var sql in validQueries) {
            try {
                var parser = new Parser(sql);
                var ast = parser.ParseStatement() as SelectNode;
                
                if (ast != null) {
                    analyzer.Visit(ast);
                    Console.WriteLine($"✓ 合法: {sql}");
                } else {
                    Console.WriteLine($"✗ 解析失败: {sql}");
                }
            } catch (Exception ex) {
                Console.WriteLine($"✗ 意外失败: {sql} - {ex.Message}");
            }
        }
    }
    
    private static void TestInvalidQueries() {
        Console.WriteLine("\n--- 测试看似合法但实际不合法的查询 ---");
        var catalog = CreateTestCatalog();
        var analyzer = new SemanticAnalyzer(catalog);
        
        var invalidQueries = new[] {
            // 1. 表不存在
            ("SELECT * FROM nonexistent_table;", "表不存在"),
            
            // 2. 列不存在
            ("SELECT invalid_column FROM users;", "列不存在"),
            
            // 3. 表别名重复
            ("SELECT * FROM users u, orders u;", "表别名重复"),
            
            // 4. 列引用模糊（多表中都有同名列但没有指定表名）
            ("SELECT id FROM users, orders;", "列引用模糊 - users和orders都有id列"),
            
            // 5. 表限定的列不存在
            ("SELECT users.invalid_column FROM users;", "指定表中的列不存在"),
            
            // 6. 错误的表别名
            ("SELECT wrong_alias.name FROM users u;", "错误的表别名"),
            
            // 7. 子查询中的表不存在
            ("SELECT name FROM users WHERE id IN (SELECT user_id FROM invalid_table);", "子查询中表不存在"),
            
            // 8. 子查询中的列不存在
            ("SELECT name FROM users WHERE id IN (SELECT invalid_col FROM orders);", "子查询中列不存在"),
            
            // 9. WHERE子句类型错误（非布尔表达式）
            ("SELECT name FROM users WHERE name;", "WHERE子句必须是布尔表达式"),
            
            // 10. table.* 中表名不存在（这个会在Parser层被拒绝）
            ("SELECT invalid_table.* FROM users;", "table.*中表名不存在于FROM子句"),
            
            // 11. 函数参数错误
            ("SELECT MAX(*) FROM users;", "只有COUNT(*)是合法的，MAX(*)不合法"),
            
            // 12. GROUP BY中的列不存在
            ("SELECT name FROM users GROUP BY invalid_column;", "GROUP BY中的列不存在"),
            
            // 13. ORDER BY中的列不存在
            ("SELECT name FROM users ORDER BY invalid_column;", "ORDER BY中的列不存在")
        };
        
        foreach (var (sql, description) in invalidQueries) {
            try {
                var parser = new Parser(sql);
                var ast = parser.ParseStatement() as SelectNode;
                
                if (ast != null) {
                    analyzer.Visit(ast);
                    Console.WriteLine($"✗ 应该失败但成功了: {sql} ({description})");
                } else {
                    Console.WriteLine($"✗ 解析失败: {sql}");
                }
            } catch (Exception ex) {
                Console.WriteLine($"✓ 正确拒绝: {description} - {ex.Message}");
            }
        }
    }
}