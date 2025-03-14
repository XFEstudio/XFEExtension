using System.Collections;
using System.Text;

namespace XFEExtension.NetCore.BufferExtension;

/// <summary>
/// XFE的二进制数组协议
/// </summary>
public class XFEBuffer : IEnumerable<KeyValuePair<string, byte[]>>
{
    private readonly Dictionary<string, byte[]> bufferDictionary = [];
    private readonly List<byte[]> headerBuffers = [];
    /// <summary>
    /// 所有的头
    /// </summary>
    public Dictionary<string, byte[]>.KeyCollection Headers
    {
        get
        {
            return bufferDictionary.Keys;
        }
    }
    /// <summary>
    /// 所有的值
    /// </summary>
    public Dictionary<string, byte[]>.ValueCollection Buffers
    {
        get
        {
            return bufferDictionary.Values;
        }
    }
    /// <summary>
    /// 长度
    /// </summary>
    public int Count
    {
        get
        {
            return bufferDictionary.Count;
        }
    }
    /// <summary>
    /// 获取或设置Buffer
    /// </summary>
    /// <param name="header"></param>
    /// <returns></returns>
    public byte[] this[string header]
    {
        get
        {
            return bufferDictionary[header];
        }
        set
        {
            bufferDictionary[header] = value;
        }
    }
    /// <summary>
    /// 封装Buffer
    /// </summary>
    /// <returns></returns>

    public byte[] ToBuffer()
    {
        return headerBuffers.PackBuffer();
    }
    /// <summary>
    /// 将Buffer转换为XFEBuffer
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static XFEBuffer ToXFEBuffer(byte[] buffer)
    {
        var xFEBuffer = new XFEBuffer();
        var buffers = buffer.UnPackBuffer();
        for (int i = 0; i < buffers.Count; i++)
        {
            if (i % 2 == 0)
            {
                xFEBuffer.bufferDictionary.Add(Encoding.UTF8.GetString(buffers[i]), buffers[i + 1]);
                xFEBuffer.headerBuffers.Add(buffers[i]);
                xFEBuffer.headerBuffers.Add(buffers[i + 1]);
            }
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
        bufferDictionary.Add(header, buffer);
        headerBuffers.Add(Encoding.UTF8.GetBytes(header));
        headerBuffers.Add(buffer);
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
        for (int i = 0; i < @params.Length; i += 2)
        {
            bufferDictionary.Add(@params[i].ToString()!, (byte[])@params[i + 1]);
            headerBuffers.Add(Encoding.UTF8.GetBytes(@params[i].ToString()!));
            headerBuffers.Add((byte[])@params[i + 1]);
        }
    }
    /// <summary>
    /// 移除XFEBuffer
    /// </summary>
    /// <param name="header">头</param>
    public void Remove(string header)
    {
        bufferDictionary.Remove(header);
        headerBuffers.Remove(Encoding.UTF8.GetBytes(header));
    }
    /// <summary>
    /// 移除指定位置的XFEBuffer
    /// </summary>
    /// <param name="index"></param>
    public void RemoveAt(int index)
    {
        var header = bufferDictionary.Keys.ToArray()[index];
        bufferDictionary.Remove(header);
        headerBuffers.Remove(Encoding.UTF8.GetBytes(header));
    }
    /// <summary>
    /// 清空
    /// </summary>
    public void Clear()
    {
        bufferDictionary.Clear();
        headerBuffers.Clear();
    }
    /// <summary>
    /// 是否包含
    /// </summary>
    /// <param name="header">头</param>
    /// <returns></returns>
    public bool Contains(string header)
    {
        return bufferDictionary.ContainsKey(header);
    }
    /// <summary>
    /// 获取头
    /// </summary>
    /// <returns></returns>
    public string[] GetHeaders()
    {
        return [.. bufferDictionary.Keys];
    }
    /// <summary>
    /// 获取枚举器
    /// </summary>
    /// <returns></returns>
    public IEnumerator<KeyValuePair<string, byte[]>> GetEnumerator()
    {
        return bufferDictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)bufferDictionary).GetEnumerator();
    }

    /// <summary>
    /// XFE的二进制数组协议
    /// </summary>
    public XFEBuffer()
    {
        bufferDictionary = [];
    }
    /// <summary>
    /// XFE的二进制数组协议
    /// </summary>
    /// <param name="params">头和Buffer对</param>
    /// <exception cref="XFEExtensionException"></exception>
    public XFEBuffer(params object[] @params)
    {
        if (@params.Length % 2 != 0)
            throw new XFEExtensionException("头和Buffer必须成对输入");
        for (int i = 0; i < @params.Length; i += 2)
        {
            bufferDictionary.Add(@params[i].ToString()!, (byte[])@params[i + 1]);
            headerBuffers.Add(Encoding.UTF8.GetBytes(@params[i].ToString()!));
            headerBuffers.Add((byte[])@params[i + 1]);
        }
    }
}
