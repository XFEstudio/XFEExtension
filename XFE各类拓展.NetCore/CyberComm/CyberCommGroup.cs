using System.Net.WebSockets;

namespace XFE各类拓展.NetCore.CyberComm
{
    /// <summary>
    /// CyberComm客户端群组
    /// </summary>
    public class CyberCommGroup
    {
        private readonly List<CyberCommServerEventArgs> cyberCommList = new List<CyberCommServerEventArgs>();
        /// <summary>
        /// 组ID
        /// </summary>
        public string GroupId { get; set; }
        /// <summary>
        /// 添加客户端
        /// </summary>
        /// <param name="e">客户端</param>
        public void Add(CyberCommServerEventArgs e)
        {
            cyberCommList.Add(e);
        }
        /// <summary>
        /// 移除指定的客户端
        /// </summary>
        /// <param name="webSocket">客户端</param>
        public void Remove(WebSocket webSocket)
        {
            var cyberCommServerEventArgs = cyberCommList.Find(x => x.CurrentWebSocket == webSocket);
            if (cyberCommServerEventArgs is not null)
                cyberCommList.Remove(cyberCommServerEventArgs);
        }
        /// <summary>
        /// 移除指定索引的客户端
        /// </summary>
        /// <param name="index">客户单索引</param>
        public void RemoveAt(int index)
        {
            cyberCommList.RemoveAt(index);
        }
        /// <summary>
        /// 清空列表
        /// </summary>
        public void Clear()
        {
            cyberCommList.Clear();
        }
        /// <summary>
        /// 群组客户端数量
        /// </summary>
        public int Count
        {
            get
            {
                return cyberCommList.Count;
            }
        }
        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="findFunc">查找</param>
        /// <returns>客户端</returns>
        public CyberCommServerEventArgs? this[Predicate<CyberCommServerEventArgs> findFunc]
        {
            get
            {
                return cyberCommList.Find(findFunc);
            }
        }
        /// <summary>
        /// 发送群组文本消息
        /// </summary>
        /// <param name="message">群发文本消息</param>
        public async Task SendGroupTextMessage(string message)
        {
            List<Task> tasks = [];
            foreach (CyberCommServerEventArgs cyberCommServerEventArgs in cyberCommList)
            {
                tasks.Add(cyberCommServerEventArgs.ReplyMessage(message));
            }
            await Task.WhenAll(tasks);
        }
        /// <summary>
        /// 向指定的WS客户端发送群组文本消息
        /// </summary>
        /// <param name="message">群发文本消息</param>
        /// <param name="findFunc">指定的WS客户端</param>
        public async Task SendGroupTextMessage(string message, Func<CyberCommServerEventArgs, bool> findFunc)
        {
            List<Task> tasks = new List<Task>();
            foreach (CyberCommServerEventArgs cyberCommServerEventArgs in cyberCommList)
            {
                if (findFunc.Invoke(cyberCommServerEventArgs))
                {
                    tasks.Add(cyberCommServerEventArgs.ReplyMessage(message));
                }
            }
            await Task.WhenAll(tasks);
        }
        /// <summary>
        /// 发送群组二进制消息
        /// </summary>
        /// <param name="bytes">群发二进制消息</param>
        public async Task SendGroupBinaryMessage(byte[] bytes)
        {
            List<Task> tasks = new List<Task>();
            foreach (CyberCommServerEventArgs cyberCommServerEventArgs in cyberCommList)
            {
                tasks.Add(cyberCommServerEventArgs.ReplyBinaryMessage(bytes));
            }
            await Task.WhenAll(tasks);
        }
        /// <summary>
        /// 向指定的WS客户端发送群组二进制消息
        /// </summary>
        /// <param name="bytes">群发二进制消息</param>
        /// <param name="findFunc">指定的WS客户端</param>
        public async Task SendGroupBinaryMessage(byte[] bytes, Func<CyberCommServerEventArgs, bool> findFunc)
        {
            List<Task> tasks = new List<Task>();
            foreach (CyberCommServerEventArgs cyberCommServerEventArgs in cyberCommList)
            {
                if (findFunc.Invoke(cyberCommServerEventArgs))
                {
                    tasks.Add(cyberCommServerEventArgs.ReplyBinaryMessage(bytes));
                }
            }
            await Task.WhenAll(tasks);
        }
        /// <summary>
        /// 客户端群组
        /// </summary>
        /// <param name="GroupId">群组ID</param>
        public CyberCommGroup(string GroupId)
        {
            this.GroupId = GroupId;
        }
        /// <summary>
        /// 客户端群组
        /// </summary>
        /// <param name="GroupId">群组ID</param>
        /// <param name="cyberCommList">客户端群组</param>
        public CyberCommGroup(string GroupId, List<CyberCommServerEventArgs> cyberCommList)
        {
            this.GroupId = GroupId;
            this.cyberCommList = cyberCommList;
        }
    }
}