namespace XFE各类拓展.NetCore.CyberComm.XCCNetWork;

/// <summary>
/// 消息接收触发器
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="isHistory">是否为历史消息</param>
/// <param name="message">消息</param>
public delegate void MessageReceivedHandler<T>(bool isHistory, T message);