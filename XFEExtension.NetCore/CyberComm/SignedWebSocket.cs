using System.Net.WebSockets;

namespace XFEExtension.NetCore.CyberComm;

/// <summary>
/// 代签名的WebSocket
/// </summary>
/// <remarks>
/// 签名WebSocket
/// </remarks>
/// <param name="signature">签名</param>
/// <param name="webSocket">WebSocket</param>
public class SignedWebSocket(string signature, WebSocket webSocket)
{
    /// <summary>
    /// 签名
    /// </summary>
    public string Signature { get; set; } = signature;
    /// <summary>
    /// 服务器
    /// </summary>
    public WebSocket WebSocket { get; set; } = webSocket;
}