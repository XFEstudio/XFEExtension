using XFE各类拓展.NetCore.XFEChatGPT.ChatGPTInnerClass.DefaultClass;

namespace XFE各类拓展.NetCore.XFEChatGPT
{
    /// <summary>
    /// XFEChatGPT的扩展方法
    /// </summary>
    public static class XFEChatGPTExtension
    {
        /// <summary>
        /// 检查是否合法（system在最前，接着是user，然后是Assistant，以此类推...，不能连着2个user或者Assistant）
        /// </summary>
        /// <returns>是否合法</returns>
        public static bool CheckLegal(this GPTMessage[] xFEGPTMessages)
        {
            if (xFEGPTMessages.Length == 0)
            {
                return true;
            }
            else
            {
                if (xFEGPTMessages[0].Role != "system")
                {
                    return false;
                }
                else
                {
                    string lastRole = xFEGPTMessages[0].Role;
                    for (int i = 1; i < xFEGPTMessages.Length; i++)
                    {
                        if (xFEGPTMessages[i].Role == lastRole)
                        {
                            return false;
                        }
                        else
                        {
                            lastRole = xFEGPTMessages[i].Role;
                        }
                    }
                    return true;
                }
            }
        }
    }
}