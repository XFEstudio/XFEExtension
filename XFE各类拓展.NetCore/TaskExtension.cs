using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace XFE各类拓展.TaskExtension
{
    /// <summary>
    /// 任务的拓展
    /// </summary>
    public static class TaskExtension
    {
        private static long CTimeCounter = 0;
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
            CTimeCounter++;
            var elapsedTime = timeCounter.Elapsed;
            if (autoOutPut)
                if (elapsedTime.TotalHours >= 1)
                    Console.WriteLine($"标识名：{timerName}\t执行批次：{CTimeCounter}\t执行时间：{elapsedTime}");
                else if (elapsedTime.TotalMinutes >= 1)
                    Console.WriteLine($"标识名：{timerName}\t执行批次：{CTimeCounter}\t执行时间: {Math.Floor(elapsedTime.TotalMinutes)} 分 {elapsedTime.Seconds} 秒");
                else if (elapsedTime.TotalSeconds >= 1)
                    Console.WriteLine($"标识名：{timerName}\t执行批次：{CTimeCounter}\t执行时间: {elapsedTime.TotalSeconds:F3} 秒");
                else if (elapsedTime.TotalMilliseconds >= 1)
                    Console.WriteLine($"标识名：{timerName}\t执行批次：{CTimeCounter}\t执行时间: {elapsedTime.TotalMilliseconds:F3} 毫秒");
                else
                    Console.WriteLine($"标识名：{timerName}\t执行批次：{CTimeCounter}\t执行时间: {elapsedTime.TotalMilliseconds * 1000:F3} 纳秒");
            return elapsedTime;
        }
    }
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
        public TaskAwaiter<T> GetAwaiter()
        {
            return WaitTaskSource.Task.GetAwaiter();
        }
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
    /// <summary>
    /// 任务同步器
    /// </summary>
    public class TaskSynchronizer
    {
        /// <summary>
        /// 当前任务
        /// </summary>
        public Task CurrentTask { get; set; }
        /// <summary>
        /// 返回当前任务的等待器
        /// </summary>
        /// <returns></returns>
        public TaskAwaiter GetAwaiter()
        {
            if (CurrentTask != null)
                return CurrentTask.GetAwaiter();
            else
                return Task.CompletedTask.GetAwaiter();
        }
        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="task">待执行任务</param>
        public void AddTask(Task task)
        {
            CurrentTask = task;
        }
        /// <summary>
        /// 任务同步器
        /// </summary>
        public TaskSynchronizer() { }
        /// <summary>
        /// 任务同步器
        /// </summary>
        /// <param name="task">第一个待执行任务</param>
        public TaskSynchronizer(Task task)
        {
            CurrentTask = task;
        }
    }
}