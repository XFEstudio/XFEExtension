namespace XFE各类拓展.NetCore.XUnit;

/// <summary>
/// 类测试
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class CNTestAttribute : CTestAttribute
{
    /// <summary>
    /// 类别名
    /// </summary>
    public string ClassOtherName { get; set; }
    /// <summary>
    /// 类测试
    /// </summary>
    /// <param name="values">创建类时构造方法的传参</param>
    /// <param name="classOtherName">类别名</param>
    public CNTestAttribute(string classOtherName, params object[] values)
    {
        Params = values;
        ClassOtherName = classOtherName;
    }
}
