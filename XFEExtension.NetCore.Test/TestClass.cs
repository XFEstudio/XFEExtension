namespace XFEExtension.NetCore.Test;

internal class TestClass(string name, string description, int age)
{
    /// <summary>
    /// 用于验证字段转换功能的整数标识。
    /// </summary>
    public int id;
    private int data;
    /// <summary>
    /// 获取用于验证空对象转换功能的测试对象。
    /// </summary>
    public EmptyClass EPClass { get; } = new();
    /// <summary>
    /// 获取或设置用于验证无符号整数转换功能的数值。
    /// </summary>
    public ushort OS { get; set; }
    /// <summary>
    /// 获取或设置测试对象的名称。
    /// </summary>
    public string Name { get; set; } = name;
    /// <summary>
    /// 获取或设置用于验证嵌套列表转换功能的标签集合。
    /// </summary>
    public List<List<string>> Tags { get; set; }
    /// <summary>
    /// 获取或设置用于验证布尔属性转换功能的值。
    /// </summary>
    public bool MyProperty { get; set; }
    /// <summary>
    /// 用于验证可空子对象转换功能的子对象。
    /// </summary>
    public SubClass? SubClass;
    /// <summary>
    /// 获取或设置用于验证枚举转换功能的枚举值。
    /// </summary>
    public MyEnum Enum { get; set; }
    /// <summary>
    /// 获取或设置测试对象的描述。
    /// </summary>
    public string Description { get; set; } = description;
    /// <summary>
    /// 获取或设置测试对象的年龄。
    /// </summary>
    public int Age { get; set; } = age;
}

class SubClass
{
    /// <summary>
    /// 获取用于验证嵌套对象转换功能的内部对象。
    /// </summary>
    public required InnerClass InnerClass;
}

class InnerClass
{
    /// <summary>
    /// 获取内部测试对象的名称。
    /// </summary>
    public required string Name;
    /// <summary>
    /// 获取内部测试对象的描述。
    /// </summary>
    public required string Description;
}

internal class EmptyClass { }
enum MyEnum
{
    Test1 = 3,
    Test2 = 12,
    Test3 = 1
}
