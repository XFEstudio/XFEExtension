using XFEExtension.NetCore.ProfileExtension;

namespace XFEExtension.NetCore.Analyzer.Test;

internal class TestClass
{
    static void Main(string[] args)
    {
        Console.WriteLine(SystemProfile.Name);
        Console.WriteLine(SystemProfile.Age);
        XFEProfile.ImportProfiles("|{+-[+-SystemProfile-+][+-|{++-[++-name-++][++-\"New Name\"-++]-++}||{++-[++-_age-++][++-33-++]-++}|-+]-+}|");
        Console.WriteLine(SystemProfile.Name);
        Console.WriteLine(SystemProfile.Age);
    }
}