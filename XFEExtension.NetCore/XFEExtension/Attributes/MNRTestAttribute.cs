namespace XFEExtension.NetCore.XUnit;

/// <summary>
/// 方法测试，可自定义名称和返回值
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class MNRTestAttribute : MRTestAttribute
{
    /// <summary>
    /// 方法别名
    /// </summary>
    public string? MethodOtherName { get; set; }
    /// <summary>
    /// 方法测试
    /// </summary>
    /// <param name="methodOtherName">方法别名</param>
    /// <param name="valuesAndResult">传参</param>
    public MNRTestAttribute(string methodOtherName, params object[] valuesAndResult)
    {
        MethodOtherName = methodOtherName;
        ReturnValue = valuesAndResult[valuesAndResult.Length - 1];
        Params = valuesAndResult.Take(valuesAndResult.Length - 1).ToArray();
    }
    /// <summary>
    /// 方法测试
    /// </summary>
    public MNRTestAttribute() { }
}
