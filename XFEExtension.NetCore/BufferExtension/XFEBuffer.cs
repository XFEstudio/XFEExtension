using System.Collections;
using System.Text;

namespace XFEExtension.NetCore.BufferExtension;

/// <summary>
/// XFE的二进制数组协议
/// </summary>
public class XFEBuffer : IEnumerable<KeyValuePair<string, byte[]>>
{
    private readonly Dictionary<string, byte[]> _bufferDictionary = [];
    private readonly List<byte[]> _headerBuffers = [];
    /// <summary>
    /// 所有的头
    /// </summary>
    public Dictionary<string, byte[]>.KeyCollection Headers => _bufferDictionary.Keys;

    /// <summary>
    /// 所有的值
    /// </summary>
    public Dictionary<string, byte[]>.ValueCollection Buffers => _bufferDictionary.Values;

    /// <summary>
    /// 长度
    /// </summary>
    public int Count => _bufferDictionary.Count;

    /// <summary>
    /// 获取或设置Buffer
    /// </summary>
    /// <param name="header"></param>
    /// <returns></returns>
    public byte[] this[string header]
    {
        get => _bufferDictionary[header];
        set => _bufferDictionary[header] = value;
    }

    /// <summary>
    /// 封装Buffer
    /// </summary>
    /// <returns></returns>
    public byte[] ToBuffer() => _headerBuffers.PackBuffer();

    /// <summary>
    /// 将Buffer转换为XFEBuffer
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static XFEBuffer ToXFEBuffer(byte[] buffer)
    {
        var xFEBuffer = new XFEBuffer();
        var buffers = buffer.UnPackBuffer();
        for (var i = 0; i < buffers.Count; i++)
        {
            if (i % 2 != 0)
                continue;
            xFEBuffer._bufferDictionary.Add(Encoding.UTF8.GetString(buffers[i]), buffers[i + 1]);
            xFEBuffer._headerBuffers.Add(buffers[i]);
            xFEBuffer._headerBuffers.Add(buffers[i + 1]);
        }
        return xFEBuffer;
    }
    /// <summary>
    /// 添加XFEBuffer
    /// </summary>
    /// <param name="header">头</param>
    /// <param name="buffer">Buffer</param>
    public void Add(string header, byte[] buffer)
    {
        _bufferDictionary.Add(header, buffer);
        _headerBuffers.Add(Encoding.UTF8.GetBytes(header));
        _headerBuffers.Add(buffer);
    }
    /// <summary>
    /// 添加XFEBuffer
    /// </summary>
    /// <param name="params"></param>
    /// <exception cref="XFEExtensionException"></exception>
    public void AddRange(params object[] @params)
    {
        if (@params.Length % 2 != 0)
            throw new XFEExtensionException("头和Buffer必须成对输入");
        for (var i = 0; i < @params.Length; i += 2)
        {
            _bufferDictionary.Add(@params[i].ToString()!, (byte[])@params[i + 1]);
            _headerBuffers.Add(Encoding.UTF8.GetBytes(@params[i].ToString()!));
            _headerBuffers.Add((byte[])@params[i + 1]);
        }
    }
    /// <summary>
    /// 移除XFEBuffer
    /// </summary>
    /// <param name="header">头</param>
    public void Remove(string header)
    {
        _bufferDictionary.Remove(header);
        _headerBuffers.Remove(Encoding.UTF8.GetBytes(header));
    }
    /// <summary>
    /// 移除指定位置的XFEBuffer
    /// </summary>
    /// <param name="index"></param>
    public void RemoveAt(int index)
    {
        var header = _bufferDictionary.Keys.ToArray()[index];
        _bufferDictionary.Remove(header);
        _headerBuffers.Remove(Encoding.UTF8.GetBytes(header));
    }
    /// <summary>
    /// 清空
    /// </summary>
    public void Clear()
    {
        _bufferDictionary.Clear();
        _headerBuffers.Clear();
    }
    /// <summary>
    /// 是否包含
    /// </summary>
    /// <param name="header">头</param>
    /// <returns></returns>
    public bool Contains(string header) => _bufferDictionary.ContainsKey(header);

    /// <summary>
    /// 获取头
    /// </summary>
    /// <returns></returns>
    public string[] GetHeaders() => [.. _bufferDictionary.Keys];

    /// <summary>
    /// 获取枚举器
    /// </summary>
    /// <returns></returns>
    public IEnumerator<KeyValuePair<string, byte[]>> GetEnumerator() => _bufferDictionary.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_bufferDictionary).GetEnumerator();

    /// <summary>
    /// XFE的二进制数组协议
    /// </summary>
    public XFEBuffer() => _bufferDictionary = [];

    /// <summary>
    /// XFE的二进制数组协议
    /// </summary>
    /// <param name="params">头和Buffer对</param>
    /// <exception cref="XFEExtensionException"></exception>
    public XFEBuffer(params object[] @params)
    {
        if (@params.Length % 2 != 0)
            throw new XFEExtensionException("头和Buffer必须成对输入");
        for (var i = 0; i < @params.Length; i += 2)
        {
            _bufferDictionary.Add(@params[i].ToString()!, (byte[])@params[i + 1]);
            _headerBuffers.Add(Encoding.UTF8.GetBytes(@params[i].ToString()!));
            _headerBuffers.Add((byte[])@params[i + 1]);
        }
    }
}
