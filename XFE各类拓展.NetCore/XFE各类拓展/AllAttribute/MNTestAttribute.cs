namespace XFE各类拓展.NetCore;

/// <summary>
/// 方法测试，可自定义名称
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class MNTestAttribute : MTestAttribute
{
    /// <summary>
    /// 方法别名
    /// </summary>
    public string? MethodOtherName { get; set; }
    /// <summary>
    /// 方法测试
    /// </summary>
    /// <param name="methodOtherName">方法别名</param>
    /// <param name="values">传参</param>
    public MNTestAttribute(string methodOtherName, params object[] values)
    {
        MethodOtherName = methodOtherName;
        Params = values;
    }
    /// <summary>
    /// 方法测试
    /// </summary>
    /// <param name="methodOtherName">方法别名</param>
    public MNTestAttribute(string methodOtherName)
    {
        MethodOtherName = methodOtherName;
    }
    /// <summary>
    /// 方法测试
    /// </summary>
    public MNTestAttribute() { }
}
