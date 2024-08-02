using System.Diagnostics;

namespace XFEExtension.NetCore.TaskExtension;

/// <summary>
/// 任务的拓展
/// </summary>
public static class TaskExtension
{
    private static long cTimeCounter = 0;
    #region 新建任务并开始
    /// <summary>
    /// 新建一个任务并开始
    /// </summary>
    /// <param name="action"></param>
    /// <returns>运行的任务</returns>
    public static Task StartNewTask(this Action action)
    {
        return Task.Run(action);
    }
    /// <summary>
    /// 新建一个任务并开始
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="action"></param>
    /// <param name="t">对象</param>
    /// <returns></returns>
    public static Task StartNewTask<T>(this Action<T> action, T t)
    {
        return Task.Run(() => action(t));
    }
    /// <summary>
    /// 新建一个任务并开始
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    public static async Task<T> StartNewTask<T>(this Func<T> func)
    {
        return await Task.Run(func);
    }
    #endregion
    /// <summary>
    /// 等待任务完成
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="task"></param>
    /// <returns></returns>
    public static T WaitAndGetResult<T>(this Task<T> task)
    {
        task.Wait();
        return task.Result;
    }
    /// <summary>
    /// 计算一段代码执行所需时间
    /// </summary>
    /// <param name="action"></param>
    /// <param name="autoOutPut">自动输出</param>
    /// <param name="timerName">该次计时标识名</param>
    /// <returns></returns>
    public static TimeSpan CTime(this Action action, bool autoOutPut = true, string timerName = "无标识名计时器")
    {
        var timeCounter = new Stopwatch();
        timeCounter.Start();
        action?.Invoke();
        timeCounter.Stop();
        cTimeCounter++;
        var elapsedTime = timeCounter.Elapsed;
        if (autoOutPut)
            if (elapsedTime.TotalHours >= 1)
                Console.WriteLine($"标识名：{timerName}\t执行批次：{cTimeCounter}\t执行时间：{elapsedTime}");
            else if (elapsedTime.TotalMinutes >= 1)
                Console.WriteLine($"标识名：{timerName}\t执行批次：{cTimeCounter}\t执行时间: {Math.Floor(elapsedTime.TotalMinutes)} 分 {elapsedTime.Seconds} 秒");
            else if (elapsedTime.TotalSeconds >= 1)
                Console.WriteLine($"标识名：{timerName}\t执行批次：{cTimeCounter}\t执行时间: {elapsedTime.TotalSeconds:F3} 秒");
            else if (elapsedTime.TotalMilliseconds >= 1)
                Console.WriteLine($"标识名：{timerName}\t执行批次：{cTimeCounter}\t执行时间: {elapsedTime.TotalMilliseconds:F3} 毫秒");
            else if (elapsedTime.TotalMicroseconds >= 1)
                Console.WriteLine($"标识名：{timerName}\t执行批次：{cTimeCounter}\t执行时间: {elapsedTime.TotalMicroseconds:F3} 微秒");
            else
                Console.WriteLine($"标识名：{timerName}\t执行批次：{cTimeCounter}\t执行时间: {elapsedTime.TotalNanoseconds:F3} 纳秒");
        return elapsedTime;
    }
}