using System.Net.WebSockets;

namespace XFE各类拓展.NetCore.CyberComm
{
    /// <summary>
    /// 代签名的WebSocket
    /// </summary>
    public class SignedWebSocket
    {
        /// <summary>
        /// 签名
        /// </summary>
        public string Signature { get; set; }
        /// <summary>
        /// 服务器
        /// </summary>
        public WebSocket WebSocket { get; set; }
        /// <summary>
        /// 签名WebSocket
        /// </summary>
        /// <param name="signature">签名</param>
        /// <param name="webSocket">WebSocket</param>
        public SignedWebSocket(string signature, WebSocket webSocket)
        {
            Signature = signature;
            WebSocket = webSocket;
        }
    }
}