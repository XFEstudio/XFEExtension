namespace XFE各类拓展.NetCore.XUnit;

/// <summary>
/// 用于标记测试类初始函数的特性
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class SetUpAttribute : XFETestAttributeBase
{
    /// <summary>
    /// 用于标记测试类构造函数的特性
    /// </summary>
    /// <param name="values"></param>
    public SetUpAttribute(params object[] values)
    {
        Params = values;
    }
}
