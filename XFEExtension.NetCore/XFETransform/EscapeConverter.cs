namespace XFEExtension.NetCore.XFETransform;

/// <summary>
/// 转义器
/// </summary>
/// <param name="escape">待转义字符</param>
/// <param name="startEscapeSymbol">起始转义用符号</param>
/// <param name="repeatEscapeSymbol">重复转义字符</param>
/// <param name="endEscapeSymbol">结束转义用符号</param>
public class EscapeConverter(string escape, string startEscapeSymbol, string repeatEscapeSymbol, string endEscapeSymbol)
{
    /// <summary>
    /// 起始转义符号
    /// </summary>
    public string StartEscapeSymbol { get; set; } = startEscapeSymbol;
    /// <summary>
    /// 重复转义符号
    /// </summary>
    public string RepeatEscapeSymbol { get; set; } = repeatEscapeSymbol;
    /// <summary>
    /// 结束转义符号
    /// </summary>
    public string EndEscapeSymbol { get; set; } = endEscapeSymbol;
    /// <summary>
    /// 待转义的符号
    /// </summary>
    public string Escape { get; set; } = escape;
    /// <summary>
    /// 转义（生成转义后文本）
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public string Convert(string str)
    {
        str = ConvertEscapeSymbol(str);
        str = str.Replace(Escape, $"{StartEscapeSymbol}{RepeatEscapeSymbol}{EndEscapeSymbol}");
        return str;
    }
    /// <summary>
    /// 逆转义（生成原文）
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public string Inverse(string str)
    {
        str = str.Replace($"{StartEscapeSymbol}{RepeatEscapeSymbol}{EndEscapeSymbol}", Escape);
        str = InverseEscapeSymbol(str);
        return str;
    }
    private string ConvertEscapeSymbol(string str) => str.Replace($"{StartEscapeSymbol}{RepeatEscapeSymbol}", $"{StartEscapeSymbol}{RepeatEscapeSymbol}{RepeatEscapeSymbol}");
    private string InverseEscapeSymbol(string str) => str.Replace($"{StartEscapeSymbol}{RepeatEscapeSymbol}{RepeatEscapeSymbol}", $"{StartEscapeSymbol}{RepeatEscapeSymbol}");
}
