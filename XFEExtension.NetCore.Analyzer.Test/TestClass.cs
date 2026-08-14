namespace XFEExtension.NetCore.Analyzer.Test;

internal class TestClass(string name, string description, int age)
{
    /// <summary>
    /// 用于验证字段分析功能的整数标识。
    /// </summary>
    public int id;
    private int data;
    /// <summary>
    /// 获取用于验证空对象分析功能的测试对象。
    /// </summary>
    public EmptyClass EPClass { get; } = new();
    /// <summary>
    /// 获取或设置用于验证无符号整数分析功能的数值。
    /// </summary>
    public ushort OS { get; set; }
    /// <summary>
    /// 获取或设置测试对象的名称。
    /// </summary>
    public string Name { get; set; } = name;
    /// <summary>
    /// 获取或设置用于验证嵌套列表分析功能的标签集合。
    /// </summary>
    public List<List<string>> Tags { get; set; }
    /// <summary>
    /// 获取或设置用于验证布尔属性分析功能的值。
    /// </summary>
    public bool MyProperty { get; set; }
    /// <summary>
    /// 获取或设置用于验证枚举分析功能的枚举值。
    /// </summary>
    public MyEnum Enum { get; set; }
    /// <summary>
    /// 获取或设置用于验证标志枚举分析功能的组合枚举值。
    /// </summary>
    public MyMultiEnum MultiEnum { get; set; }
    /// <summary>
    /// 获取或设置测试对象的描述。
    /// </summary>
    public string Description { get; set; } = description;
    /// <summary>
    /// 获取或设置测试对象的年龄。
    /// </summary>
    public int Age { get; set; } = age;
}

internal class EmptyClass { }
