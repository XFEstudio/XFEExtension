namespace XFE各类拓展.NetCore.XUnit;

/// <summary>
/// 用于计时的特性，可自定义名称和返回值
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class SMNRTestAttribute : SMRTestAttribute
{
    /// <summary>
    /// 该次计时标识名
    /// </summary>
    public string? TimerName { get; set; }
    /// <summary>
    /// 计算执行一段代码所需时间
    /// </summary>
    /// <param name="timerName">计时器名称</param>
    /// <param name="valuesAndResult">传参</param>
    public SMNRTestAttribute(string timerName, params object[] valuesAndResult)
    {
        TimerName = timerName;
        ReturnValue = valuesAndResult[^1];
        Params = valuesAndResult.Take(valuesAndResult.Length - 1).ToArray();
    }
    /// <summary>
    /// 计算执行一段代码所需时间
    /// </summary>
    public SMNRTestAttribute() { }
}
