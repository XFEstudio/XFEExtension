using System;

namespace XFE各类拓展.ArrysExtension
{
    namespace AI
    {
        /// <summary>
        /// 对神经网络算法的数组矩阵的拓展
        /// </summary>
        public static class MatrixOfArrysExtension
        {
            /// <summary>
            /// 以矩阵形式打印数组
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="arrays"></param>
            public static void OutPutInMatrix<T>(this T[] arrays)
            {
                int i;
                Console.Write("[");
                for (i = 0; i < arrays.GetLength(0) - 1; i++)
                {
                    Console.Write($"{arrays[i]}\t");
                }
                Console.Write($"{arrays[i]}]\n");
            }
            /// <summary>
            /// 以矩阵形式打印数组
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="arrays"></param>
            public static void OutPutInMatrix<T>(this T[][] arrays)
            {
                for (int i = 0; i < arrays.GetLength(0); i++)
                {
                    int j;
                    Console.Write("[");
                    for (j = 0; j < arrays.GetLength(1) - 1; j++)
                    {
                        Console.Write($"{arrays[i][j]}\t");
                    }
                    Console.Write($"{arrays[i][j++]}]\n");
                }
            }
            /// <summary>
            /// 以矩阵形式打印数组
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="arrays"></param>
            public static void OutPutInMatrix<T>(this T[,] arrays)
            {
                for (int i = 0; i < arrays.GetLength(0); i++)
                {
                    int j;
                    Console.Write("[");
                    for (j = 0; j < arrays.GetLength(1) - 1; j++)
                    {
                        Console.Write($"{arrays[i, j]}\t");
                    }
                    Console.Write($"{arrays[i, j++]}]\n");
                }
            }
        }
    }
}