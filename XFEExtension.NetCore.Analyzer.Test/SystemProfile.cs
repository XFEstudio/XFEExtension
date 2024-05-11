using XFEExtension.NetCore.ProfileExtension;

namespace XFEExtension.NetCore.Analyzer.Test;

public partial class SystemProfile
{
    [ProfileProperty]
    int _age = 1;
    [ProfileProperty]
    string name  = "12";
    static partial void GetNameProperty()
    {
        Console.WriteLine("获取了Name");
    }
    static partial void SetNameProperty(string value)
    {
        Console.WriteLine($"设置了Name：从{Name}变为了{value}");
    }

    static partial void GetAgeProperty()
    {
        Console.WriteLine("获取了Age");
    }

    static partial void SetAgeProperty(int value)
    {
        Console.WriteLine($"设置了Age：从{Age}变为了{value}");
    }
}
