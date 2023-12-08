# XFEExtension (XFE各类拓展)

## 描述

XFE各类拓展是一个C#的DLL库，旨在优化C#代码中常用语句的使用，并提供更简洁的访问方式，同时提供Xunit测试框架，快速搭建服务器/客户端，免费ChatGPTAPI接口，免费通讯服务器，XFE下载器，新增格式等

## 用途

XFE各类拓展库适用于各种C#项目，特别适合在需要提高代码可读性的情况下使用。它包含了许多常见操作的拓展方法，使得代码编写更加高效和简便。以下是一些XFEExtension的用途示例：

- **简化代码访问：** XFE各类拓展提供了更简洁的语法，使得代码中的访问操作更加清晰和易读。

- **优化性能：** 通过使用XFE各类拓展，您可以执行各种性能优化操作，提高应用程序的效率。

- **加速开发：** 通过减少样板代码，XFE各类拓展可以加速项目的开发过程，同时提高代码的可维护性。

# 示例（使用前记得进行相应的引用）

---

## XFE的ChatGPT使用示例

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

---

## IO流拓展操作示例

```csharp
// 使用XFEExtension来简化文件读取/写入操作
"Hello World!".WriteIn("test.txt");
string txt = "test.txt".ReadOut();
```

---

## XEA加密算法示例

```csharp
// 使用XFE各类拓展来进行加密操作
string text = "这是一段将要加密的文本";
$"未加密内容：{text}".CW();
string password = "这是一个秘钥";
string encrypt = text.XEAEncrypt(password);//加密
Console.WriteLine("加密内容：" + encrypt);
Console.WriteLine("解密内容：" + encrypt.XEADecrypt(password));//解密
```

---

## 特性操作示例

```csharp
// 使用XFE各类拓展来简化特性读取操作
string str = testObject.GetAttribute<string>();
```

---

## 使用XUnit测试框架

```csharp
[CTest]
class TestClass : XFECode
{
    [MTest]
    void Test()
    {
        Assert(true, "断言内容");
    }
}
public class Program : XFECode
{
    public static void Main(string[] args)
    {
        Pause();
    }
}
```

---

## 快速搭建网络通讯服务器

```csharp
public class CustomServer
{
    CyberCommServer CyberCommServer { get; } = new("http://127.0.0.1:19019/");
    public async Task StartServer()
    {
        CyberCommServer.ServerStarted += CyberCommServer_ServerStarted;
        CyberCommServer.ConnectionClosed += CyberCommServer_ConnectionClosed;
        CyberCommServer.ClientConnected += CyberCommServer_ClientConnected;
        CyberCommServer.MessageReceived += CyberCommServer_MessageReceived;
        await CyberCommServer.StartCyberCommServer();
    }

    private void CyberCommServer_MessageReceived(object? sender, CyberCommServerEventArgs e)
    {
        e.ReplyMessage("服务器已接收消息");
        Console.WriteLine($"收到客户端[{e.IpAddress}]消息：{e.TextMessage}");//明文传输实例
    }

    private void CyberCommServer_ClientConnected(object? sender, CyberCommServerEventArgs e)
    {
        Console.WriteLine($"新客户端连接：{e.IpAddress}");
    }

    private void CyberCommServer_ConnectionClosed(object? sender, CyberCommServerEventArgs e)
    {
        Console.WriteLine($"客户端[{e.IpAddress}]断开连接");
    }

    private void CyberCommServer_ServerStarted(object? sender, EventArgs e)
    {
        Console.WriteLine("服务器已启动");
    }
}
```

---

## 快速搭建网络通讯客户端

```csharp
public class CustomClient
{
    CyberCommClient CyberCommClient { get; } = new("http://127.0.0.1:19019/");
    public async Task StartClient()
    {
        CyberCommClient.Connected += CyberCommClient_Connected;
        CyberCommClient.ConnectionClosed += CyberCommClient_ConnectionClosed;
        CyberCommClient.MessageReceived += CyberCommClient_MessageReceived;
        await CyberCommClient.StartCyberCommClient();
    }

    private void CyberCommClient_MessageReceived(object? sender, CyberCommClientEventArgs e)
    {
        Console.WriteLine($"收到消息：{e.TextMessage}");//接收明文消息
        //此处可以进行消息回复
        //e.ReplyMessage();
    }

    private void CyberCommClient_ConnectionClosed(object? sender, EventArgs e)
    {
        Console.WriteLine("与服务器断开连接");
    }

    private void CyberCommClient_Connected(object? sender, EventArgs e)
    {
        Console.WriteLine("已连接到服务器");
        CyberCommClient.SendTextMessage("这是一条测试消息");//以明文消息为示例
    }
}
```

---

## 使用XCC网络通讯API接口快速搭建聊天室

```csharp
XCCNetWork xCCNetWork = new();//创建XCC网络通讯基础
var group = xCCNetWork.CreateGroup("测试群组", "测试人员");//创建网络通讯中的群组，输入群组名，群内名称
#region 订阅事件
xCCNetWork.Connected += (sender, e) =>
{
    Console.WriteLine($"群组：{e.Group.GroupId}\t连接成功");
    group.SendTextMessage("测试消息");
};
xCCNetWork.ConnectionClosed += (sender, e) =>
{
    Console.WriteLine($"群组：{e.Group.GroupId}\t断开连接");
};
xCCNetWork.TextMessageReceived += (sender, e) =>
{
    Console.WriteLine($"群组：{e.Group.GroupId}\t收到文本消息：{e.TextMessage}");
};
#endregion
await group.StartXCC();//启动该群组的网络通讯
```

---

## 使用XFE下载器下载文件（支持继续上次下载等操作）

```csharp
XFEDownloader xFEDownloader = new()
{
    DownloadUrl = "https://www.nuget.org/api/v2/package/XFE%E5%90%84%E7%B1%BB%E6%8B%93%E5%B1%95.NetCore/1.2.2",
    SavePath = "XFE各类拓展.NetCore.nuget"
};
xFEDownloader.BufferDownloaded += (sender, e) =>
{
    Console.WriteLine($"进度：{e.DownloadedBufferSize.FileSize()}/{e.TotalBufferSize?.FileSize()}");
};
await xFEDownloader.Download();
```