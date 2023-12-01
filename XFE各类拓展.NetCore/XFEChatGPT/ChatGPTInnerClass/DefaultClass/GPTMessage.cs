namespace XFE各类拓展.NetCore.XFEChatGPT.ChatGPTInnerClass.DefaultClass
{
    /// <summary>
    /// GPT消息
    /// </summary>
    /// <param name="role">角色</param>
    /// <param name="content">内容</param>
    public class GPTMessage(string role, string content)
    {
        private string role = role;
        /// <summary>
        /// 角色（目前已知有：System，User和Assistant）
        /// </summary>
        public string Role
        {
            get
            {
                return role;
            }
            set
            {
                role = SetRoleAndCheckRoleLegal(value);
            }
        }
        /// <summary>
        /// 与角色对应的消息内容
        /// </summary>
        public string Content { get; set; } = content;
        private static string SetRoleAndCheckRoleLegal(string inputRole)
        {
            string lowerRole = inputRole.ToLower();
            if (lowerRole == "system" || lowerRole == "user" || lowerRole == "assistant")
            {
                return lowerRole;
            }
            else
            {
                throw new XFEChatGPTException($"角色名称不合法：{inputRole}\n应为system，user或assistant三者中的一个");
            }
        }
    }
}