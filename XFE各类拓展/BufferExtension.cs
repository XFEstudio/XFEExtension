using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XFE各类拓展
{
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
            return indexes.ToArray();
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
            var indexes = buffer.IndexesOf(originBuffer);
            var newBuffer = new byte[buffer.Length + (targetBuffer.Length - originBuffer.Length) * indexes.Length];
            int index = 0;
            for (int i = 0; i < indexes.Length; i++)
            {
                for (int j = index; j < indexes[i]; j++)
                {
                    newBuffer[j] = buffer[j];
                }
                for (int j = 0; j < targetBuffer.Length; j++)
                {
                    newBuffer[indexes[i] + j] = targetBuffer[j];
                }
                index = indexes[i] + targetBuffer.Length;
            }
            for (int i = index; i < newBuffer.Length; i++)
            {
                newBuffer[i] = buffer[i];
            }
            return newBuffer;
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
            int index = 0;
            for (int i = 0; i < indexes.Length; i++)
            {
                var newBuffer = new byte[indexes[i] - index];
                for (int j = index; j < indexes[i]; j++)
                {
                    newBuffer[j - index] = buffer[j];
                }
                buffers.Add(newBuffer);
                index = indexes[i] + targetBuffer.Length;
            }
            var lastBuffer = new byte[buffer.Length - index];
            for (int i = index; i < buffer.Length; i++)
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
                if (i != 0 && i != buffers.Count - 1)
                    packedBuffer.AddRange(new List<byte> { 0x01, 0x02, 0x03 });
                packedBuffer.AddRange(buffers[i].Replace(new byte[] { 0x02, 0x03 }, new byte[] { 0x02, 0x02, 0x03 }).ToList());
            }
            return packedBuffer.ToArray();
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
        /// 将XEFBuffer转换为BufferList
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static List<byte[]> UnPackBuffer(this byte[] buffer)
        {
            var unPackedBuffers = buffer.Split(new byte[] { 0x01, 0x02, 0x03 });
            foreach (var unPackedBuffer in unPackedBuffers)
            {
                unPackedBuffer.Replace(new byte[] { 0x02, 0x02, 0x03 }, new byte[] { 0x02, 0x03 });
            }
            return unPackedBuffers;
        }
    }
    /// <summary>
    /// XFE的二进制数组协议
    /// </summary>
    public class XFEBuffer
    {
        private Dictionary<string, byte[]> xFEBuffer;
        private List<byte[]> headerBuffers;
        /// <summary>
        /// 长度
        /// </summary>
        public int Count
        {
            get
            {
                return xFEBuffer.Count;
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
                return xFEBuffer[header];
            }
            set
            {
                xFEBuffer[header] = value;
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
                    xFEBuffer.xFEBuffer.Add(Encoding.UTF8.GetString(buffers[i]), buffers[i + 1]);
                    xFEBuffer.headerBuffers.Add(buffers[i]);
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
            xFEBuffer.Add(header, buffer);
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
                xFEBuffer.Add(@params[i].ToString(), (byte[])@params[i + 1]);
                headerBuffers.Add(Encoding.UTF8.GetBytes(@params[i].ToString()));
                headerBuffers.Add((byte[])@params[i + 1]);
            }
        }
        /// <summary>
        /// 移除XFEBuffer
        /// </summary>
        /// <param name="header">头</param>
        public void Remove(string header)
        {
            xFEBuffer.Remove(header);
            headerBuffers.Remove(Encoding.UTF8.GetBytes(header));
        }
        /// <summary>
        /// 移除指定位置的XFEBuffer
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            var header = xFEBuffer.Keys.ToArray()[index];
            xFEBuffer.Remove(header);
            headerBuffers.Remove(Encoding.UTF8.GetBytes(header));
        }
        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            xFEBuffer.Clear();
            headerBuffers.Clear();
        }
        /// <summary>
        /// 是否包含
        /// </summary>
        /// <param name="header">头</param>
        /// <returns></returns>
        public bool Contains(string header)
        {
            return xFEBuffer.ContainsKey(header);
        }
        /// <summary>
        /// 获取Buffer
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public byte[] GetBuffer(string header)
        {
            return xFEBuffer[header];
        }
        /// <summary>
        /// 获取头
        /// </summary>
        /// <returns></returns>
        public string[] GetHeaders()
        {
            return xFEBuffer.Keys.ToArray();
        }
        /// <summary>
        /// XFE的二进制数组协议
        /// </summary>
        public XFEBuffer()
        {
            xFEBuffer = new Dictionary<string, byte[]>();
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
                xFEBuffer.Add(@params[i].ToString(), (byte[])@params[i + 1]);
                headerBuffers.Add(Encoding.UTF8.GetBytes(@params[i].ToString()));
                headerBuffers.Add((byte[])@params[i + 1]);
            }
        }
    }
}
