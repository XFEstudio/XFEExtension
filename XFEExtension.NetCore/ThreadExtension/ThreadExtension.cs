using System.Runtime.Versioning;

namespace XFEExtension.NetCore.ThreadExtension;

/// <summary>
/// 线程的拓展
/// </summary>
public static class ThreadExtension
{
    #region 等待线程状态

    /// <param name="threads"></param>
    extension(List<Thread> threads)
    {
        /// <summary>
        /// 等待指定线程组的所有线程完成
        /// </summary>
        /// <returns>等待任务</returns>
        public Task WaitThreadListComplete()
        {
            return Task.Run(() =>
            {
                while (true)
                {
                    bool isAllThreadComplete = true;
                    foreach (var thread in threads)
                    {
                        if (thread.ThreadState != ThreadState.Stopped)
                        {
                            isAllThreadComplete = false;
                            break;
                        }
                    }
                    if (isAllThreadComplete)
                    {
                        break;
                    }
                }
            });
        }

        /// <summary>
        /// 等待指定线程组的所有线程达到指定状态
        /// </summary>
        /// <param name="threadState">线程的状态</param>
        /// <returns></returns>
        public Task WaitThreadListComplete(ThreadState threadState)
        {
            return Task.Run(() =>
            {
                while (true)
                {
                    bool isAllThreadComplete = true;
                    foreach (var thread in threads)
                    {
                        if (thread.ThreadState != threadState)
                        {
                            isAllThreadComplete = false;
                            break;
                        }
                    }
                    if (isAllThreadComplete)
                    {
                        break;
                    }
                }
            });
        }
    }

    /// <param name="thread"></param>
    extension(Thread thread)
    {
        /// <summary>
        /// 等待指定线程完成
        /// </summary>
        /// <returns>等待任务</returns>
        public Task WaitThreadComplete()
        {
            return Task.Run(() => { while (thread.ThreadState != ThreadState.Stopped) ; });
        }

        /// <summary>
        /// 等待指定线程达到指定状态
        /// </summary>
        /// <param name="threadState">线程的状态</param>
        /// <returns></returns>
        public Task WaitThreadComplete(ThreadState threadState)
        {
            return Task.Run(() => { while (thread.ThreadState != threadState) ; });
        }
    }

    #endregion
    #region 新建线程并开始
    /// <summary>
    /// 新建一个线程并开始
    /// </summary>
    /// <param name="method">线程方法</param>
    /// <param name="parameter">传递的参数</param>
    /// <returns></returns>
    public static Thread StartNewThread(ParameterizedThreadStart method, object parameter)
    {
        Thread thread = new(method);
        thread.Start(parameter);
        return thread;
    }
    /// <summary>
    /// 新建一个线程并开始
    /// </summary>
    /// <param name="method">线程方法</param>
    /// <returns></returns>
    public static Thread StartNewThread(ThreadStart method)
    {
        Thread thread = new(method);
        thread.Start();
        return thread;
    }
    /// <summary>
    /// 新建一个线程并开始
    /// </summary>
    /// <param name="method">线程方法</param>
    /// <param name="parameter">传递的参数</param>
    /// <param name="apartmentState">线程的状态</param>
    /// <returns></returns>
    [SupportedOSPlatform("windows")]
    public static Thread StartNewThread(ParameterizedThreadStart method, object parameter, ApartmentState apartmentState)
    {
        Thread thread = new(method);
#if WINDOWS
        thread.SetApartmentState(apartmentState);
#endif
        thread.Start(parameter);
        return thread;
    }
    /// <summary>
    /// 新建一个线程并开始
    /// </summary>
    /// <param name="method">线程方法</param>
    /// <param name="apartmentState">线程的状态</param>
    /// <returns></returns>
    [SupportedOSPlatform("windows")]
    public static Thread StartNewThread(ThreadStart method, ApartmentState apartmentState)
    {
        Thread thread = new(method);
#if WINDOWS
        thread.SetApartmentState(apartmentState);
#endif
        thread.Start();
        return thread;
    }
    #endregion
}