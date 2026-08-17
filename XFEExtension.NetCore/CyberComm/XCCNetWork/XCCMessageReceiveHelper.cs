using XFEExtension.NetCore.DelegateExtension;
using XFEExtension.NetCore.Exceptions;
using System.Text;
using XFEExtension.NetCore.FormatExtension;

namespace XFEExtension.NetCore.CyberComm.XCCNetWork;

/// <summary>
/// XCC消息接收器
/// </summary>
public class XCCMessageReceiveHelper
{
    private readonly Dictionary<(string GroupId, string MessageId), XCCFile> _xCCFileDictionary = [];
    private readonly Dictionary<string, List<XCCMessage>> _xCCMessageDictionary = [];
    private bool _loaded;
    /// <summary>
    /// 自动保存到本地
    /// </summary>
    public bool AutoSaveInLocal { get; set; }
    /// <summary>
    /// 保存的根目录
    /// </summary>
    public string SavePathRoot { get; set; }
    /// <summary>单个文件的最大字节数。</summary>
    public long MaxFileBytes { get; set; } = 64 * 1024 * 1024;
    /// <summary>单条文本消息的最大 UTF-8 字节数。</summary>
    public int MaxTextMessageBytes { get; set; } = 1024 * 1024;
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
                    var messagePath = Path.Combine(groupIdFullPath, "XFEMessage", "XFEMessage.xfe");
                    if (File.Exists(messagePath))
                    {
                        var xCCMessageList = new List<XCCMessage>();
                        foreach (var entry in new XFEMultiDictionary(File.ReadAllText(messagePath)))
                        {
                            var xCCMessage = XCCMessage.ConvertToXCCMessage(entry.Content, groupId);
                            xCCMessageList.Add(xCCMessage);
                            if (xCCMessage.MessageType == XCCTextMessageType.Text)
                                TextReceived?.Invoke(true, xCCMessage);
                            else
                                FileReceived?.Invoke(true, LoadFile(xCCMessage)!);
                        }
                        _xCCMessageDictionary.Add(groupId, xCCMessageList);
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(SavePathRoot);
            }
        });
        _loaded = true;
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
            var messagePath = Path.Combine(GetSafeGroupDirectory(groupId), "XFEMessage", "XFEMessage.xfe");
            if (File.Exists(messagePath))
            {
                var xCCMessageList = new List<XCCMessage>();
                foreach (var entry in new XFEMultiDictionary(File.ReadAllText(messagePath)))
                {
                    var xCCMessage = XCCMessage.ConvertToXCCMessage(entry.Content, groupId);
                    xCCMessageList.Add(xCCMessage);
                    if (xCCMessage.MessageType == XCCTextMessageType.Text)
                        TextReceived?.Invoke(true, xCCMessage);
                    else
                        FileReceived?.Invoke(true, LoadFile(xCCMessage)!);
                }
                _xCCMessageDictionary.Add(groupId, xCCMessageList);
            }
        });
        _loaded = true;
    }
    /// <summary>
    /// 清理无用文件
    /// </summary>
    /// <returns></returns>
    public async Task ClearUselessFile()
    {
        if (!_loaded)
            throw new XFEExtensionException("不能在加载完成前调用清理");
        await Task.Run(() =>
        {
            foreach (var groupId in _xCCMessageDictionary.Keys)
            {
                foreach (var filePath in Directory.EnumerateFiles(GetSafeGroupDirectory(groupId), "*.xfe", SearchOption.TopDirectoryOnly))
                {
                    var messageId = Path.GetFileNameWithoutExtension(filePath);
                    if (!_xCCMessageDictionary.TryGetValue(groupId, out var value) || value.Find(x => x.MessageId == messageId) is null)
                    {
                        File.Delete(filePath);
                    }
                }
            }
        });
    }
    private XCCFile? LoadFile(XCCMessage xCCMessage)
    {
        var filePath = GetSafeFilePath(xCCMessage.GroupId, xCCMessage.MessageId);
        byte[]? fileBuffer = null;
        if (File.Exists(filePath) && new FileInfo(filePath).Length <= MaxFileBytes)
            fileBuffer = File.ReadAllBytes(filePath);
        XCCFile xCCFile;
        if (_xCCFileDictionary.TryGetValue((xCCMessage.GroupId, xCCMessage.MessageId), out var value))
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
        _xCCFileDictionary.Add((xCCMessage.GroupId, xCCMessage.MessageId), xCCFile);
        return xCCFile;
    }
    /// <summary>
    /// 获取文件
    /// </summary>
    /// <param name="messageId">消息ID</param>
    /// <returns></returns>
    public XCCFile? GetFile(string messageId)
    {
        return _xCCFileDictionary.FirstOrDefault(pair => pair.Key.MessageId == messageId).Value;
    }
    /// <summary>按群组和消息 ID 获取文件，避免跨群 ID 冲突。</summary>
    public XCCFile? GetFile(string groupId, string messageId) =>
        _xCCFileDictionary.TryGetValue((groupId, messageId), out var value) ? value : null;
    /// <summary>
    /// 添加文件
    /// </summary>
    /// <param name="xCCFile">XCC文件实例</param>
    public void AddFile(XCCFile xCCFile)
    {
        _xCCFileDictionary.Add((xCCFile.GroupId, xCCFile.MessageId), xCCFile);
        if (AutoSaveInLocal && xCCFile.FileBuffer is not null)
            SaveFile(xCCFile);
    }
    /// <summary>
    /// 保存文件
    /// </summary>
    /// <param name="xCCFile">XCC文件实例</param>
    public void SaveFile(XCCFile xCCFile)
    {
        if (xCCFile.FileBuffer is null) throw new ArgumentException("文件内容不能为空", nameof(xCCFile));
        if (xCCFile.FileBuffer.LongLength > MaxFileBytes) throw new XFEExtensionException("文件超过允许的大小");
        var filePath = GetSafeFilePath(xCCFile.GroupId, xCCFile.MessageId);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        AtomicWrite(filePath, xCCFile.FileBuffer);
    }
    /// <summary>
    /// 保存群组消息
    /// </summary>
    /// <param name="groupId"></param>
    public void SaveMessage(string groupId)
    {
        var filePath = Path.Combine(GetSafeGroupDirectory(groupId), "XFEMessage");
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }
        var storageDictionary = new XFEDictionary();
        foreach (var xCCMessage in _xCCMessageDictionary[groupId])
        {
            storageDictionary.Add(xCCMessage.MessageId, xCCMessage.ToString());
        }
        AtomicWrite(Path.Combine(filePath, "XFEMessage.xfe"), System.Text.Encoding.UTF8.GetBytes(storageDictionary.ToString()));
    }
    private void ReceiveFilePlaceHolder(XCCTextMessageReceivedEventArgs e, XCCFileType fileType)
    {
        var xCCFile = new XCCFile(e.GroupId, e.MessageId!, fileType, e.Sender!, e.SendTime);
        _xCCFileDictionary.TryAdd((e.GroupId, e.MessageId!), xCCFile);
        FileReceived?.Invoke(e.IsHistory, xCCFile);
        if (AutoSaveInLocal)
            SaveMessage(e.GroupId);
    }
    private void ReceiveTextMessage(object? sender, XCCTextMessageReceivedEventArgs e)
    {
        if (Encoding.UTF8.GetByteCount(e.TextMessage ?? string.Empty) > MaxTextMessageBytes)
        {
            ExceptionOccurred?.Invoke(new XFECyberCommException("接收到的 XCC 文本消息超过允许的大小"));
            return;
        }
        var message = new XCCMessage(e.MessageId!, e.MessageType, e.TextMessage ?? string.Empty, e.Sender!, e.SendTime, e.GroupId);
        if (_xCCMessageDictionary.TryGetValue(e.GroupId, out var value))
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
            _xCCMessageDictionary.Add(e.GroupId, [message]);
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
        }
    }
    private void ReceiveBinaryMessage(object? sender, XCCBinaryMessageReceivedEventArgs e)
    {
        if (e.MessageType == XCCBinaryMessageType.AudioBuffer)
        {
            AudioBufferReceived?.Invoke(e.BinaryMessage);
            return;
        }
        if (e.BinaryMessage.LongLength > MaxFileBytes)
        {
            ExceptionOccurred?.Invoke(new XFECyberCommException("接收到的 XCC 文件超过允许的大小"));
            return;
        }
        if (_xCCFileDictionary.TryGetValue((e.GroupId, e.MessageId!), out var value))
        {
            if (!value.Loaded)
            {
                value.LoadFile(e.BinaryMessage);
                if (AutoSaveInLocal)
                    SaveFile(value);
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
            }
            var xCCFile = new XCCFile(e.GroupId, e.MessageId!, fileType, e.Sender!, e.SendTime, e.BinaryMessage);
            _xCCFileDictionary.Add((e.GroupId, e.MessageId!), xCCFile);
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
        SavePathRoot = Path.GetFullPath(savePathRoot);
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
        SavePathRoot = Path.GetFullPath(savePathRoot);
        xCCNetWork.TextMessageReceived += ReceiveTextMessage;
        xCCNetWork.BinaryMessageReceived += ReceiveBinaryMessage;
        xCCNetWork.ExceptionMessageReceived += XCCNetWork_ExceptionMessageReceived;
    }

    private string GetSafeGroupDirectory(string groupId)
    {
        ValidatePathSegment(groupId, nameof(groupId));
        var root = Path.GetFullPath(SavePathRoot);
        var path = Path.GetFullPath(Path.Combine(root, groupId));
        if (!path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new XFEExtensionException("群组路径超出保存根目录");
        return path;
    }

    private string GetSafeFilePath(string groupId, string messageId)
    {
        ValidatePathSegment(messageId, nameof(messageId));
        return Path.Combine(GetSafeGroupDirectory(groupId), $"{messageId}.xfe");
    }

    private static void ValidatePathSegment(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value) || value is "." or ".." ||
            value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || value.Contains(Path.DirectorySeparatorChar) || value.Contains(Path.AltDirectorySeparatorChar))
            throw new ArgumentException("标识不能包含路径字符", parameterName);
    }

    private static void AtomicWrite(string path, byte[] content)
    {
        var temporaryPath = $"{path}.{Guid.NewGuid():N}.tmp";
        try
        {
            File.WriteAllBytes(temporaryPath, content);
            File.Move(temporaryPath, path, true);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }
}
