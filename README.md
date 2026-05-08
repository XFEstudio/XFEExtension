# XFEExtension / .NetCore

[![NuGet Version](https://img.shields.io/nuget/v/xfeextension.netcore?label=NuGet&logo=NuGet)](https://www.nuget.org/packages/XFEExtension.NetCore/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/xfeextension.netcore?label=Downloads&logo=NuGet)](https://www.nuget.org/packages/XFEExtension.NetCore/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download)

> 🌐 English | [简体中文](https://github.com/XFEstudio/XFEExtension/blob/master/README.zh-CN.md)

## Description

XFEExtension is a C# DLL library designed to optimize common statements in C# code and provide more concise access patterns, rapid server/client setup, a free ChatGPT API interface, a free communication server, an XFE downloader, new formats, and more.

## Purpose

The XFEExtension library is suitable for a wide variety of C# projects, especially when improving code readability is a priority. It includes extension methods for many common operations, making code writing more efficient and straightforward. Here are some examples of what XFEExtension can do:

- **Simplify code access:** XFEExtension provides cleaner syntax, making access operations in your code clearer and more readable.

- **Optimize performance:** With XFEExtension you can perform various performance-optimization operations to improve application efficiency.

- **Accelerate development:** By reducing boilerplate code, XFEExtension speeds up project development while improving code maintainability.

# Examples (remember to add the appropriate `using` directives)

---

## Automatic JSON parsing without creating a JSON object

#### Basic usage

```csharp
using XFEExtension.NetCore.XFETransform.JsonConverter;

var jsonString = """
                 {
                     "status": 500,
                     "message": "Not found"
                 }
                 """;
QueryableJsonNode jsonNode = jsonString;
if (jsonNode["status"] == "500")
{
    Console.WriteLine(jsonNode["message"]);
}
```

#### Query and package

```csharp
var jsonString = """
                 {"code":0,"message":"0","data":{"archives":[{"id":0,"text":"This is a JSON tutorial document","trash":"trash text 1"},{"id":1,"text":"Here you will learn how to query JsonNode","trash":"trash text 2"},{"id":2,"text":"Hello World!","trash":"trash text 3"}]}}
                 """;
var jsonNode = (QueryableJsonNode)jsonString;
var packageList = jsonNode["data"]["archives"]["package:list", "id", "text"].PackageInListObject(); // package as list
foreach (var node in packageList)
{
    Console.WriteLine($"ID: {node["id"]}\tDocument: {node["text"]}"); // access properties directly
}

var packageObject = jsonNode["package:object", "code", "message"].PackageObject(); // package as object
foreach (var node in packageObject)
{
    Console.WriteLine($"PropertyName: {node.Key}\tValue: {node.Value}\tValueType: {node.Value.ValueType}"); // iterate properties
}
```

## Using LANDeviceDetector to detect all devices on the local network

#### Basic usage

```csharp
var lANDeviceDetector = new LANDeviceDetector();
lANDeviceDetector.DeviceFind += (sender) =>
{
    Console.WriteLine($"IP: {sender.IPAddress}\tDevice name: {sender.DeviceName}");
};
await lANDeviceDetector.StartDetecting();
```

#### Custom scan subnet

```csharp
var lANDeviceDetector = new LANDeviceDetector("100.73.121.*"); // scans 100.73.121.1 – 100.73.121.255
lANDeviceDetector.DeviceFind += (sender) =>
{
    Console.WriteLine($"IP: {sender.IPAddress}\tDevice name: {sender.DeviceName}");
};
await lANDeviceDetector.StartDetecting();
```

#### Custom timeout

```csharp
var lANDeviceDetector = new LANDeviceDetector("100.73.121.*", 2000); // sets timeout to 2000 ms
lANDeviceDetector.DeviceFind += (sender) =>
{
    Console.WriteLine($"IP: {sender.IPAddress}\tDevice name: {sender.DeviceName}");
};
await lANDeviceDetector.StartDetecting();
```

---

## Using the X method to analyze object information and simplify debugging

#### Output to console (console applications only)

```csharp
var testClass = new TestClass("Test Name", "Test Description", 15); // the object you want to analyze
testClass.X(); // prints all information about the object to the console
```

#### Output to trace/debug output (all C# application types)

```csharp
var testClass = new TestClass("Test Name", "Test Description", 15); // the object you want to analyze
testClass.XL(); // writes all information about the object to the debug trace output
```

---

## XFE ChatGPT usage examples

#### Simplest usage

```csharp
// Ask GPT and receive a reply
var result = await XFEChatGPT.SendAndGetGPTResponse("Hello");
Console.WriteLine(result);
```

#### General usage

```csharp
// Use XFEChatGPT for interactive GPT conversations
XFEChatGPT xFEChatGPT = new XFEChatGPT("You are an AI assistant", true);

// Subscribe to events
xFEChatGPT.XFEChatGPTMessageReceived += (sender, e) =>
{
    switch (e.GenerateState)
    {
        case GenerateState.Start:
            Console.Write("[Generation started] ChatGPT:");
            break;
        case GenerateState.Continue:
            Console.Write(e.Message);
            break;
        case GenerateState.End:
            Console.WriteLine("[Generation complete]");
            break;
        case GenerateState.Error:
            Console.WriteLine($"[Error] {e.Message}");
            break;
        default:
            break;
    }
};

// Read the user's question
var askContent = Console.ReadLine();

// Send with a random message ID
xFEChatGPT.SendGPTMessage(Guid.NewGuid().ToString(), askContent);
```

#### Recommended usage

```csharp
// Create a MemorableXFEChatGPT object with conversation memory
MemorableXFEChatGPT memorableXFEChatGPT = new MemorableXFEChatGPT();

// Create a new dialog and set the system message
memorableXFEChatGPT.CreateDialog("dialog-id", "You are an AI assistant developed by XFEstudio", true, true);

// Subscribe to the message-received event
memorableXFEChatGPT.XFEChatGPTMessageReceived += (sender, e) =>
{
    switch (e.GenerateState)
    {
        case GenerateState.Start:
            Console.Write("[Generation started] ChatGPT:");
            break;
        case GenerateState.Continue:
            Console.Write(e.Message);
            break;
        case GenerateState.End:
            Console.WriteLine("[Generation complete]");
            break;
        case GenerateState.Error:
            Console.WriteLine($"[Error] {e.Message}");
            break;
        default:
            break;
    }
};

// Read the user's question
var askContent = Console.ReadLine();

// Send using the dialog ID created above, a random message ID, and the question
memorableXFEChatGPT.AskChatGPT("dialog-id", Guid.NewGuid().ToString(), askContent);
```

---

## IO stream extension examples

```csharp
// Simplify file read/write operations with XFEExtension
"Hello World!".WriteIn("test.txt");
string txt = "test.txt".ReadOut();
```

---

## XEA encryption algorithm example

```csharp
// Encrypt and decrypt text using XFEExtension
string text = "This is text that will be encrypted";
$"Plain text: {text}".CW();
string password = "my-secret-key";
string encrypt = text.XEAEncrypt(password); // encrypt
Console.WriteLine("Encrypted: " + encrypt);
Console.WriteLine("Decrypted: " + encrypt.XEADecrypt(password)); // decrypt
```

---

## Attribute access example

```csharp
// Simplify attribute reading with XFEExtension
string str = testObject.GetAttribute<string>();
```

---

## Using the XUnit testing framework

```csharp
[CTest]
class TestClass : XFECode
{
    [MTest]
    void Test()
    {
        Assert(true, "assertion content");
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

## Quickly set up a network communication server

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
        e.ReplyMessage("Server received your message");
        Console.WriteLine($"Message from client [{e.IPAddress}]: {e.TextMessage}"); // plain-text example
    }

    private void CyberCommServer_ClientConnected(object? sender, CyberCommServerEventArgs e)
    {
        Console.WriteLine($"New client connected: {e.IPAddress}");
    }

    private void CyberCommServer_ConnectionClosed(object? sender, CyberCommServerEventArgs e)
    {
        Console.WriteLine($"Client [{e.IPAddress}] disconnected");
    }

    private void CyberCommServer_ServerStarted(object? sender, EventArgs e)
    {
        Console.WriteLine("Server started");
    }
}
```

---

## Quickly set up a network communication client

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
        Console.WriteLine($"Message received: {e.TextMessage}"); // receive plain-text message
        // reply here if needed:
        // e.ReplyMessage();
    }

    private void CyberCommClient_ConnectionClosed(object? sender, EventArgs e)
    {
        Console.WriteLine("Disconnected from server");
    }

    private void CyberCommClient_Connected(object? sender, EventArgs e)
    {
        Console.WriteLine("Connected to server");
        CyberCommClient.SendTextMessage("This is a test message"); // plain-text example
    }
}
```

---

## Build a chat room quickly using the XCC network communication API

```csharp
XCCNetWork xCCNetWork = new(); // create XCC network base
var group = xCCNetWork.CreateGroup("Test Group", "Member Name"); // create a group, provide group name and member name
#region Subscribe to events
xCCNetWork.Connected += (sender, e) =>
{
    Console.WriteLine($"Group: {e.Group.GroupId}\tConnected");
    group.SendTextMessage("Test message");
};
xCCNetWork.ConnectionClosed += (sender, e) =>
{
    Console.WriteLine($"Group: {e.Group.GroupId}\tDisconnected");
};
xCCNetWork.TextMessageReceived += (sender, e) =>
{
    Console.WriteLine($"Group: {e.Group.GroupId}\tText message received: {e.TextMessage}");
};
#endregion
await group.StartXCC(); // start the group's network communication
```

---

## Use XFEDownloader to accelerate file downloads (supports resuming and multi-threaded acceleration)

```csharp
XFEDownloader xFEDownloader = new()
{
    DownloadUrl = "https://www.nuget.org/api/v2/package/XFEExtension.NetCore/4.1.0",
    SavePath = "XFEExtension.NetCore.nupkg",
    FileSegmentCount = 9 // use 9 threads to accelerate the download (recommended: no more than 15)
};
xFEDownloader.BufferDownloaded += (sender, e) =>
{
    Console.WriteLine($"Progress: {e.DownloadedBufferSize.FileSize()}/{e.TotalBufferSize?.FileSize()}");
};
await xFEDownloader.Download();
```