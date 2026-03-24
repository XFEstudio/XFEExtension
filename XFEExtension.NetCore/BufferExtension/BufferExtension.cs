using System.Text;

namespace XFEExtension.NetCore.BufferExtension;

/// <summary>
/// 对二进制数组的拓展
/// </summary>
public static class BufferExtension
{
    /// <param name="buffer"></param>
    extension(byte[]? buffer)
    {
        /// <summary>
        /// 获取第一个匹配Buffer的位置
        /// </summary>
        /// <param name="targetBuffer">要匹配Buffer</param>
        /// <returns>没有查找到为-1</returns>
        public int IndexOf(byte[] targetBuffer)
        {
            var index = -1;
            if (buffer is null)
                return index;

            for (var i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] != targetBuffer[0])
                    continue;
                var isMatch = true;
                for (var j = targetBuffer.Length - 1; j >= 0; j--)
                {
                    if (buffer[i + j] == targetBuffer[j])
                        continue;
                    isMatch = false;
                    break;
                }

                if (!isMatch)
                    continue;
                index = i;
                break;
            }
            return index;
        }

        /// <summary>
        /// 获取所有匹配的Buffer的位置
        /// </summary>
        /// <param name="targetBuffer">要匹配的Buffer</param>
        /// <param name="shareable">是否可以共用</param>
        /// <returns>查找到的位置数组</returns>
        public int[] IndexesOf(byte[] targetBuffer, bool shareable = false)
        {
            var indexes = new List<int>();
            if (buffer == null)
                return [.. indexes];

            for (var i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] != targetBuffer[0])
                    continue;
                var isMatch = !targetBuffer.Where((t, j) => buffer[i + j] != t).Any();
                if (!isMatch) continue;
                indexes.Add(i);
                if (!shareable)
                    i += targetBuffer.Length - 1;
            }

            return [.. indexes];
        }

        /// <summary>
        /// 替换Buffer中的指定Buffer为目标Buffer
        /// </summary>
        /// <param name="originBuffer"></param>
        /// <param name="targetBuffer"></param>
        /// <returns>替换后的Buffer</returns>
        public byte[] Replace(byte[] originBuffer, byte[] targetBuffer)
        {
            if (originBuffer is null || originBuffer.LongLength == 0)
                throw new ArgumentException("Origin buffer cannot be null or empty.");
            if (buffer is null || buffer.LongLength == 0)
                return [];
            List<byte> result = [];
            for (long i = 0; i <= buffer.LongLength - originBuffer.LongLength; i++)
            {
                var isMatch = true;
                for (long j = 0; j < originBuffer.LongLength; j++)
                {
                    if (buffer[i + j] == originBuffer[j])
                        continue;
                    isMatch = false;
                    break;
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
            for (var i = buffer.LongLength - originBuffer.LongLength + 1; i < buffer.LongLength; i++)
            {
                if (i >= 0)
                    result.Add(buffer[i]);
            }
            return [.. result];
        }

        /// <summary>
        /// 分割Buffer
        /// </summary>
        /// <param name="targetBuffer">分割器</param>
        /// <returns></returns>
        public List<byte[]> Split(byte[] targetBuffer)
        {
            var indexes = buffer.IndexesOf(targetBuffer);
            var buffers = new List<byte[]>();
            long index = 0;
            for (long i = 0; i < indexes.LongLength; i++)
            {
                var newBuffer = new byte[indexes[i] - index];
                for (var j = index; j < indexes[i]; j++)
                    if (buffer is not null)
                        newBuffer[j - index] = buffer[j];
                buffers.Add(newBuffer);
                index = indexes[i] + targetBuffer.LongLength;
            }

            if (buffer is null)
                return buffers;
            var lastBuffer = new byte[buffer.LongLength - index];
            for (var i = index; i < buffer.LongLength; i++)
                lastBuffer[i - index] = buffer[i];
            buffers.Add(lastBuffer);
            return buffers;
        }
    }

    /// <summary>
    /// 将Buffer转换为XFEBuffer
    /// </summary>
    /// <param name="buffers"></param>
    /// <returns></returns>
    public static byte[] PackBuffer(this List<byte[]> buffers)
    {
        var packedBuffer = new List<byte>();
        for (var i = 0; i < buffers.Count; i++)
        {
            if (i != 0)
                packedBuffer.AddRange(new List<byte> { 0x01, 0x02, 0x03 });
            packedBuffer.AddRange(buffers[i].Replace([0x02, 0x03], [0x02, 0x02, 0x03]).ToList());
        }
        return [.. packedBuffer];
    }

    /// <param name="buffer"></param>
    extension(byte[] buffer)
    {
        /// <summary>
        /// 为Buffer添加头部并封装为XFEBuffer
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        public byte[] AddHeaderAndPack(params string[] headers)
        {
            var buffers = headers.Select(header => Encoding.UTF8.GetBytes(header)).ToList();
            buffers.Add(buffer);
            return buffers.PackBuffer();
        }

        /// <summary>
        /// 为Buffer添加头部并封装为XFEBuffer
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        public byte[] AddHeaderAndPack(params byte[] headers) => new List<byte[]> { headers, buffer }.PackBuffer();

        /// <summary>
        /// 将XEFBuffer转换为BufferList
        /// </summary>
        /// <returns></returns>
        public List<byte[]> UnPackBuffer() => buffer.Split([0x01, 0x02, 0x03]).Select(unPackedBuffer => unPackedBuffer.Replace([0x02, 0x02, 0x03], [0x02, 0x03])).ToList();
    }
}