using System.Runtime.CompilerServices;

namespace XFEExtension.NetCore.XFECode;

/// <summary>
/// 对代码整体书写习惯的拓展
/// </summary>
public static class XFEExtension
{
    /// <summary>
    /// 使得计时器可以直接调用（单位：秒）
    /// </summary>
    /// <param name="waitTime"></param>
    /// <returns></returns>
    public static TaskAwaiter GetAwaiter(this int waitTime)
        => Task.Delay(TimeSpan.FromSeconds(waitTime)).GetAwaiter();
    /// <summary>
    /// 使得计时器可以直接调用（单位：秒）
    /// </summary>
    /// <param name="waitTime"></param>
    /// <returns></returns>
    public static TaskAwaiter GetAwaiter(this double waitTime)
        => Task.Delay(TimeSpan.FromSeconds(waitTime)).GetAwaiter();
    /// <summary>
    /// 获取枚举器
    /// </summary>
    /// <param name="range">范围运算符</param>
    /// <returns></returns>
    public static XFEIntEnumerator GetEnumerator(this Range range)
    {
        return new XFEIntEnumerator(range);
    }
    /// <summary>
    /// 获取枚举器
    /// </summary>
    /// <param name="count">计数</param>
    /// <returns></returns>
    public static XFEIntEnumerator GetEnumerator(this int count)
    {
        return new XFEIntEnumerator(..count);
    }
}