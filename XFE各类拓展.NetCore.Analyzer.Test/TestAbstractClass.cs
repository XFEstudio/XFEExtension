using XFE各类拓展.NetCore.ImplExtension;

namespace XFE各类拓展.NetCore.Analyzer.Test
{
    
    [CreateImpl]
    public abstract class Animal(string name, int age)
    {
        public string Name { get; set; } = name;
        public int Age { get; set; } = age;
    }
}
