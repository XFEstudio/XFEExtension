namespace XFEExtension.NetCore.Analyzer.Test;

internal class TestClass(string name, string description, int age)
{
    public int id;
    private int data;
    public string Name { get; set; } = name;
    public List<List<string>> Tags { get; set; }
    public bool MyProperty { get; set; }
    public MyEnum Enum { get; set; }
    public string Description { get; set; } = description;
    public int Age { get; set; } = age;
}
