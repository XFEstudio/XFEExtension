namespace XFE各类拓展.NetCore.XUnit;

/// <summary>
/// 方法测试，可自定义返回值
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class MRTestAttribute : MTestAttribute
{
    /// <summary>
    /// 返回值
    /// </summary>
    public object? ReturnValue { get; set; }
    /// <summary>
    /// 方法测试
    /// </summary>
    /// <param name="valuesAndResult">传参</param>
    public MRTestAttribute(params object[] valuesAndResult)
    {
        ReturnValue = valuesAndResult[^1];
        Params = valuesAndResult.Take(valuesAndResult.Length - 1).ToArray();
    }
    /// <summary>
    /// 方法测试
    /// </summary>
    public MRTestAttribute() { }
}
