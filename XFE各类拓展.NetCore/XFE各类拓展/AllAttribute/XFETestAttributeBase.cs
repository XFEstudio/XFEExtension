namespace XFE各类拓展.NetCore.XUnit;

/// <summary>
/// 测试特性基类
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class XFETestAttributeBase : Attribute
{
    /// <summary>
    /// 参数
    /// </summary>
    public object[]? Params { get; set; }
}