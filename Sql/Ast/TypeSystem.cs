namespace LiteDatabase.Sql.Ast;

/// <summary>
/// 类型系统工具类 - 处理类型转换和兼容性检查
/// </summary>
public static class TypeSystem
{
    /// <summary>
    /// 检查 SqlValue 的 ValueType 是否与列的 ColumnType 兼容
    /// </summary>
    /// <param name="valueType">值的类型</param>
    /// <param name="columnType">列的类型</param>
    /// <returns>是否兼容</returns>
    public static bool IsCompatible(ValueType valueType, ColumnType columnType)
    {
        return (valueType, columnType) switch
        {
            // 精确匹配
            (ValueType.Int, ColumnType.Int) => true,
            (ValueType.Float, ColumnType.Float) => true,
            (ValueType.String, ColumnType.String) => true,
            (ValueType.True, ColumnType.Bool) => true,
            (ValueType.False, ColumnType.Bool) => true,
            
            // NULL 可以赋值给任何类型（如果列允许NULL的话）
            (ValueType.Null, _) => true,
            
            // 数值类型的隐式转换
            (ValueType.Int, ColumnType.Float) => true,  // int 可以转换为 float
            
            // 其他情况都不兼容
            _ => false
        };
    }
}