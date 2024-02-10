namespace XFE各类拓展.NetCore.XUnit;

/// <summary>
/// 用于计时的特性，可自定义名称
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class SMNTestAttribute : SMTestAttribute
{
    /// <summary>
    /// 该次计时标识名
    /// </summary>
    public string TimerName { get; set; }
    /// <summary>
    /// 计算执行一段代码所需时间
    /// </summary>
    /// <param name="timerName">计时器名称</param>
    /// <param name="values">传参</param>
    public SMNTestAttribute(string timerName, params object[] values)
    {
        TimerName = timerName;
        Params = values;
    }
    /// <summary>
    /// 计算执行一段代码所需时间
    /// </summary>
    public SMNTestAttribute(string timerName)
    {
        TimerName = timerName;
    }
}
