namespace LiteDatabase.Utils;

/// <summary>
/// 字符工具类
/// </summary>
public static class CharUtils {
    
    /// <summary>
    /// 判断字符是否是SQL允许的符号
    /// </summary>
    public static bool IsPunct(char c) {
        return "+-*/%=<>(),;.!".Contains(c);
    }

}