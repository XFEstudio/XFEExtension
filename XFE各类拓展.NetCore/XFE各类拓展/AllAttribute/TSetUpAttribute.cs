namespace XFE各类拓展.NetCore;

/// <summary>
/// 用于标记测试类初始函数的特性
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class TSetUpAttribute : XFETestAttributeBase
{
    /// <summary>
    /// 用于标记测试类构造函数的特性
    /// </summary>
    /// <param name="values"></param>
    public TSetUpAttribute(params object[] values)
    {
        Params = values;
    }
}
