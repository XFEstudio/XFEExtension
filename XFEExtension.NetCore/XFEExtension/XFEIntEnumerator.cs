namespace XFEExtension.NetCore.XFECode;

/// <summary>
/// XFE枚举器
/// </summary>
/// <param name="range">范围运算符</param>
public class XFEIntEnumerator(Range range)
{
    /// <summary>
    /// 开始
    /// </summary>
    public int Start { get; } = range.Start.Value - 1;
    /// <summary>
    /// 结束
    /// </summary>
    public int End { get; } = range.End.Value;
    /// <summary>
    /// 当前
    /// </summary>
    public int Current { get; set; } = range.Start.Value - 1;
    /// <summary>
    /// 下一个
    /// </summary>
    /// <returns>是否为最后一个</returns>
    public bool MoveNext()
    {
        Current++;
        return Current <= End;
    }
}