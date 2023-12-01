namespace XFE各类拓展.NetCore.XFEChatGPT
{
    /// <summary>
    /// ChatGPT模型的拓展
    /// </summary>
    public static class ChatGPTModelExtension
    {
        /// <summary>
        /// 获取模型的字符串
        /// </summary>
        /// <param name="chatGPTModel"></param>
        /// <returns></returns>
        /// <exception cref="XFEChatGPTException">意料之外的模型</exception>
        public static string GetModelString(this ChatGPTModel chatGPTModel)
        {
            switch (chatGPTModel)
            {
                case ChatGPTModel.gpt4:
                    return "gpt-4";
                case ChatGPTModel.gpt4turbo:
                    return "gpt-4-1106-preview";
                case ChatGPTModel.gpt4turbovision:
                    return "gpt-4-vision-preview";
                case ChatGPTModel.gpt40613:
                    return "gpt-4-0613";
                case ChatGPTModel.gpt432k:
                    return "gpt-4-32k";
                case ChatGPTModel.gpt432k0613:
                    return "gpt-4-32k-0613";
                case ChatGPTModel.gpt3point5turbo:
                    return "gpt-3.5-turbo";
                case ChatGPTModel.gpt3point5turbo16k:
                    return "gpt-3.5-turbo-16k";
                case ChatGPTModel.gpt3point5turbo0613:
                    return "gpt-3.5-turbo-0613";
                case ChatGPTModel.gpt3point5turbo16k0613:
                    return "gpt-3.5-turbo-16k-0613";
                case ChatGPTModel.textdavinci003:
                    return "text-davinci-003";
                case ChatGPTModel.textdavinci002:
                    return "text-davinci-002";
                case ChatGPTModel.codedavinci002:
                    return "code-davinci-002";
                default:
                    throw new XFEChatGPTException("未知的ChatGPT模型");
            }
        }
        /// <summary>
        /// 通过字符串获取模型
        /// </summary>
        /// <param name="chatGPTModel"></param>
        /// <returns></returns>
        /// <exception cref="XFEChatGPTException">意料之外的模型</exception>
        public static ChatGPTModel GetModel(this string chatGPTModel)
        {
            switch (chatGPTModel)
            {
                case "gpt-3.5-turbo":
                    return ChatGPTModel.gpt3point5turbo;
                case "gpt-3.5-turbo-16k":
                    return ChatGPTModel.gpt3point5turbo16k;
                case "gpt-3.5-turbo-0613":
                    return ChatGPTModel.gpt3point5turbo0613;
                case "gpt-3.5-turbo-16k-0613":
                    return ChatGPTModel.gpt3point5turbo16k0613;
                case "text-davinci-003":
                    return ChatGPTModel.textdavinci003;
                case "text-davinci-002":
                    return ChatGPTModel.textdavinci002;
                case "code-davinci-002":
                    return ChatGPTModel.codedavinci002;
                default:
                    throw new XFEChatGPTException("未知的ChatGPT模型");
            }
        }
    }
}