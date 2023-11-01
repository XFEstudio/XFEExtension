using System.Collections.Generic;

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
        public static byte[] Replace(byte[] buffer, byte[] originBuffer, byte[] targetBuffer)
        {
            var indexes = IndexesOf(buffer, originBuffer);
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
            var indexes = IndexesOf(buffer, targetBuffer);
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
    }
}
