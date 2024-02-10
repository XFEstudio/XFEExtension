namespace XFE各类拓展.NetCore.XUnit;

/// <summary>
/// 用于计时的特性，可绑定返回值
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class SMRTestAttribute : SMTestAttribute
{
    /// <summary>
    /// 返回值
    /// </summary>
    public object? ReturnValue { get; set; }
    /// <summary>
    /// 计算执行一段代码所需时间
    /// </summary>
    /// <param name="valuesAndResult">传参</param>
    public SMRTestAttribute(params object[] valuesAndResult)
    {
        ReturnValue = valuesAndResult[^1];
        Params = valuesAndResult.Take(valuesAndResult.Length - 1).ToArray();
    }
    /// <summary>
    /// 计算执行一段代码所需时间
    /// </summary>
    public SMRTestAttribute() { }
}
