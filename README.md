# XFEExtension (XFE各类拓展)

## 描述

XFEExtension是一个C#的DLL库，旨在优化C#代码中常用语句的使用，并提供更简洁的访问方式，以便在快速开发C#项目时提高代码的可读性和简洁性。且提供了免费的ChatGPT代码API访问

## 用途

XFEExtension库适用于各种C#项目，特别适合在需要提高代码可读性的情况下使用。它包含了许多常见操作的拓展方法，使得代码编写更加高效和简便。以下是一些XFEExtension的用途示例：

- **简化代码访问：** XFEExtension提供了更简洁的语法，使得代码中的访问操作更加清晰和易读。

- **优化性能：** 通过使用XFEExtension，您可以执行各种性能优化操作，提高应用程序的效率。

- **加速开发：** 通过减少样板代码，XFEExtension可以加速项目的开发过程，同时提高代码的可维护性。

# 示例（使用前记得进行相应的引用）

## 以下是一些使用XFE各类拓展的示例：

### XFE的ChatGPT使用示例

#### 最简单的用法

```csharp
//询问GPT并接收回复
var result = await XFEChatGPT.SendAndGetGPTResponse("你好");
Console.WriteLine(result);
```

#### 一般用法

```csharp
//使用XFEChatGPT类来进行GPT的交互
XFEChatGPT xFEChatGPT = new XFEChatGPT("你是一个人工智能AI", true);

//订阅事件
xFEChatGPT.XFEChatGPTMessageReceived += (sender, e) =>
{
    switch (e.GenerateState)
    {
        case GenerateState.Start:
            Console.Write("【输出开始】ChatGPT:");
            break;
        case GenerateState.Continue:
            Console.Write(e.Message);
            break;
        case GenerateState.End:
            Console.WriteLine("【输出完成】");
            break;
        case GenerateState.Error:
            Console.WriteLine($"【发生错误】{e.Message}");
            break;
        default:
            break;
    }
};

//输入询问内容
var askContent = Console.ReadLine();

//发送生成随机ID并询问内容
xFEChatGPT.SendGPTMessage(Guid.NewGuid().ToString(), askContent);
```

#### 推荐用法

```csharp
//创建有记忆功能的XFEChatGPT对象
MemorableXFEChatGPT memorableXFEChatGPT = new MemorableXFEChatGPT();

//创建一个新的对话并设置System内容
memorableXFEChatGPT.CreateDialog("新的对话ID", "你是一个由寰宇朽力网络科技开发的人工智能AI", true, true);

//订阅消息接收事件
memorableXFEChatGPT.XFEChatGPTMessageReceived += (sender, e) =>
{
    switch (e.GenerateState)
    {
        case GenerateState.Start:
            Console.Write("【输出开始】ChatGPT:");
            break;
        case GenerateState.Continue:
            Console.Write(e.Message);
            break;
        case GenerateState.End:
            Console.WriteLine("【输出完成】");
            break;
        case GenerateState.Error:
            Console.WriteLine($"【发生错误】{e.Message}");
            break;
        default:
            break;
    }
};

//读取询问内容
var askContent = Console.ReadLine();

//填写之前创建的对话ID，生成随机的消息ID，并输入刚刚读取的询问内容
memorableXFEChatGPT.AskChatGPT("新的对话ID", Guid.NewGuid().ToString(), askContent);
```

### IO流拓展操作示例

```csharp
// 使用XFEExtension来简化文件读取/写入操作
"Hello World!".WriteIn("test.txt");
string txt = "test.txt".ReadOut();
```

### XEA加密算法示例

```csharp
// 使用XFE各类拓展来进行加密操作
string text = "这是一段将要加密的文本";
$"未加密内容：{text}".CW();
string password = "这是一个秘钥";
string encrypt = text.XEAEncrypt(password);//加密
Console.WriteLine("加密内容：" + encrypt);
Console.WriteLine("解密内容：" + encrypt.XEADecrypt(password));//解密
```

### 特性操作示例

```csharp
// 使用XFE各类拓展来简化特性读取操作
string str = testObject.GetAttribute<string>();
```
