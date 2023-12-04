using XFE各类拓展.NetCore.XFE各类拓展.AllAttribute;

namespace XFE各类拓展.NetCore;

/// <summary>
/// 方法测试
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class MTestAttribute : XFETestAttributeBase
{
    /// <summary>
    /// 方法测试
    /// </summary>
    /// <param name="values">传参</param>
    public MTestAttribute(params object[] values)
    {
        Params = values;
    }
    /// <summary>
    /// 方法测试
    /// </summary>
    public MTestAttribute() { }
}
