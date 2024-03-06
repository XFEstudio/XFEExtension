﻿namespace XFE各类拓展.NetCore.ProfileExtension;

/// <summary>
/// 向属性的Get方法中添加代码
/// </summary>
/// <param name="funcString">需要添加的代码</param>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class ProfilePropertyAddGetAttribute(string funcString) : Attribute
{
    /// <summary>
    /// 需要添加的代码
    /// </summary>
    internal string FuncString { get; set; } = funcString;
}
