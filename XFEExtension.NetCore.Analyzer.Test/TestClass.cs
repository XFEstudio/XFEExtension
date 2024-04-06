namespace XFEExtension.NetCore.Analyzer.Test;

internal class TestClass(string name, string description, int age)
{
    public int id;
    private int data;
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public int Age { get; set; } = age;
    public List<List<string>> Tags { get; set; }
}
