﻿using XFE各类拓展.NetCore.ProfileExtension;

namespace XFE各类拓展.NetCore.Analyzer.Test;

[AutoLoadProfile]
public static partial class SystemProfile
{
    [ProfileProperty]
    private static string name = string.Empty;
    [ProfileProperty]
    private static int _age;
}
