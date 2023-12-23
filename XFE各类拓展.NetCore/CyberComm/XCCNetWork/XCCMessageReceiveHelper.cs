using XFE各类拓展.NetCore.DelegateExtension;
using XFE各类拓展.NetCore.FormatExtension;

namespace XFE各类拓展.NetCore.CyberComm.XCCNetWork;

/// <summary>
/// XCC消息接收器
/// </summary>
public class XCCMessageReceiveHelper
{
    private readonly Dictionary<string, XCCFile> xCCFileDictionary = [];
    private readonly Dictionary<string, List<XCCMessage>> xCCMessageDictionary = [];
    private bool loaded = false;
    /// <summary>
    /// 自动保存到本地
    /// </summary>
    public bool AutoSaveInLocal { get; set; }
    /// <summary>
    /// 保存的根目录
    /// </summary>
    public string SavePathRoot { get; set; }
    /// <summary>
    /// 接收到文件事件
    /// </summary>
    public event MessageReceivedHandler<XCCFile>? FileReceived;
    /// <summary>
    /// 接收到文本事件
    /// </summary>
    public event MessageReceivedHandler<XCCMessage>? TextReceived;
    /// <summary>
    /// 错误发生事件
    /// </summary>
    public event XFEEventHandler<XFECyberCommException>? ExceptionOccurred;
    /// <summary>
    /// 接收到实时音频字节流事件
    /// </summary>
    public event XFEEventHandler<byte[]>? AudioBufferReceived;
    /// <summary>
    /// 从设置的根目录加载
    /// </summary>
    /// <returns></returns>
    public async Task Load()
    {
        await Task.Run(() =>
        {
            if (Directory.Exists(SavePathRoot))
            {
                foreach (var groupIdFullPath in Directory.EnumerateDirectories(SavePathRoot))
                {
                    var groupId = Path.GetFileName(groupIdFullPath);
                    if (File.Exists($"{groupIdFullPath}/XFEMessage/XFEMessage.xfe"))
                    {
                        var xCCMessageList = new List<XCCMessage>();
                        foreach (var entry in new XFEMultiDictionary(File.ReadAllText($"{groupIdFullPath}/XFEMessage/XFEMessage.xfe")))
                        {
                            var xCCMessage = XCCMessage.ConvertToXCCMessage(entry.Content, groupId);
                            xCCMessageList.Add(xCCMessage);
                            if (xCCMessage.MessageType == XCCTextMessageType.Text)
                                TextReceived?.Invoke(true, xCCMessage);
                            else
                                FileReceived?.Invoke(true, LoadFile(xCCMessage)!);
                        }
                        xCCMessageDictionary.Add(groupId, xCCMessageList);
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(SavePathRoot);
            }
        });
        loaded = true;
    }
    /// <summary>
    /// 从设置的根目录的指定群组加载
    /// </summary>
    /// <param name="groupId">群组ID</param>
    /// <returns></returns>
    public async Task LoadGroup(string groupId)
    {
        await Task.Run(() =>
        {
            if (File.Exists($"{SavePathRoot}/{groupId}/XFEMessage/XFEMessage.xfe"))
            {
                var xCCMessageList = new List<XCCMessage>();
                foreach (var entry in new XFEMultiDictionary(File.ReadAllText($"{SavePathRoot}/{groupId}/XFEMessage/XFEMessage.xfe")))
                {
                    var xCCMessage = XCCMessage.ConvertToXCCMessage(entry.Content, groupId);
                    xCCMessageList.Add(xCCMessage);
                    if (xCCMessage.MessageType == XCCTextMessageType.Text)
                        TextReceived?.Invoke(true, xCCMessage);
                    else
                        FileReceived?.Invoke(true, LoadFile(xCCMessage)!);
                }
                xCCMessageDictionary.Add(groupId, xCCMessageList);
            }
        });
        loaded = true;
    }
    /// <summary>
    /// 清理无用文件
    /// </summary>
    /// <returns></returns>
    public async Task ClearUselessFile()
    {
        if (!loaded)
            throw new XFEExtensionException("不能在加载完成前调用清理");
        await Task.Run(() =>
        {
            foreach (var groupId in xCCMessageDictionary.Keys)
            {
                foreach (var file in Directory.EnumerateFiles($"{SavePathRoot}/{groupId}"))
                {
                    var filePath = $"{SavePathRoot}/{groupId}/{file}";
                    var messageId = Path.GetFileNameWithoutExtension(filePath);
                    if (!xCCMessageDictionary.TryGetValue(groupId, out List<XCCMessage>? value) || value.Find(x => x.MessageId == messageId) is null)
                    {
                        File.Delete(filePath);
                    }
                }
            }
        });
    }
    private XCCFile? LoadFile(XCCMessage xCCMessage)
    {
        var filePath = $"{SavePathRoot}/{xCCMessage.GroupId}/{xCCMessage.MessageId}.xfe";
        byte[]? fileBuffer = null;
        if (File.Exists(filePath))
            fileBuffer = File.ReadAllBytes(filePath);
        XCCFile xCCFile;
        if (xCCFileDictionary.TryGetValue(xCCMessage.MessageId, out XCCFile? value))
        {
            if (!value.Loaded && fileBuffer is not null)
                value.LoadFile(fileBuffer);
            return value;
        }
        switch (xCCMessage.MessageType)
        {
            case XCCTextMessageType.Image:
                xCCFile = new XCCFile(xCCMessage.GroupId, xCCMessage.MessageId, XCCFileType.Image, xCCMessage.Sender, xCCMessage.SendTime, fileBuffer);
                break;
            case XCCTextMessageType.Audio:
                xCCFile = new XCCFile(xCCMessage.GroupId, xCCMessage.MessageId, XCCFileType.Audio, xCCMessage.Sender, xCCMessage.SendTime, fileBuffer);
                break;
            case XCCTextMessageType.Video:
                xCCFile = new XCCFile(xCCMessage.GroupId, xCCMessage.MessageId, XCCFileType.Video, xCCMessage.Sender, xCCMessage.SendTime, fileBuffer);
                break;
            default:
                return null;
        }
        xCCFileDictionary.Add(xCCMessage.MessageId, xCCFile);
        return xCCFile;
    }
    /// <summary>
    /// 获取文件
    /// </summary>
    /// <param name="messageId">消息ID</param>
    /// <returns></returns>
    public XCCFile? GetFile(string messageId)
    {
        return xCCFileDictionary.TryGetValue(messageId, out XCCFile? value) ? value : null;
    }
    /// <summary>
    /// 添加文件
    /// </summary>
    /// <param name="xCCFile">XCC文件实例</param>
    public void AddFile(XCCFile xCCFile)
    {
        xCCFileDictionary.Add(xCCFile.MessageId, xCCFile);
        if (AutoSaveInLocal && xCCFile.FileBuffer is not null)
            SaveFile(xCCFile);
    }
    /// <summary>
    /// 保存文件
    /// </summary>
    /// <param name="xCCFile">XCC文件实例</param>
    public void SaveFile(XCCFile xCCFile)
    {
        if (!Directory.Exists($"{SavePathRoot}/{xCCFile.GroupId}"))
        {
            Directory.CreateDirectory($"{SavePathRoot}/{xCCFile.GroupId}");
        }
        File.WriteAllBytes($"{SavePathRoot}/{xCCFile.GroupId}/{xCCFile.MessageId}.xfe", xCCFile.FileBuffer!);
    }
    /// <summary>
    /// 保存群组消息
    /// </summary>
    /// <param name="groupId"></param>
    public void SaveMessage(string groupId)
    {
        var filePath = $"{SavePathRoot}/{groupId}/XFEMessage";
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }
        var storageDictionary = new XFEDictionary();
        foreach (var xCCMessage in xCCMessageDictionary[groupId])
        {
            storageDictionary.Add(xCCMessage.MessageId, xCCMessage.ToString());
        }
        File.WriteAllText(filePath + "/XFEMessage.xfe", storageDictionary.ToString());
    }
    private void ReceiveFilePlaceHolder(XCCTextMessageReceivedEventArgs e, XCCFileType fileType)
    {
        var xCCFile = new XCCFile(e.GroupId, e.MessageId!, fileType, e.Sender!, e.SendTime);
        xCCFileDictionary.TryAdd(e.MessageId!, xCCFile);
        FileReceived?.Invoke(e.IsHistory, xCCFile);
        if (AutoSaveInLocal)
            SaveMessage(e.GroupId);
    }
    private void ReceiveTextMessage(object? sender, XCCTextMessageReceivedEventArgs e)
    {
        var message = new XCCMessage(e.MessageId!, e.MessageType, e.TextMessage, e.Sender!, e.SendTime, e.GroupId);
        if (xCCMessageDictionary.TryGetValue(e.GroupId, out List<XCCMessage>? value))
        {
            if (value.Find(x => x.MessageId == e.MessageId) is null)
            {
                value.Add(message);
            }
            else
            {
                return;
            }
        }
        else
        {
            xCCMessageDictionary.Add(e.GroupId, [message]);
        }
        switch (e.MessageType)
        {
            case XCCTextMessageType.Text:
                TextReceived?.Invoke(e.IsHistory, message);
                if (AutoSaveInLocal)
                    SaveMessage(e.GroupId);
                break;
            case XCCTextMessageType.Image:
                ReceiveFilePlaceHolder(e, XCCFileType.Image);
                break;
            case XCCTextMessageType.Audio:
                ReceiveFilePlaceHolder(e, XCCFileType.Audio);
                break;
            case XCCTextMessageType.Video:
                ReceiveFilePlaceHolder(e, XCCFileType.Video);
                break;
            default:
                break;
        }
    }
    private void ReceiveBinaryMessage(object? sender, XCCBinaryMessageReceivedEventArgs e)
    {
        if (e.MessageType == XCCBinaryMessageType.AudioBuffer)
        {
            AudioBufferReceived?.Invoke(e.BinaryMessage);
            return;
        }
        if (xCCFileDictionary.TryGetValue(e.MessageId!, out XCCFile? value))
        {
            var xCCFile = value;
            if (!xCCFile.Loaded)
            {
                value.LoadFile(e.BinaryMessage);
                if (AutoSaveInLocal)
                    SaveFile(xCCFile);
            }
        }
        else
        {
            var fileType = XCCFileType.Image;
            switch (e.MessageType)
            {
                case XCCBinaryMessageType.Image:
                    fileType = XCCFileType.Image;
                    break;
                case XCCBinaryMessageType.Audio:
                    fileType = XCCFileType.Audio;
                    break;
                case XCCBinaryMessageType.AudioBuffer:
                    break;
                case XCCBinaryMessageType.Video:
                    fileType = XCCFileType.Video;
                    break;
                default:
                    break;
            }
            var xCCFile = new XCCFile(e.GroupId, e.MessageId!, fileType, e.Sender!, e.SendTime, e.BinaryMessage);
            xCCFileDictionary.Add(e.MessageId!, xCCFile);
            if (!e.IsHistory)
                FileReceived?.Invoke(e.IsHistory, xCCFile);
        }
    }
    private void XCCNetWork_ExceptionMessageReceived(object? sender, XCCExceptionMessageReceivedEventArgs e)
    {
        ExceptionOccurred?.Invoke(e.Exception);
    }
    /// <summary>
    /// XCC消息接收器
    /// </summary>
    /// <param name="savePathRoot">保存根目录</param>
    /// <param name="autoSaveInLocal">自动保存</param>
    public XCCMessageReceiveHelper(string savePathRoot, bool autoSaveInLocal = true)
    {
        AutoSaveInLocal = autoSaveInLocal;
        SavePathRoot = savePathRoot;
    }
    /// <summary>
    /// XCC消息接收器
    /// </summary>
    /// <param name="savePathRoot">保存根目录</param>
    /// <param name="xCCNetWork">XCC网络通讯实例</param>
    /// <param name="autoSaveInLocal">自动保存</param>
    public XCCMessageReceiveHelper(string savePathRoot, XCCNetWork xCCNetWork, bool autoSaveInLocal = true)
    {
        AutoSaveInLocal = autoSaveInLocal;
        SavePathRoot = savePathRoot;
        xCCNetWork.TextMessageReceived += ReceiveTextMessage;
        xCCNetWork.BinaryMessageReceived += ReceiveBinaryMessage;
        xCCNetWork.ExceptionMessageReceived += XCCNetWork_ExceptionMessageReceived;
    }
}
