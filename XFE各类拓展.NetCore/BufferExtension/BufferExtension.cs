using System.Text;

namespace XFE各类拓展.NetCore.BufferExtension;

/// <summary>
/// 对二进制数组的拓展
/// </summary>
public static class BufferExtension
{
    /// <summary>
    /// 获取第一个匹配Buffer的位置
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="targetBuffer">要匹配Buffer</param>
    /// <returns>没有查找到为-1</returns>
    public static int IndexOf(this byte[] buffer, byte[] targetBuffer)
    {
        int index = -1;
        for (int i = 0; i < buffer.Length; i++)
        {
            if (buffer[i] == targetBuffer[0])
            {
                bool isMatch = true;
                for (int j = 0; j < targetBuffer.Length; j++)
                {
                    if (buffer[i + j] != targetBuffer[j])
                    {
                        isMatch = false;
                        break;
                    }
                }
                if (isMatch)
                {
                    index = i;
                    break;
                }
            }
        }
        return index;
    }
    /// <summary>
    /// 获取所有匹配的Buffer的位置
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="targetBuffer">要匹配的Buffer</param>
    /// <param name="shareable">是否可以共用</param>
    /// <returns>查找到的位置数组</returns>
    public static int[] IndexesOf(this byte[] buffer, byte[] targetBuffer, bool shareable = false)
    {
        var indexes = new List<int>();
        for (int i = 0; i < buffer.Length; i++)
        {
            if (buffer[i] == targetBuffer[0])
            {
                bool isMatch = true;
                for (int j = 0; j < targetBuffer.Length; j++)
                {
                    if (buffer[i + j] != targetBuffer[j])
                    {
                        isMatch = false;
                        break;
                    }
                }
                if (isMatch)
                {
                    indexes.Add(i);
                    if (!shareable)
                        i += targetBuffer.Length - 1;
                }
            }
        }
        return [.. indexes];
    }
    /// <summary>
    /// 替换Buffer中的指定Buffer为目标Buffer
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="originBuffer"></param>
    /// <param name="targetBuffer"></param>
    /// <returns>替换后的Buffer</returns>
    public static byte[] Replace(this byte[] buffer, byte[] originBuffer, byte[] targetBuffer)
    {
        if (originBuffer is null || originBuffer.LongLength == 0)
        {
            throw new ArgumentException("Origin buffer cannot be null or empty.");
        }
        if (buffer is null || buffer.LongLength == 0)
        {
            throw new ArgumentException("Input buffer cannot be null or empty.");
        }
        List<byte> result = new List<byte>();
        for (long i = 0; i <= buffer.LongLength - originBuffer.LongLength; i++)
        {
            bool isMatch = true;
            for (long j = 0; j < originBuffer.LongLength; j++)
            {
                if (buffer[i + j] != originBuffer[j])
                {
                    isMatch = false;
                    break;
                }
            }
            if (isMatch)
            {
                result.AddRange(targetBuffer);
                i += originBuffer.LongLength - 1;
            }
            else
            {
                result.Add(buffer[i]);
            }
        }
        for (long i = buffer.LongLength - originBuffer.LongLength + 1; i < buffer.LongLength; i++)
        {
            result.Add(buffer[i]);
        }
        return [.. result];
    }

    /// <summary>
    /// 分割Buffer
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="targetBuffer">分割器</param>
    /// <returns></returns>
    public static List<byte[]> Split(this byte[] buffer, byte[] targetBuffer)
    {
        var indexes = buffer.IndexesOf(targetBuffer);
        var buffers = new List<byte[]>();
        long index = 0;
        for (long i = 0; i < indexes.LongLength; i++)
        {
            var newBuffer = new byte[indexes[i] - index];
            for (long j = index; j < indexes[i]; j++)
            {
                newBuffer[j - index] = buffer[j];
            }
            buffers.Add(newBuffer);
            index = indexes[i] + targetBuffer.LongLength;
        }
        var lastBuffer = new byte[buffer.LongLength - index];
        for (long i = index; i < buffer.LongLength; i++)
        {
            lastBuffer[i - index] = buffer[i];
        }
        buffers.Add(lastBuffer);
        return buffers;
    }
    /// <summary>
    /// 将Buffer转换为XFEBuffer
    /// </summary>
    /// <param name="buffers"></param>
    /// <returns></returns>
    public static byte[] PackBuffer(this List<byte[]> buffers)
    {
        var packedBuffer = new List<byte>();
        for (int i = 0; i < buffers.Count; i++)
        {
            if (i != 0)
                packedBuffer.AddRange(new List<byte> { 0x01, 0x02, 0x03 });
            packedBuffer.AddRange(buffers[i].Replace(new byte[] { 0x02, 0x03 }, new byte[] { 0x02, 0x02, 0x03 }).ToList());
        }
        return [.. packedBuffer];
    }
    /// <summary>
    /// 为Buffer添加头部并封装为XFEBuffer
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="headers"></param>
    /// <returns></returns>
    public static byte[] AddHeaderAndPack(this byte[] buffer, params string[] headers)
    {
        var buffers = new List<byte[]>();
        foreach (var header in headers)
        {
            buffers.Add(Encoding.UTF8.GetBytes(header));
        }
        buffers.Add(buffer);
        return buffers.PackBuffer();
    }
    /// <summary>
    /// 为Buffer添加头部并封装为XFEBuffer
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="headers"></param>
    /// <returns></returns>
    public static byte[] AddHeaderAndPack(this byte[] buffer, params byte[] headers)
    {
        return new List<byte[]>() { headers, buffer }.PackBuffer();
    }
    /// <summary>
    /// 将XEFBuffer转换为BufferList
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static List<byte[]> UnPackBuffer(this byte[] buffer)
    {
        var unPackedBuffers = new List<byte[]>();
        foreach (var unPackedBuffer in buffer.Split(new byte[] { 0x01, 0x02, 0x03 }))
        {
            unPackedBuffers.Add(unPackedBuffer.Replace(new byte[] { 0x02, 0x02, 0x03 }, new byte[] { 0x02, 0x03 }));
        }
        return unPackedBuffers;
    }
}