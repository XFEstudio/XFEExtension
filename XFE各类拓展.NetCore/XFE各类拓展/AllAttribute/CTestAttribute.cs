namespace XFE各类拓展.NetCore;

/// <summary>
/// 类测试
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class CTestAttribute : XFETestAttributeBase
{
    /// <summary>
    /// 类测试
    /// </summary>
    /// <param name="values">创建类时构造方法的传参</param>
    public CTestAttribute(params object[] values)
    {
        Params = values;
    }
    /// <summary>
    /// 类测试
    /// </summary>
    public CTestAttribute() { }
}
