namespace XFEExtension.DelegateExtension
{
    /// <summary>
    /// 指定发送者泛型的委托事件
    /// </summary>
    /// <typeparam name="T">发送者泛型</typeparam>
    /// <param name="sender">发送者</param>
    public delegate void XFEEventHandler<T>(T sender);
    /// <summary>
    /// 指定发送者泛型的委托事件
    /// </summary>
    /// <typeparam name="T1">发送者泛型</typeparam>
    /// <typeparam name="T2">事件参数泛型</typeparam>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void XFEEventHandler<T1, T2>(T1 sender, T2 e);
    /// <summary>
    /// 委托事件拓展
    /// </summary>
    public static class DelegateExtension
    {
    }
}
