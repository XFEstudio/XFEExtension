namespace XFE各类拓展.NetCore.ProfileExtension;

/// <summary>
/// 向属性的Set方法中添加代码
/// </summary>
/// <param name="funcString">需要添加的代码</param>
[AttributeUsage(AttributeTargets.Field,AllowMultiple = true)]
public class ProfilePropertyAddSetAttribute(string funcString) : Attribute
{
    /// <summary>
    /// 需要添加的代码
    /// </summary>
    public string FuncString { get; set; } = funcString;
}
