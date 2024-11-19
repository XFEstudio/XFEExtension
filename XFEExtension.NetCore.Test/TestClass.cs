namespace XFEExtension.NetCore.Test;

internal class TestClass(string name, string description, int age)
{
    public int id;
    private int data;
    public EmptyClass EPClass { get; } = new();
    public ushort OS { get; set; }
    public string Name { get; set; } = name;
    public List<List<string>> Tags { get; set; }
    public bool MyProperty { get; set; }
    public SubClass? SubClass;
    public MyEnum Enum { get; set; }
    public string Description { get; set; } = description;
    public int Age { get; set; } = age;
}

class SubClass
{
    public required InnerClass InnerClass;
}

class InnerClass
{
    public required string Name;
    public required string Description;
}

internal class EmptyClass { }
enum MyEnum
{
    Test1 = 3,
    Test2 = 12,
    Test3 = 1
}