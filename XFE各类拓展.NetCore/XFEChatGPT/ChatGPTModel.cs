namespace XFE各类拓展.NetCore.XFEChatGPT;

/// <summary>
/// ChatGPT的模型
/// </summary>
public enum ChatGPTModel
{
    /// <summary>
    /// 比任何 GPT-3.5 型号都更强大，能够执行更复杂的任务，并针对聊天进行了优化。将在发布 2 周后使用我们最新的模型迭代进行更新。
    /// </summary>
    gpt4,
    /// <summary>
    /// 最新的GPT-4模型，具有改进的指令跟随、JSON模式、可复制输出、并行函数调用等功能。最多返回4096个输出标记。此预览模型还不适合生产流量。
    /// </summary>
    gpt4turbo,
    /// <summary>
    /// 能够理解图像，以及所有其他GPT-4 Turbo功能。最多返回4096个输出标记。这是一个预览模型版本，还不适合生产流量。
    /// </summary>
    gpt4turbovision,
    /// <summary>
    /// 13 年 2023 月 3 日带有函数调用数据的快照。与gpt-4不同，此模型不会收到更新，并将在新版本发布3个月后弃用。基于gpt-4
    /// </summary>
    gpt40613,
    /// <summary>
    /// 功能与gpt4基本模型相同，但上下文长度是其 4 倍。将使用最新的模型迭代进行更新。基于gpt-4
    /// </summary>
    gpt432k,
    /// <summary>
    /// 13 年 2023 月 3 日的快照。与GPT-4-32K不同，此模型不会收到更新，并将在新版本发布 3 个月后弃用。基于gpt-4-32k
    /// </summary>
    gpt432k0613,
    /// <summary>
    /// 功能最强大的 GPT-3.5 型号，针对聊天进行了优化，成本仅为 .将在发布 1 周后使用最新的模型迭代进行更新。
    /// </summary>
    gpt3point5turbo,
    /// <summary>
    /// 功能与标准模型相同，但上下文是其 4 倍。
    /// </summary>
    gpt3point5turbo16k,
    /// <summary>
    /// 13 年 2023 月 3 日的快照。与GPT-3.5-16K不同，此模型不会收到更新，并将在新版本发布 3 个月后弃用。
    /// </summary>
    gpt3point5turbo0613,
    /// <summary>
    /// 13 年 2023 月 3 日的快照。与GPT-3.5-turbo-16k不同，此模型不会收到更新，并将在新版本发布 3 个月后弃用。
    /// </summary>
    gpt3point5turbo16k0613,
    /// <summary>
    /// 可以完成任何语言任务，质量更好，输出时间更长，并且遵循一致的指令，而不是居里，巴贝奇或ADA模型。还支持一些附加功能，例如插入文本。
    /// </summary>
    textdavinci003,
    /// <summary>
    /// 与监督微调而不是强化学习类似的能力，但经过训练。
    /// </summary>
    textdavinci002,
    /// <summary>
    /// 针对代码完成任务进行优化的模型
    /// </summary>
    codedavinci002
}