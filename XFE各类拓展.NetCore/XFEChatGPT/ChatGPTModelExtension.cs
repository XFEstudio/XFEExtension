namespace XFE各类拓展.NetCore.XFEChatGPT;

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
        return chatGPTModel switch
        {
            ChatGPTModel.gpt4 => "gpt-4",
            ChatGPTModel.gpt4turbo => "gpt-4-1106-preview",
            ChatGPTModel.gpt4turbovision => "gpt-4-vision-preview",
            ChatGPTModel.gpt40613 => "gpt-4-0613",
            ChatGPTModel.gpt432k => "gpt-4-32k",
            ChatGPTModel.gpt432k0613 => "gpt-4-32k-0613",
            ChatGPTModel.gpt3point5turbo => "gpt-3.5-turbo",
            ChatGPTModel.gpt3point5turbo16k => "gpt-3.5-turbo-16k",
            ChatGPTModel.gpt3point5turbo0613 => "gpt-3.5-turbo-0613",
            ChatGPTModel.gpt3point5turbo16k0613 => "gpt-3.5-turbo-16k-0613",
            ChatGPTModel.textdavinci003 => "text-davinci-003",
            ChatGPTModel.textdavinci002 => "text-davinci-002",
            ChatGPTModel.codedavinci002 => "code-davinci-002",
            _ => throw new XFEChatGPTException("未知的ChatGPT模型"),
        };
    }
    /// <summary>
    /// 通过字符串获取模型
    /// </summary>
    /// <param name="chatGPTModel"></param>
    /// <returns></returns>
    /// <exception cref="XFEChatGPTException">意料之外的模型</exception>
    public static ChatGPTModel GetModel(this string chatGPTModel)
    {
        return chatGPTModel switch
        {
            "gpt-3.5-turbo" => ChatGPTModel.gpt3point5turbo,
            "gpt-3.5-turbo-16k" => ChatGPTModel.gpt3point5turbo16k,
            "gpt-3.5-turbo-0613" => ChatGPTModel.gpt3point5turbo0613,
            "gpt-3.5-turbo-16k-0613" => ChatGPTModel.gpt3point5turbo16k0613,
            "text-davinci-003" => ChatGPTModel.textdavinci003,
            "text-davinci-002" => ChatGPTModel.textdavinci002,
            "code-davinci-002" => ChatGPTModel.codedavinci002,
            _ => throw new XFEChatGPTException("未知的ChatGPT模型"),
        };
    }
}