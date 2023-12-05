using System.Runtime.CompilerServices;

namespace XFE各类拓展.NetCore.XFECode;

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
    //public static IEnumerator<int> GetEnumerator(this Range range)
    //{

    //}
    public static IEnumerator<int> GetEnumerator(this int count)
    {
        for (int i = 0; i <= count; i++)
        {
            yield return i;
        }
    }
}
public class XFEEnumerator(Range range)
{
    private int current;
    private readonly int end;
    
}