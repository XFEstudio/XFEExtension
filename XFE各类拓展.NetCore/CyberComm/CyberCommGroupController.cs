namespace XFE各类拓展.NetCore.CyberComm
{
    /// <summary>
    /// 通信群组控制器
    /// </summary>
    public class CyberCommGroupController
    {
        private readonly List<CyberCommGroup> commGroups;
        /// <summary>
        /// 刷新
        /// </summary>
        public void Refresh()
        {
            for (int i = commGroups.Count - 1; i >= 0; i--)
            {
                if (commGroups[i].Count == 0)
                {
                    commGroups.Remove(commGroups[i]);
                }
            }
        }
        /// <summary>
        /// 添加群组
        /// </summary>
        /// <param name="commGroup"></param>
        public void AddGroup(CyberCommGroup commGroup)
        {
            commGroups.Add(commGroup);
        }
        /// <summary>
        /// 移除群组
        /// </summary>
        /// <param name="commGroup"></param>
        public void RemoveGroup(CyberCommGroup commGroup)
        {
            commGroups.Remove(commGroup);
        }
        /// <summary>
        /// 移除指定索引的群组
        /// </summary>
        /// <param name="index"></param>
        public void RemoveGroupAt(int index)
        {
            commGroups.RemoveAt(index);
        }
        /// <summary>
        /// 清空群组
        /// </summary>
        public void Clear()
        {
            commGroups.Clear();
        }
        /// <summary>
        /// 群组数量
        /// </summary>
        public int Count
        {
            get
            {
                return commGroups.Count;
            }
        }
        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public CyberCommGroup this[int index]
        {
            get
            {
                return commGroups[index];
            }
        }
        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="GroupId"></param>
        /// <returns></returns>
        public CyberCommGroup this[string GroupId]
        {
            get
            {
                foreach (CyberCommGroup commGroup in commGroups)
                {
                    if (commGroup.GroupId == GroupId)
                    {
                        return commGroup;
                    }
                }
                return null;
            }
        }
        /// <summary>
        /// 通信群组控制器发送文本消息
        /// </summary>
        /// <param name="GroupId">目标群组的ID</param>
        /// <param name="message">发送的文本消息</param>
        public async Task SendGroupTextMessage(string GroupId, string message)
        {
            CyberCommGroup commGroup = this[GroupId];
            if (commGroup != null)
            {
                await commGroup.SendGroupTextMessage(message);
            }
        }
        /// <summary>
        /// 通信群组控制器发送二进制消息
        /// </summary>
        /// <param name="GroupId">目标群组的ID</param>
        /// <param name="bytes">发送的二进制消息</param>
        public async Task SendGroupBinaryMessage(string GroupId, byte[] bytes)
        {
            CyberCommGroup commGroup = this[GroupId];
            if (commGroup != null)
            {
                await commGroup.SendGroupBinaryMessage(bytes);
            }
        }
        /// <summary>
        /// 通信群组控制器
        /// </summary>
        public CyberCommGroupController()
        {
            commGroups = new List<CyberCommGroup>();
        }
    }
}