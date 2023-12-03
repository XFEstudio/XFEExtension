namespace XFE各类拓展.NetCore;

/// <summary>
/// 用于计时的特性
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class SMTestAttribute : XFETestAttributeBase
{
    /// <summary>
    /// 计算执行一段代码所需时间
    /// </summary>
    /// <param name="values">传参</param>
    public SMTestAttribute(params object[] values)
    {
        Params = values;
    }
    /// <summary>
    /// 计算执行一段代码所需时间
    /// </summary>
    public SMTestAttribute() { }
}
