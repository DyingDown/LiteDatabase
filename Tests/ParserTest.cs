using LiteDatabase.Sql;
using LiteDatabase.Sql.Ast;
using LiteDatabase.Sql.Ast.Expressions;
using Microsoft.Extensions.Logging;

namespace LiteDatabase.Tests;

/// <summary>
/// Parser 单元测试类
/// </summary>
public static class ParserTest {
    
    /// <summary>
    /// 测试CREATE TABLE语句解析
    /// </summary>
    public static void TestCreateTableStatement() {
        Console.WriteLine("=== 测试CREATE TABLE语句 ===");
        
        // 测试基本CREATE TABLE
        try {
            string sql = "CREATE TABLE users (id INT PRIMARY KEY, name VARCHAR(50) NOT NULL, age INT);";
            Console.WriteLine($"测试SQL 1: {sql}");
            var parser = new Parser(sql);
            var result = parser.ParseStatement();
            Console.WriteLine($"解析结果:\n{result}");
            Console.WriteLine("✅ 基本CREATE TABLE测试通过");
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ 基本CREATE TABLE测试失败: {ex.Message}");
        }
        
        // 测试多种数据类型的CREATE TABLE
        try {
            string sql = "CREATE TABLE products (id INT PRIMARY KEY, name VARCHAR(100) NOT NULL, price FLOAT, is_active BOOL);";
            Console.WriteLine($"\n测试SQL 2: {sql}");
            var parser = new Parser(sql);
            var result = parser.ParseStatement();
            Console.WriteLine($"解析结果:\n{result}");
            Console.WriteLine("✅ 多数据类型CREATE TABLE测试通过");
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ 多数据类型CREATE TABLE测试失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 测试SELECT语句解析
    /// </summary>
    public static void TestSelectStatement() {
        Console.WriteLine("\n=== 测试SELECT语句 ===");
        
        // 测试基本SELECT
        try {
            string sql = "SELECT id, name FROM users WHERE age > 18;";
            Console.WriteLine($"测试SQL 1: {sql}");
            var parser = new Parser(sql);
            var result = parser.ParseStatement();
            Console.WriteLine($"解析结果:\n{result}");
            Console.WriteLine("✅ 基本SELECT测试通过");
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ 基本SELECT测试失败: {ex.Message}");
        }
        
        // 测试带函数的SELECT
        try {
            string sql = "SELECT name, COUNT(*), AVG(age) FROM users WHERE age > 21 GROUP BY name;";
            Console.WriteLine($"\n测试SQL 2: {sql}");
            var parser = new Parser(sql);
            var result = parser.ParseStatement();
            Console.WriteLine($"解析结果:\n{result}");
            Console.WriteLine("✅ 聚合函数SELECT测试通过");
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ 聚合函数SELECT测试失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 测试复杂的SELECT语句（包含GROUP BY和ORDER BY）
    /// </summary>
    public static void TestComplexSelectStatement() {
        Console.WriteLine("\n=== 测试复杂SELECT语句 ===");
        
        // 测试GROUP BY和ORDER BY
        try {
            string sql = "SELECT name, COUNT(*) FROM users WHERE age > 18 GROUP BY name ORDER BY name ASC;";
            Console.WriteLine($"测试SQL 1: {sql}");
            var parser = new Parser(sql);
            var result = parser.ParseStatement();
            Console.WriteLine($"解析结果:\n{result}");
            Console.WriteLine("✅ GROUP BY + ORDER BY测试通过");
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ GROUP BY + ORDER BY测试失败: {ex.Message}");
        }
        
        // 测试多表查询
        try {
            string sql = "SELECT u.name, p.title FROM users u, posts p WHERE u.id = p.user_id AND u.age > 21;";
            Console.WriteLine($"\n测试SQL 2: {sql}");
            var parser = new Parser(sql);
            var result = parser.ParseStatement();
            Console.WriteLine($"解析结果:\n{result}");
            Console.WriteLine("✅ 多表查询测试通过");
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ 多表查询测试失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 测试INSERT语句解析
    /// </summary>
    public static void TestInsertStatement() {
        Console.WriteLine("\n=== 测试INSERT语句 ===");
        
        // 测试基本INSERT
        try {
            string sql = "INSERT INTO users (name, age) VALUES ('John', 25);";
            Console.WriteLine($"测试SQL 1: {sql}");
            var parser = new Parser(sql);
            var result = parser.ParseStatement();
            Console.WriteLine($"解析结果:\n{result}");
            Console.WriteLine("✅ 基本INSERT测试通过");
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ 基本INSERT测试失败: {ex.Message}");
        }
        
        // 测试多值INSERT
        try {
            string sql = "INSERT INTO products (name, price, is_active) VALUES ('Product 1', 29.99, true), ('Product 2', 49.99, false);";
            Console.WriteLine($"\n测试SQL 2: {sql}");
            var parser = new Parser(sql);
            var result = parser.ParseStatement();
            Console.WriteLine($"解析结果:\n{result}");
            Console.WriteLine("✅ 多值INSERT测试通过");
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ 多值INSERT测试失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 测试UPDATE语句解析
    /// </summary>
    public static void TestUpdateStatement() {
        Console.WriteLine("\n=== 测试UPDATE语句 ===");
        
        // 测试基本UPDATE
        try {
            string sql = "UPDATE users SET age = 26, name = 'Jane' WHERE id = 1;";
            Console.WriteLine($"测试SQL 1: {sql}");
            var parser = new Parser(sql);
            var result = parser.ParseStatement();
            Console.WriteLine($"解析结果:\n{result}");
            Console.WriteLine("✅ 基本UPDATE测试通过");
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ 基本UPDATE测试失败: {ex.Message}");
        }
        
        // 测试带数学表达式的UPDATE
        try {
            string sql = "UPDATE products SET price = price * 1.1, stock_count = stock_count - 1 WHERE category_id = 1;";
            Console.WriteLine($"\n测试SQL 2: {sql}");
            var parser = new Parser(sql);
            var result = parser.ParseStatement();
            Console.WriteLine($"解析结果:\n{result}");
            Console.WriteLine("✅ 数学表达式UPDATE测试通过");
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ 数学表达式UPDATE测试失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 测试DELETE语句解析
    /// </summary>
    public static void TestDeleteStatement() {
        Console.WriteLine("\n=== 测试DELETE语句 ===");
        
        // 测试基本DELETE
        try {
            string sql = "DELETE FROM users WHERE age < 18;";
            Console.WriteLine($"测试SQL 1: {sql}");
            var parser = new Parser(sql);
            var result = parser.ParseStatement();
            Console.WriteLine($"解析结果:\n{result}");
            Console.WriteLine("✅ 基本DELETE测试通过");
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ 基本DELETE测试失败: {ex.Message}");
        }
        
        // 测试复杂条件DELETE
        try {
            string sql = "DELETE FROM products WHERE price < 10.0 AND stock_count = 0;";
            Console.WriteLine($"\n测试SQL 2: {sql}");
            var parser = new Parser(sql);
            var result = parser.ParseStatement();
            Console.WriteLine($"解析结果:\n{result}");
            Console.WriteLine("✅ 复杂条件DELETE测试通过");
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ 复杂条件DELETE测试失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 测试子查询(Subquery)和特殊操作符解析
    /// </summary>
    public static void TestSubqueryStatement() {
        Console.WriteLine("\n=== 测试子查询和特殊操作符 ===");
        
        // 测试IN操作符 - 值列表
        try {
            string sql = "SELECT name, age FROM users WHERE id IN (1, 2, 3, 4, 5);";
            Console.WriteLine($"测试SQL 1: {sql}");
            var parser = new Parser(sql);
            var result = parser.ParseStatement();
            Console.WriteLine($"解析结果:\n{result}");
            Console.WriteLine("✅ IN值列表测试通过");
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ IN值列表测试失败: {ex.Message}");
        }
        
        // 测试BETWEEN操作符
        try {
            string sql = "SELECT name, age FROM users WHERE age BETWEEN 18 AND 65;";
            Console.WriteLine($"\n测试SQL 2: {sql}");
            var parser = new Parser(sql);
            var result = parser.ParseStatement();
            Console.WriteLine($"解析结果:\n{result}");
            Console.WriteLine("✅ BETWEEN测试通过");
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ BETWEEN测试失败: {ex.Message}");
        }
        
        // 测试IN操作符 - 子查询
        try {
            string sql = "SELECT name, age FROM users WHERE id IN (SELECT user_id FROM orders WHERE total > 100);";
            Console.WriteLine($"\n测试SQL 3: {sql}");
            var parser = new Parser(sql);
            var result = parser.ParseStatement();
            Console.WriteLine($"解析结果:\n{result}");
            Console.WriteLine("✅ IN子查询测试通过");
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ IN子查询测试失败: {ex.Message}");
        }
        
        // 测试SELECT列表中的子查询
        try {
            string sql = "SELECT name, (SELECT COUNT(*) FROM orders WHERE orders.user_id = users.id) AS order_count FROM users;";
            Console.WriteLine($"\n测试SQL 4: {sql}");
            var parser = new Parser(sql);
            var result = parser.ParseStatement();
            Console.WriteLine($"解析结果:\n{result}");
            Console.WriteLine("✅ SELECT子查询测试通过");
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ SELECT子查询测试失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 测试DROP TABLE语句解析
    /// </summary>
    public static void TestDropTableStatement() {
        Console.WriteLine("\n=== 测试DROP TABLE语句 ===");
        
        try {
            var parser = new Parser("DROP TABLE users, orders;");
            var result = parser.ParseStatement();
            
            Console.WriteLine("测试SQL: DROP TABLE users, orders;");
            Console.WriteLine($"解析结果:\n{result}");
            Console.WriteLine("✅ DROP TABLE测试通过");
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ DROP TABLE测试失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 测试表达式解析
    /// </summary>
    public static void TestExpressionParsing() {
        Console.WriteLine("\n=== 测试表达式解析 ===");
        
        // 测试基本表达式
        try {
            string sql = "SELECT * FROM users WHERE (age > 18 AND age < 65) OR name = 'Admin';";
            Console.WriteLine($"测试SQL 1: {sql}");
            var parser = new Parser(sql);
            var result = parser.ParseStatement();
            Console.WriteLine($"解析结果:\n{result}");
            Console.WriteLine("✅ 基本表达式解析测试通过");
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ 基本表达式解析测试失败: {ex.Message}");
        }
        
        // 测试数学表达式
        try {
            string sql = "SELECT name, salary * 12 AS annual_income FROM employees WHERE salary > 40000;";
            Console.WriteLine($"\n测试SQL 2: {sql}");
            var parser = new Parser(sql);
            var result = parser.ParseStatement();
            Console.WriteLine($"解析结果:\n{result}");
            Console.WriteLine("✅ 数学表达式测试通过");
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ 数学表达式测试失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 测试更复杂的错误SQL语句处理
    /// </summary>
    public static void TestErrorHandling() {
        Console.WriteLine("\n=== 测试错误处理 ===");
        
        string[] errorSQLs = {
            // 语法错误
            "SELECT FROM users;", // 缺少选择列
            "CREATE TABLE;", // 缺少表名
            "INSERT INTO;", // 不完整的INSERT
            "UPDATE;", // 不完整的UPDATE
            "DELETE;", // 不完整的DELETE
            
            // 更复杂的语法错误
            "SELECT * FROM WHERE age > 18;", // 缺少表名
            "CREATE TABLE users (;", // 不完整的列定义
            "SELECT * FROM users WHERE;", // 不完整的WHERE条件
            "INSERT INTO users VALUES;", // 缺少值
            "UPDATE users SET WHERE id = 1;", // 缺少SET内容
            
            // 表达式错误
            "SELECT * FROM users WHERE age > ;", // 不完整的比较
            "SELECT * FROM users WHERE (age > 18;", // 括号不匹配
            "SELECT * FROM users WHERE age > 18 AND;", // 不完整的逻辑表达式
            "SELECT COUNT( FROM users;", // 不完整的函数调用
            "SELECT * FROM users ORDER BY;", // 缺少排序列
            
            // 数据类型错误
            "CREATE TABLE users (id INT, name VARCHAR);", // 缺少VARCHAR长度
            "CREATE TABLE users (id , name VARCHAR(50));", // 缺少数据类型
            "INSERT INTO users VALUES ('name', 'invalid_number');", // 类型不匹配的值
            
            // 复杂嵌套错误
            "SELECT * FROM users WHERE id IN (SELECT FROM orders);", // 子查询缺少列
            "SELECT COUNT(*), name FROM users;", // 聚合和非聚合混合但没有GROUP BY
            "UPDATE users SET name = (SELECT name FROM) WHERE id = 1;", // 不完整的子查询
        };
        
        int errorCount = 0;
        int successCount = 0;
        
        foreach (var sql in errorSQLs) {
            try {
                Console.WriteLine($"测试错误SQL: {sql}");
                var parser = new Parser(sql);
                var result = parser.ParseStatement();
                Console.WriteLine(result);
                Console.WriteLine($"❌ 应该出错但没有出错: {sql}");
                successCount++;
            }
            catch (Exception ex) {
                Console.WriteLine($"✅ 正确捕获错误: {ex.Message}");
                errorCount++;
            }
        }
        
        Console.WriteLine($"\n错误处理测试总结: 总共 {errorSQLs.Length} 个测试，正确捕获错误 {errorCount} 个，意外成功 {successCount} 个");
    }
    
    /// <summary>
    /// 运行所有测试
    /// </summary>
    public static void RunAllTests() {
        Console.WriteLine("开始运行 Parser 单元测试...\n");
        
        TestCreateTableStatement();
        TestSelectStatement();
        TestComplexSelectStatement();
        TestInsertStatement();
        TestUpdateStatement();
        TestDeleteStatement();
        TestSubqueryStatement();
        TestDropTableStatement();
        TestExpressionParsing();
        TestErrorHandling();
        
        Console.WriteLine("\n=== 所有 Parser 测试完成 ===");
        Console.WriteLine("如果你看到这行消息，说明测试套件已完整运行！");
        Console.WriteLine("请查看上方的测试结果，✅ 表示成功，❌ 表示失败");
    }
}