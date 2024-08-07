using System.Runtime.CompilerServices;

namespace XFEExtension.NetCore.TaskExtension;

/// <summary>
/// 传入值并终止等待器
/// </summary>
/// <typeparam name="T">泛型</typeparam>
/// <param name="taskId">任务ID</param>
/// <param name="result">目标等待器返回值</param>
public delegate void EndTaskTrigger<T>(T result, string taskId = "DEFAULTTASKID");

/// <summary>
/// XFE等待器
/// </summary>
/// <typeparam name="T">泛型</typeparam>
public class XFEWaitTask<T>
{
    private TaskCompletionSource<T> WaitTaskSource { get; set; } = new TaskCompletionSource<T>();
    /// <summary>
    /// 任务ID
    /// </summary>
    public string TaskId { get; set; }
    /// <summary>
    /// 结束任务并返回
    /// </summary>
    /// <param name="result">返回结果</param>
    /// <param name="taskId">任务ID</param>
    public void EndTask(T result, string taskId)
    {
        if (taskId == TaskId)
            WaitTaskSource.SetResult(result);
    }
    /// <summary>
    /// 获取等待器
    /// </summary>
    /// <returns></returns>
    public TaskAwaiter<T> GetAwaiter() => WaitTaskSource.Task.GetAwaiter();
    /// <summary>
    /// XFE等待器
    /// </summary>
    /// <param name="endTaskAndReturn">触发任务结束事件</param>
    /// <param name="taskId">任务ID</param>
    public XFEWaitTask(ref EndTaskTrigger<T> endTaskAndReturn, string taskId = "DEFAULTTASKID")
    {
        endTaskAndReturn += EndTask;
        TaskId = taskId;
    }
}
