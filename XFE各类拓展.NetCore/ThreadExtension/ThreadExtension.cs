namespace XFE各类拓展.NetCore.ThreadExtension;

/// <summary>
/// 线程的拓展
/// </summary>
public static class ThreadExtension
{
    #region 等待线程状态
    /// <summary>
    /// 等待指定线程组的所有线程完成
    /// </summary>
    /// <param name="threads"></param>
    /// <returns>等待任务</returns>
    public static Task WaitThreadListComplete(this List<Thread> threads)
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
    /// <param name="threads"></param>
    /// <param name="threadState">线程的状态</param>
    /// <returns></returns>
    public static Task WaitThreadListComplete(this List<Thread> threads, ThreadState threadState)
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
    /// <summary>
    /// 等待指定线程完成
    /// </summary>
    /// <param name="thread"></param>
    /// <returns>等待任务</returns>
    public static Task WaitThreadComplete(this Thread thread)
    {
        return Task.Run(() => { while (thread.ThreadState != ThreadState.Stopped) ; });
    }
    /// <summary>
    /// 等待指定线程达到指定状态
    /// </summary>
    /// <param name="thread"></param>
    /// <param name="threadState">线程的状态</param>
    /// <returns></returns>
    public static Task WaitThreadComplete(this Thread thread, ThreadState threadState)
    {
        return Task.Run(() => { while (thread.ThreadState != threadState) ; });
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
    public static Thread StartNewThread(ParameterizedThreadStart method, object parameter, ApartmentState apartmentState)
    {
        Thread thread = new(method);
#if WINDOWS
#pragma warning disable CA1416 // 验证平台兼容性
        thread.SetApartmentState(apartmentState);
#pragma warning restore CA1416 // 验证平台兼容性
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
    public static Thread StartNewThread(ThreadStart method, ApartmentState apartmentState)
    {
        Thread thread = new(method);
#if WINDOWS
#pragma warning disable CA1416 // 验证平台兼容性
        thread.SetApartmentState(apartmentState);
#pragma warning restore CA1416 // 验证平台兼容性
#endif
        thread.Start();
        return thread;
    }
    #endregion
}