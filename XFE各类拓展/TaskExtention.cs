using System;
using System.Threading.Tasks;

namespace XFE各类拓展.TaskExtension
{
    /// <summary>
    /// 任务的拓展
    /// </summary>
    public static class TaskExtension
    {
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
    }
}