namespace XFEExtension.NetCore;

/// <summary>
/// 流程控制：停止并执行方法
/// </summary>
public class StopAction : Exception
{
    /// <summary>
    /// 执行方法并抛出异常
    /// </summary>
    /// <param name="stopAction">需要执行的方法</param>
    /// <param name="message">异常消息</param>
    public StopAction(Action stopAction, string? message = null) : base(message) => stopAction.Invoke();

    /// <summary>
    /// 执行方法并抛出异常
    /// </summary>
    /// <param name="stopAction">需要执行的方法</param>
    /// <param name="message">异常消息</param>
    /// <param name="innerException">内部错误</param>
    public StopAction(Action stopAction, string? message, Exception? innerException) : base(message, innerException) => stopAction.Invoke();

    /// <summary>
    /// 执行异步方法并抛出异常
    /// </summary>
    /// <param name="stopAction">需要执行的方法</param>
    /// <param name="message">异常消息</param>
    public StopAction(Func<Task> stopAction, string? message = null) : base(message) => stopAction.Invoke().Wait();

    /// <summary>
    /// 执行异步方法并抛出异常
    /// </summary>
    /// <param name="stopAction">需要执行的方法</param>
    /// <param name="message">异常消息</param>
    /// <param name="innerException">内部错误</param>
    public StopAction(Func<Task> stopAction, string? message, Exception? innerException) : base(message, innerException) => stopAction.Invoke().Wait();
}
