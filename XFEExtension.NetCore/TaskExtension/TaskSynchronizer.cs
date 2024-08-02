using System.Runtime.CompilerServices;

namespace XFEExtension.NetCore.TaskExtension;

/// <summary>
/// 任务同步器
/// </summary>
public class TaskSynchronizer
{
    /// <summary>
    /// 当前任务
    /// </summary>
    public Task? CurrentTask { get; set; }
    /// <summary>
    /// 返回当前任务的等待器
    /// </summary>
    /// <returns></returns>
    public TaskAwaiter GetAwaiter()
    {
        if (CurrentTask is not null)
            return CurrentTask.GetAwaiter();
        else
            return Task.CompletedTask.GetAwaiter();
    }
    /// <summary>
    /// 添加任务
    /// </summary>
    /// <param name="task">待执行任务</param>
    public void AddTask(Task task) => CurrentTask = task;
    /// <summary>
    /// 任务同步器
    /// </summary>
    public TaskSynchronizer() { }
    /// <summary>
    /// 任务同步器
    /// </summary>
    /// <param name="task">第一个待执行任务</param>
    public TaskSynchronizer(Task task) => CurrentTask = task;
}