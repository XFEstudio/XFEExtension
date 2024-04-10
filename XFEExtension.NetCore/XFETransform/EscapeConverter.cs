﻿namespace XFEExtension.NetCore.XFETransform;

/// <summary>
/// 转义器
/// </summary>
public class EscapeConverter(string escapeSymbol = "/", params string[] escapes)
{
    /// <summary>
    /// 转义用符号
    /// </summary>
    public string EscapeSymbol { get; set; } = escapeSymbol;
    /// <summary>
    /// 待转义的符号
    /// </summary>
    public string[] Escapes { get; set; } = escapes;
    /// <summary>
    /// 转义（生成转义后文本）
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public string Convert(string str)
    {
        str = ConvertEscapeSymbol(str);
        foreach (string escape in Escapes)
        {
            str = str.Replace(escape, $"{EscapeSymbol}{escape}");
        }
        return str;
    }
    /// <summary>
    /// 逆转义（生成原文）
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public string Inverse(string str)
    {
        foreach (string escape in Escapes)
        {
            str = str.Replace($"{EscapeSymbol}{escape}", escape);
        }
        str = InverseEscapeSymbol(str);
        return str;
    }
    private string ConvertEscapeSymbol(string str) => str.Replace(EscapeSymbol, $"{EscapeSymbol}{EscapeSymbol}");
    private string InverseEscapeSymbol(string str) => str.Replace($"{EscapeSymbol}{EscapeSymbol}", EscapeSymbol);
}
