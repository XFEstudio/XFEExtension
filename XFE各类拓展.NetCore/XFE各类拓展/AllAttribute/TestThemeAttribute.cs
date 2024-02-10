namespace XFE各类拓展.NetCore.XUnit;

/// <summary>
/// 测试颜色主题
/// </summary>
/// <param name="mainColor">主色</param>
/// <param name="classColor">类颜色</param>
/// <param name="classBorderColor">类边框颜色</param>
/// <param name="methodColor">方法颜色</param>
/// <param name="methodBorderColor">方法边框颜色</param>
/// <param name="successColor">成功颜色</param>
/// <param name="failColor">失败颜色</param>
/// <param name="timeColor">时间颜色</param>
/// <param name="counterColor">计数器颜色</param>
[AttributeUsage(AttributeTargets.Class)]
public sealed class TestThemeAttribute(ConsoleColor mainColor = ConsoleColor.Blue, ConsoleColor classColor = ConsoleColor.Green, ConsoleColor classBorderColor = ConsoleColor.DarkGreen, ConsoleColor methodColor = ConsoleColor.Yellow, ConsoleColor methodBorderColor = ConsoleColor.DarkYellow, ConsoleColor successColor = ConsoleColor.Green, ConsoleColor failColor = ConsoleColor.Red, ConsoleColor timeColor = ConsoleColor.Cyan, ConsoleColor counterColor = ConsoleColor.Gray) : Attribute
{
    /// <summary>
    /// 主色
    /// </summary>
    public ConsoleColor MainColor { get; set; } = mainColor;
    /// <summary>
    /// 类颜色
    /// </summary>
    public ConsoleColor ClassColor { get; set; } = classColor;
    /// <summary>
    /// 类边框颜色
    /// </summary>
    public ConsoleColor ClassBorderColor { get; set; } = classBorderColor;
    /// <summary>
    /// 方法颜色
    /// </summary>
    public ConsoleColor MethodColor { get; set; } = methodColor;
    /// <summary>
    /// 方法边框颜色
    /// </summary>
    public ConsoleColor MethodBorderColor { get; set; } = methodBorderColor;
    /// <summary>
    /// 成功颜色
    /// </summary>
    public ConsoleColor SuccessColor { get; set; } = successColor;
    /// <summary>
    /// 失败颜色
    /// </summary>
    public ConsoleColor FailColor { get; set; } = failColor;
    /// <summary>
    /// 时间颜色
    /// </summary>
    public ConsoleColor TimeColor { get; set; } = timeColor;
    /// <summary>
    /// 计数器颜色
    /// </summary>
    public ConsoleColor CounterColor { get; set; } = counterColor;
}
