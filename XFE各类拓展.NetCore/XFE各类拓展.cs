using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using XFE各类拓展.ArrayExtension;
using XFE各类拓展.NetCore.AttributeExtension;
using XFE各类拓展.ReflectionExtension;
using XFE各类拓展.StringExtension;
using XFE各类拓展.TaskExtension;

namespace XFE各类拓展
{
    #region 异常
    /// <summary>
    /// XFE各类拓展的异常
    /// </summary>
    public class XFEExtensionException : Exception
    {
        /// <summary>
        /// XFE各类拓展的异常
        /// </summary>
        /// <param name="message"></param>
        internal XFEExtensionException(string message) : base(message) { }
        /// <summary>
        /// XFE各类拓展的异常
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        internal XFEExtensionException(string message, Exception innerException) : base(message, innerException) { }
    }
    /// <summary>
    /// XFEJson转换的异常
    /// </summary>
    public class XFEJsonTransformException : XFEExtensionException
    {
        /// <summary>
        /// XFEJson转换的异常
        /// </summary>
        /// <param name="message"></param>
        internal XFEJsonTransformException(string message) : base(message) { }
        /// <summary>
        /// XFEJson转换的异常
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        internal XFEJsonTransformException(string message, Exception innerException) : base(message, innerException) { }
    }
    /// <summary>
    /// XFEChatGPT的异常
    /// </summary>
    public class XFEChatGPTException : XFEExtensionException
    {
        /// <summary>
        /// XFEChatGPT的异常
        /// </summary>
        /// <param name="message"></param>
        internal XFEChatGPTException(string message) : base(message) { }
        internal XFEChatGPTException(string message, Exception innerException) : base(message, innerException) { }
    }
    /// <summary>
    /// XFE网络通信的异常
    /// </summary>
    public class XFECyberCommException : XFEExtensionException
    {
        /// <summary>
        /// XFE网络通信的异常
        /// </summary>
        /// <param name="message"></param>
        internal XFECyberCommException(string message) : base(message) { }
        internal XFECyberCommException(string message, Exception innerException) : base(message, innerException) { }
    }
    /// <summary>
    /// XFE邮件的异常
    /// </summary>
    public class XFEMailException : XFEExtensionException
    {
        internal XFEMailException(string message) : base(message) { }
        internal XFEMailException(string message, Exception innerException) : base(message, innerException) { }
    }
    /// <summary>
    /// XFE字典的异常
    /// </summary>
    public class XFEDictionaryException : XFEExtensionException
    {
        internal XFEDictionaryException(string message) : base(message) { }
        internal XFEDictionaryException(string message, Exception innerException) : base(message, innerException) { }
    }
    #endregion
    /// <summary>
    /// XFE各类拓展编写
    /// </summary>
    public abstract class XFECode
    {
        private static int CTimeCounter = 0;
        private static bool CurrentMethodIsAsserted = false;
        private static string CurrentAssertMessage = string.Empty;
        #region 暂停
        /// <summary>
        /// 暂停
        /// </summary>
        /// <param name="ShowText">是否显示提示</param>
        protected static void Pause(bool ShowText = true)
        {
            if (ShowText)
            {
                Console.WriteLine("按任意键继续...");
            }
            Console.ReadKey();
        }
        /// <summary>
        /// 暂停
        /// </summary>
        /// <param name="ShowText">显示的文字</param>
        protected static void Pause(string ShowText)
        {
            Console.WriteLine(ShowText);
            Console.ReadKey();
        }
        /// <summary>
        /// 暂停，按下指定按键继续
        /// </summary>
        /// <param name="consoleKey">指定的按键</param>
        protected static void Pause(ConsoleKey consoleKey)
        {
            Console.WriteLine($"按 {consoleKey} 键继续");
            while (Console.ReadKey().Key != consoleKey) ;
        }
        /// <summary>
        /// 暂停，按下指定按键继续
        /// </summary>
        /// <param name="consoleKey">指定的按键</param>
        /// <param name="ShowText">显示的文字</param>
        protected static void Pause(ConsoleKey consoleKey, string ShowText)
        {
            Console.WriteLine(ShowText);
            while (Console.ReadKey().Key != consoleKey) ;
        }
        #endregion
        /// <summary>
        /// 并行循环指定次数
        /// </summary>
        /// <param name="action">执行的操作</param>
        /// <param name="count">循环次数</param>
        /// <param name="autoWaitAll">自动等待所有线程执行完毕</param>
        protected static async Task<List<Task>> Circle(Action action, int count, bool autoWaitAll = false)
        {
            var taskList = new List<Task>();
            for (int i = 0; i < count; i++)
            {
                taskList.Add(action.StartNewTask());
            }
            if (autoWaitAll)
                await Task.WhenAll(taskList);
            return taskList;
        }
        /// <summary>
        /// 并行循环指定次数
        /// </summary>
        /// <param name="action">执行的操作</param>
        /// <param name="count">循环次数</param>
        protected static async void CircleOrderly(Action action, int count)
        {
            for (int i = 0; i < count; i++)
            {
                await action.StartNewTask();
            }
        }
        /// <summary>
        /// 计算一段代码执行所需时间
        /// </summary>
        /// <param name="action"></param>
        /// <param name="autoOutPut">自动输出</param>
        /// <param name="timerName">该次计时标识名</param>
        /// <returns></returns>
        public static TimeSpan CTime(Action action, bool autoOutPut = true, string timerName = "无标识名计时器")
        {
            var timeCounter = new Stopwatch();
            timeCounter.Start();
            action?.Invoke();
            timeCounter.Stop();
            CTimeCounter++;
            var elapsedTime = timeCounter.Elapsed;
            if (autoOutPut)
                if (elapsedTime.TotalHours >= 1)
                    Console.WriteLine($"标识名：{timerName}\t执行批次：{CTimeCounter}\t执行时间：{elapsedTime}");
                else if (elapsedTime.TotalMinutes >= 1)
                    Console.WriteLine($"标识名：{timerName}\t执行批次：{CTimeCounter}\t执行时间: {Math.Floor(elapsedTime.TotalMinutes)} 分 {elapsedTime.Seconds} 秒");
                else if (elapsedTime.TotalSeconds >= 1)
                    Console.WriteLine($"标识名：{timerName}\t执行批次：{CTimeCounter}\t执行时间: {elapsedTime.TotalSeconds:F3} 秒");
                else if (elapsedTime.TotalMilliseconds >= 1)
                    Console.WriteLine($"标识名：{timerName}\t执行批次：{CTimeCounter}\t执行时间: {elapsedTime.TotalMilliseconds:F3} 毫秒");
                else
                    Console.WriteLine($"标识名：{timerName}\t执行批次：{CTimeCounter}\t执行时间: {elapsedTime.TotalMilliseconds * 1000:F3} 纳秒");
            return elapsedTime;
        }
        /// <summary>
        /// 计算一段代码执行所需时间
        /// </summary>
        /// <param name="action"></param>
        /// <param name="autoOutPut">自动输出</param>
        /// <param name="timerName">该次计时标识名</param>
        /// <returns></returns>
        public static async Task<TimeSpan> CTimeAsync(Func<Task> action, bool autoOutPut = true, string timerName = "无标识名计时器")
        {
            var timeCounter = new Stopwatch();
            timeCounter.Start();
            await action?.Invoke();
            timeCounter.Stop();
            CTimeCounter++;
            var elapsedTime = timeCounter.Elapsed;
            if (autoOutPut)
                if (elapsedTime.TotalHours >= 1)
                    Console.WriteLine($"标识名：{timerName}\t执行批次：{CTimeCounter}\t执行时间：{elapsedTime}");
                else if (elapsedTime.TotalMinutes >= 1)
                    Console.WriteLine($"标识名：{timerName}\t执行批次：{CTimeCounter}\t执行时间: {Math.Floor(elapsedTime.TotalMinutes)} 分 {elapsedTime.Seconds} 秒");
                else if (elapsedTime.TotalSeconds >= 1)
                    Console.WriteLine($"标识名：{timerName}\t执行批次：{CTimeCounter}\t执行时间: {elapsedTime.TotalSeconds:F3} 秒");
                else if (elapsedTime.TotalMilliseconds >= 1)
                    Console.WriteLine($"标识名：{timerName}\t执行批次：{CTimeCounter}\t执行时间: {elapsedTime.TotalMilliseconds:F3} 毫秒");
                else
                    Console.WriteLine($"标识名：{timerName}\t执行批次：{CTimeCounter}\t执行时间: {elapsedTime.TotalMilliseconds * 1000:F3} 纳秒");
            return elapsedTime;
        }
        /// <summary>
        /// 计算一段代码执行所需时间
        /// </summary>
        /// <returns></returns>
        /// <exception cref="XFEExtensionException"></exception>
        protected static async Task RunTest()
        {
            var isFirstClass = true;
            var subClasses = Assembly.GetEntryAssembly().GetTypes()
            .Where(type => type.IsSubclassOf(typeof(XFECode)));
            foreach (var subClass in subClasses)
            {
                var mainColor = ConsoleColor.Blue;
                var classColor = ConsoleColor.Green;
                var classBorderColor = ConsoleColor.DarkGreen;
                var methodColor = ConsoleColor.Yellow;
                var methodBorderColor = ConsoleColor.DarkYellow;
                var successColor = ConsoleColor.Green;
                var failColor = ConsoleColor.Red;
                var timeColor = ConsoleColor.Cyan;
                var counterColor = ConsoleColor.Gray;
                if (subClass.GetCustomAttributes(typeof(TestThemeAttribute), false).FirstOrDefault() is TestThemeAttribute themeAttribute)
                {
                    mainColor = themeAttribute.MainColor;
                    classColor = themeAttribute.ClassColor;
                    classBorderColor = themeAttribute.ClassBorderColor;
                    methodColor = themeAttribute.MethodColor;
                    methodBorderColor = themeAttribute.MethodBorderColor;
                    successColor = themeAttribute.SuccessColor;
                    failColor = themeAttribute.FailColor;
                    timeColor = themeAttribute.TimeColor;
                    counterColor = themeAttribute.CounterColor;
                }
                var singleRunMethods = subClass.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Where(m => m.IsDefined(typeof(SMTestAttribute)));
                #region 静态测试
                var isFirstStaticMethod = true;
                foreach (var method in singleRunMethods)
                {
                    var attributes = method.GetCustomAttributes(typeof(SMTestAttribute));
                    foreach (var likeAttribute in attributes)
                    {
                        if (likeAttribute is SMTestAttribute attribute)
                        {
                            string timerName = "方法名：" + method.Name;
                            if (likeAttribute is SMNTestAttribute subAttribute)
                            {
                                timerName = "标识名：" + subAttribute.TimerName;
                            }
                            if (attribute is SMNRTestAttribute resultAttribute)
                            {
                                timerName = "标识名：" + resultAttribute.TimerName;
                            }
                            object[] paramsForMethod = null;
                            if (attribute.Params != null)
                            {
                                paramsForMethod = attribute.Params;
                            }
                            else
                            {
                                paramsForMethod = method.GetDefaultParameters();
                            }
                            var timeCounter = new Stopwatch();
                            var selfTimeCounter = new Stopwatch();
                            var elapsedTime = new TimeSpan();
                            var isSuccessful = true;
                            var isResultEqual = true;
                            var failedMessage = string.Empty;
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = methodBorderColor;
                            if (isFirstStaticMethod)
                            {
                                isFirstStaticMethod = false;
                            }
                            else
                            {
                                Console.WriteLine("\n\n\n\n\n");
                            }
                            var borderCount = (Console.BufferWidth - timerName.DisplayLength()) / 2;
                            if (timerName != null)
                            {
                                for (int i = 0; i < borderCount; i++)
                                {
                                    Console.Write("=");
                                }
                                if ((Console.BufferWidth - timerName.DisplayLength()) % 2 == 1)
                                    Console.Write("=");
                                Console.ForegroundColor = mainColor;
                                Console.Write(timerName.Substring(0, 4));
                                Console.ForegroundColor = methodColor;
                                Console.Write(timerName.Substring(4));
                                Console.ForegroundColor = methodBorderColor;
                                for (int i = 0; i < borderCount; i++)
                                {
                                    Console.Write("=");
                                }
                            }
                            else
                            {
                                for (int i = 0; i < Console.BufferWidth; i++)
                                {
                                    Console.Write("=");
                                }
                            }
                            Console.WriteLine("\n");
                            Console.BackgroundColor = mainColor;
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.Write("开始执行");
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = mainColor;
                            Console.Write($"\t方法：");
                            Console.ForegroundColor = methodColor;
                            Console.Write(method.Name);
                            Console.ForegroundColor = mainColor;
                            Console.Write($"\t类：");
                            Console.ForegroundColor = classColor;
                            Console.WriteLine($"{subClass.Name}\n");
                            Console.ResetColor();
                            selfTimeCounter.Start();
                            selfTimeCounter.Stop();
                            try
                            {
                                CurrentMethodIsAsserted = false;
                                if (method.ReturnType == typeof(Task))
                                {
                                    timeCounter.Start();
                                    await (Task)method.Invoke(Activator.CreateInstance(subClass), paramsForMethod);
                                    timeCounter.Stop();
                                }
                                else
                                {
                                    timeCounter.Start();
                                    var result = method.Invoke(Activator.CreateInstance(subClass), paramsForMethod);
                                    timeCounter.Stop();
                                    if (attribute is SMRTestAttribute resultAttribute2)
                                    {
                                        isResultEqual = result.Equals(resultAttribute2.ReturnValue);
                                        if (!isResultEqual)
                                        {
                                            isSuccessful = false;
                                            failedMessage = $"预期结果：{resultAttribute2.ReturnValue}\t实际结果：{result}";
                                        }
                                    }
                                }
                                if (CurrentMethodIsAsserted)
                                {
                                    isSuccessful = false;
                                    if (failedMessage == string.Empty)
                                        failedMessage = CurrentAssertMessage;
                                    else
                                        failedMessage += $"\n{CurrentAssertMessage}";
                                    CurrentMethodIsAsserted = false;
                                    CurrentAssertMessage = string.Empty;
                                }
                            }
                            catch (Exception e)
                            {
                                isSuccessful = false;
                                failedMessage = e.Message;
                            }
                            elapsedTime = timeCounter.Elapsed - selfTimeCounter.Elapsed;
                            CTimeCounter++;
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.WriteLine();
                            Console.BackgroundColor = mainColor;
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.Write("执行完成");
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = mainColor;
                            Console.Write("\t执行批次：");
                            Console.ForegroundColor = counterColor;
                            Console.Write(CTimeCounter);
                            Console.ForegroundColor = mainColor;
                            Console.Write("\t执行时间：");
                            Console.ForegroundColor = timeColor;
                            if (elapsedTime.TotalHours >= 1)
                                Console.WriteLine(elapsedTime);
                            else if (elapsedTime.TotalMinutes >= 1)
                                Console.WriteLine($"{elapsedTime.Minutes} 分 {elapsedTime.Seconds} 秒 {elapsedTime.Milliseconds} 毫秒");
                            else if (elapsedTime.TotalSeconds >= 1)
                                Console.WriteLine($"{elapsedTime.TotalSeconds:F3} 秒");
                            else if (elapsedTime.TotalMilliseconds >= 1)
                                Console.WriteLine($"{elapsedTime.TotalMilliseconds:F3} 毫秒");
                            else
                                Console.WriteLine($"{elapsedTime.TotalMilliseconds * 1000:F2} 纳秒");
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.WriteLine();
                            if (isSuccessful && isResultEqual)
                            {
                                Console.BackgroundColor = successColor;
                                Console.Write("测试通过");
                            }
                            else
                            {
                                Console.BackgroundColor = failColor;
                                Console.Write("测试失败");
                                Console.BackgroundColor = ConsoleColor.Black;
                                Console.ForegroundColor = failColor;
                                Console.WriteLine($"\t失败原因：{failedMessage}");
                            }
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.Write("\n");
                            Console.ForegroundColor = methodBorderColor;
                            Console.Write("\n");
                            for (int i = 0; i < Console.BufferWidth; i++)
                            {
                                Console.Write("=");
                            }
                            Console.ResetColor();
                            Console.WriteLine("\n");
                            Console.WriteLine("\n");
                        }
                    }
                }
                #endregion
                var classAttributes = subClass.GetCustomAttributes(typeof(CTestAttribute));
                var classRunMethods = subClass.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Where(m => m.IsDefined(typeof(MTestAttribute)));
                foreach (var classAttribute in classAttributes.Cast<CTestAttribute>())
                {
                    var isFirstMethod = true;
                    var failedList = new List<MethodAndCounter>();
                    object classInstance = classAttribute.Params == null;
                    if (classAttribute.Params == null)
                    {
                        classInstance = subClass.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null).Invoke(classAttribute.Params);
                    }
                    else
                    {
                        classInstance = subClass.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, classAttribute.Params.GetTypes(), null).Invoke(classAttribute.Params);
                    }
                    var setUpAttribute = subClass.GetAttribute<TSetUpAttribute>();
                    if (setUpAttribute != null)
                    {
                        var setUpMethod = subClass.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Where(m => m.IsDefined(typeof(TSetUpAttribute))).FirstOrDefault();
                        if (setUpMethod != null)
                        {
                            setUpMethod.Invoke(classInstance, setUpMethod.GetDefaultParameters());
                        }
                    }
                    string classOtherName = "类名：" + subClass.Name;
                    if (classAttribute is CNTestAttribute subClassAttribute)
                    {
                        classOtherName = "标识名：" + subClassAttribute.ClassOtherName;
                    }
                    var classTimeCounter = new Stopwatch();
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = classBorderColor;
                    if (isFirstClass)
                    {
                        isFirstClass = false;
                    }
                    else
                    {
                        Console.WriteLine("\n\n\n\n\n\n\n\n");
                    }
                    var borderCount = (Console.BufferWidth - classOtherName.DisplayLength()) / 2;
                    if (classOtherName != null)
                    {
                        for (int i = 0; i < borderCount; i++)
                        {
                            Console.Write("=");
                        }
                        if ((Console.BufferWidth - classOtherName.DisplayLength()) % 2 == 1)
                            Console.Write("=");
                        Console.ForegroundColor = mainColor;
                        if (classOtherName.Substring(0, 4) == "标识名：")
                        {
                            Console.Write(classOtherName.Substring(0, 4));
                            Console.ForegroundColor = classColor;
                            Console.Write(classOtherName.Substring(4));
                        }
                        else
                        {
                            Console.Write(classOtherName.Substring(0, 3));
                            Console.ForegroundColor = classColor;
                            Console.Write(classOtherName.Substring(3));
                        }
                        Console.ForegroundColor = classBorderColor;
                        for (int i = 0; i < borderCount; i++)
                        {
                            Console.Write("=");
                        }
                    }
                    else
                    {
                        for (int i = 0; i < Console.BufferWidth; i++)
                        {
                            Console.Write("=");
                        }
                    }
                    Console.WriteLine("\n");
                    Console.BackgroundColor = mainColor;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write("开始执行");
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = mainColor;
                    Console.Write("\t类名：");
                    Console.ForegroundColor = classColor;
                    Console.WriteLine($"{subClass.Name}\n\n\n\n\n");
                    Console.ResetColor();
                    classTimeCounter.Start();
                    foreach (var method in classRunMethods)
                    {
                        var attributes = method.GetCustomAttributes(typeof(MTestAttribute));
                        foreach (MTestAttribute attribute in attributes)
                        {
                            if (attribute is MTestAttribute methodAttribute)
                            {
                                string methodOtherName = "方法名：" + method.Name;
                                if (attribute is MNTestAttribute subAttribute)
                                {
                                    methodOtherName = "标识名：" + subAttribute.MethodOtherName;
                                }
                                if (attribute is MNRTestAttribute resultAttribute)
                                {
                                    methodOtherName = "标识名：" + resultAttribute.MethodOtherName;
                                }
                                object[] paramsForMethod = null;
                                if (attribute.Params != null)
                                {
                                    paramsForMethod = attribute.Params;
                                }
                                else
                                {
                                    paramsForMethod = method.GetDefaultParameters();
                                }
                                var timeCounter = new Stopwatch();
                                var selfTimeCounter = new Stopwatch();
                                var elapsedTime = new TimeSpan();
                                var isSuccessful = true;
                                var isResultEqual = true;
                                var failedMessage = string.Empty;
                                Console.BackgroundColor = ConsoleColor.Black;
                                Console.ForegroundColor = methodBorderColor;
                                if (isFirstMethod)
                                {
                                    isFirstMethod = false;
                                }
                                else
                                {
                                    Console.WriteLine("\n\n\n\n\n");
                                }
                                borderCount = (Console.BufferWidth - methodOtherName.DisplayLength()) / 2;
                                if (methodOtherName != null)
                                {
                                    for (int i = 0; i < borderCount; i++)
                                    {
                                        Console.Write("=");
                                    }
                                    if ((Console.BufferWidth - methodOtherName.DisplayLength()) % 2 == 1)
                                        Console.Write("=");
                                    Console.ForegroundColor = mainColor;
                                    Console.Write(methodOtherName.Substring(0, 4));
                                    Console.ForegroundColor = methodColor;
                                    Console.Write(methodOtherName.Substring(4));
                                    Console.ForegroundColor = methodBorderColor;
                                    for (int i = 0; i < borderCount; i++)
                                    {
                                        Console.Write("=");
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < Console.BufferWidth; i++)
                                    {
                                        Console.Write("=");
                                    }
                                }
                                Console.WriteLine("\n");
                                Console.BackgroundColor = mainColor;
                                Console.ForegroundColor = ConsoleColor.Black;
                                Console.Write("开始执行");
                                Console.BackgroundColor = ConsoleColor.Black;
                                Console.ForegroundColor = mainColor;
                                Console.Write($"\t方法：");
                                Console.ForegroundColor = methodColor;
                                Console.Write(method.Name);
                                Console.ForegroundColor = mainColor;
                                Console.Write($"\t类：");
                                Console.ForegroundColor = classColor;
                                Console.WriteLine($"{subClass.Name}\n");
                                Console.ResetColor();
                                selfTimeCounter.Start();
                                selfTimeCounter.Stop();
                                try
                                {
                                    CurrentMethodIsAsserted = false;
                                    if (method.ReturnType == typeof(Task))
                                    {
                                        timeCounter.Start();
                                        await (Task)method.Invoke(classInstance, paramsForMethod);
                                        timeCounter.Stop();
                                    }
                                    else
                                    {
                                        timeCounter.Start();
                                        var result = method.Invoke(classInstance, paramsForMethod);
                                        timeCounter.Stop();
                                        if (methodAttribute is MRTestAttribute resultAttribute2)
                                        {
                                            isResultEqual = result.Equals(resultAttribute2.ReturnValue);
                                            if (!isResultEqual)
                                            {
                                                isSuccessful = false;
                                                failedMessage = $"预期结果：{resultAttribute2.ReturnValue}\t实际结果：{result}";
                                            }
                                        }
                                    }
                                    if (CurrentMethodIsAsserted)
                                    {
                                        isSuccessful = false;
                                        if (failedMessage == string.Empty)
                                            failedMessage = CurrentAssertMessage;
                                        else
                                            failedMessage += $"\n{CurrentAssertMessage}";
                                        CurrentMethodIsAsserted = false;
                                        CurrentAssertMessage = string.Empty;
                                    }
                                }
                                catch (Exception e)
                                {
                                    isSuccessful = false;
                                    failedMessage = e.Message;
                                }
                                elapsedTime = timeCounter.Elapsed - selfTimeCounter.Elapsed;
                                CTimeCounter++;
                                Console.BackgroundColor = ConsoleColor.Black;
                                Console.WriteLine();
                                Console.BackgroundColor = mainColor;
                                Console.ForegroundColor = ConsoleColor.Black;
                                Console.Write("执行完成");
                                Console.BackgroundColor = ConsoleColor.Black;
                                Console.ForegroundColor = mainColor;
                                Console.Write("\t执行批次：");
                                Console.ForegroundColor = counterColor;
                                Console.Write(CTimeCounter);
                                Console.ForegroundColor = mainColor;
                                Console.Write("\t执行时间：");
                                Console.ForegroundColor = timeColor;
                                if (elapsedTime.TotalHours >= 1)
                                    Console.WriteLine(elapsedTime);
                                else if (elapsedTime.TotalMinutes >= 1)
                                    Console.WriteLine($"{elapsedTime.Minutes} 分 {elapsedTime.Seconds} 秒 {elapsedTime.Milliseconds} 毫秒");
                                else if (elapsedTime.TotalSeconds >= 1)
                                    Console.WriteLine($"{elapsedTime.TotalSeconds:F3} 秒");
                                else if (elapsedTime.TotalMilliseconds >= 1)
                                    Console.WriteLine($"{elapsedTime.TotalMilliseconds:F3} 毫秒");
                                else
                                    Console.WriteLine($"{elapsedTime.TotalMilliseconds * 1000:F2} 纳秒");
                                Console.ForegroundColor = ConsoleColor.Black;
                                Console.WriteLine();
                                if (isSuccessful && isResultEqual)
                                {
                                    Console.BackgroundColor = successColor;
                                    Console.Write("测试通过");
                                }
                                else
                                {
                                    Console.BackgroundColor = failColor;
                                    Console.Write("测试失败");
                                    Console.BackgroundColor = ConsoleColor.Black;
                                    Console.ForegroundColor = failColor;
                                    Console.WriteLine($"\t失败原因：{failedMessage}");
                                    failedList.Add(new MethodAndCounter(method, CTimeCounter, failedMessage));
                                }
                                Console.BackgroundColor = ConsoleColor.Black;
                                Console.Write("\n");
                                Console.ForegroundColor = methodBorderColor;
                                Console.Write("\n");
                                for (int i = 0; i < Console.BufferWidth; i++)
                                {
                                    Console.Write("=");
                                }
                                Console.ResetColor();
                            }
                        }
                    }
                    classTimeCounter.Stop();
                    CTimeCounter++;
                    Console.WriteLine("\n\n\n\n\n");
                    Console.BackgroundColor = mainColor;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write("执行完成");
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = mainColor;
                    Console.Write("\t执行批次：");
                    Console.ForegroundColor = counterColor;
                    Console.Write(CTimeCounter);
                    Console.ForegroundColor = mainColor;
                    Console.Write("\t执行时间：");
                    Console.ForegroundColor = timeColor;
                    if (classTimeCounter.Elapsed.TotalHours >= 1)
                        Console.WriteLine(classTimeCounter.Elapsed);
                    else if (classTimeCounter.Elapsed.TotalMinutes >= 1)
                        Console.WriteLine($"{classTimeCounter.Elapsed.Minutes} 分 {classTimeCounter.Elapsed.Seconds} 秒 {classTimeCounter.Elapsed.Milliseconds} 毫秒");
                    else if (classTimeCounter.Elapsed.TotalSeconds >= 1)
                        Console.WriteLine($"{classTimeCounter.Elapsed.TotalSeconds:F3} 秒");
                    else if (classTimeCounter.Elapsed.TotalMilliseconds >= 1)
                        Console.WriteLine($"{classTimeCounter.Elapsed.TotalMilliseconds:F3} 毫秒");
                    else
                        Console.WriteLine($"{classTimeCounter.Elapsed.TotalMilliseconds * 1000:F2} 纳秒");
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.WriteLine();
                    if (failedList.Count == 0)
                    {
                        Console.BackgroundColor = successColor;
                        Console.Write("测试通过");
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.WriteLine();
                    }
                    else
                    {
                        Console.BackgroundColor = failColor;
                        Console.Write("测试失败");
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = failColor;
                        Console.WriteLine("\t未通过的方法：");
                        foreach (var failedMethod in failedList)
                        {
                            Console.ForegroundColor = mainColor;
                            Console.Write("\t\t方法名：");
                            Console.ForegroundColor = methodColor;
                            Console.Write(failedMethod.Method.Name);
                            Console.ForegroundColor = mainColor;
                            Console.Write("\t执行批次：");
                            Console.ForegroundColor = counterColor;
                            Console.Write(failedMethod.Counter);
                            Console.ForegroundColor = mainColor;
                            Console.Write("\t失败原因：");
                            Console.ForegroundColor = failColor;
                            Console.WriteLine(failedMethod.FailMessage);
                        }
                    }
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = classBorderColor;
                    Console.Write("\n");
                    for (int i = 0; i < Console.BufferWidth; i++)
                    {
                        Console.Write("=");
                    }
                    Console.ResetColor();
                }
            }
        }
        /// <summary>
        /// 断言：判断条件为真
        /// </summary>
        /// <param name="condition">判断条件</param>
        protected static bool Assert(bool condition)
        {
            if (!condition)
            {
                CurrentMethodIsAsserted = true;
                CurrentAssertMessage = "断言失败：期望为真，实际为假";
                return false;
            }
            return true;
        }

        /// <summary>
        /// 断言：判断条件为真
        /// </summary>
        /// <param name="condition">判断条件</param>
        /// <param name="message">消息</param>
        protected static bool Assert(bool condition, string message)
        {
            if (!condition)
            {
                CurrentMethodIsAsserted = true;
                CurrentAssertMessage = message;
                return false;
            }
            return true;
        }

        /// <summary>
        /// 断言：判断条件为假
        /// </summary>
        /// <param name="condition">判断条件</param>
        protected static bool AssertF(bool condition)
        {
            if (condition)
            {
                CurrentMethodIsAsserted = true;
                CurrentAssertMessage = "断言失败：期望为假，实际为真";
                return false;
            }
            return true;
        }

        /// <summary>
        /// 断言：判断条件为假
        /// </summary>
        /// <param name="condition">判断条件</param>
        /// <param name="message">消息</param>
        protected static bool AssertF(bool condition, string message)
        {
            if (condition)
            {
                CurrentMethodIsAsserted = true;
                CurrentAssertMessage = message;
                return false;
            }
            return true;
        }

        /// <summary>
        /// 断言：判断两个值是否相等
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="expected">期望值</param>
        /// <param name="actual">实际值</param>
        protected static bool AssertE<T>(T expected, T actual)
        {
            var result = !EqualityComparer<T>.Default.Equals(expected, actual);
            if (result)
            {
                CurrentMethodIsAsserted = true;
                CurrentAssertMessage = $"断言失败：期望 {expected}，实际 {actual}";
                return false;
            }
            return true;
        }

        /// <summary>
        /// 断言：判断两个值是否相等
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="expected">期望值</param>
        /// <param name="actual">实际值</param>
        /// <param name="message">消息</param>
        protected static bool AssertE<T>(T expected, T actual, string message)
        {
            var result = !EqualityComparer<T>.Default.Equals(expected, actual);
            if (result)
            {
                CurrentMethodIsAsserted = true;
                CurrentAssertMessage = message;
                return false;
            }
            return true;
        }

        static XFECode()
        {
            new Action(async () => await RunTest()).StartNewTask();
        }
    }
    #region 特性
    /// <summary>
    /// 测试颜色主题
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class TestThemeAttribute : Attribute
    {
        /// <summary>
        /// 主色
        /// </summary>
        public ConsoleColor MainColor { get; set; }
        /// <summary>
        /// 类颜色
        /// </summary>
        public ConsoleColor ClassColor { get; set; }
        /// <summary>
        /// 类边框颜色
        /// </summary>
        public ConsoleColor ClassBorderColor { get; set; }
        /// <summary>
        /// 方法颜色
        /// </summary>
        public ConsoleColor MethodColor { get; set; }
        /// <summary>
        /// 方法边框颜色
        /// </summary>
        public ConsoleColor MethodBorderColor { get; set; }
        /// <summary>
        /// 成功颜色
        /// </summary>
        public ConsoleColor SuccessColor { get; set; }
        /// <summary>
        /// 失败颜色
        /// </summary>
        public ConsoleColor FailColor { get; set; }
        /// <summary>
        /// 时间颜色
        /// </summary>
        public ConsoleColor TimeColor { get; set; }
        /// <summary>
        /// 计数器颜色
        /// </summary>
        public ConsoleColor CounterColor { get; set; }
        /// <summary>
        /// 测试颜色主题
        /// </summary>
        /// <param name="mainColor">主色</param>
        /// <param name="classColor">类颜色</param>
        /// <param name="classBorderColor">类边框颜色</param>
        /// <param name="methodColor">方法颜色</param>
        /// <param name="methodBorderColor">方法边框颜色</param>
        /// <param name="successColor">成功颜色</param>
        /// <param name="failColor">失败颜色</param>
        /// <param name="timeColor">时间颜色</param>
        /// <param name="counterColor">计数器颜色</param>
        public TestThemeAttribute(ConsoleColor mainColor = ConsoleColor.Blue, ConsoleColor classColor = ConsoleColor.Green, ConsoleColor classBorderColor = ConsoleColor.DarkGreen, ConsoleColor methodColor = ConsoleColor.Yellow, ConsoleColor methodBorderColor = ConsoleColor.DarkYellow, ConsoleColor successColor = ConsoleColor.Green, ConsoleColor failColor = ConsoleColor.Red, ConsoleColor timeColor = ConsoleColor.Cyan, ConsoleColor counterColor = ConsoleColor.Gray)
        {
            MainColor = mainColor;
            ClassColor = classColor;
            ClassBorderColor = classBorderColor;
            MethodColor = methodColor;
            MethodBorderColor = methodBorderColor;
            SuccessColor = successColor;
            FailColor = failColor;
            TimeColor = timeColor;
            CounterColor = counterColor;
        }
    }
    /// <summary>
    /// 测试特性基类
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class XFETestAttributeBase : Attribute
    {
        /// <summary>
        /// 参数
        /// </summary>
        public object[] Params { get; set; }
    }
    /// <summary>
    /// 用于计时的特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SMTestAttribute : XFETestAttributeBase
    {
        /// <summary>
        /// 计算执行一段代码所需时间
        /// </summary>
        /// <param name="values">传参</param>
        public SMTestAttribute(params object[] values)
        {
            Params = values;
        }
        /// <summary>
        /// 计算执行一段代码所需时间
        /// </summary>
        public SMTestAttribute() { }
    }
    /// <summary>
    /// 用于计时的特性，可自定义名称
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class SMNTestAttribute : SMTestAttribute
    {
        /// <summary>
        /// 该次计时标识名
        /// </summary>
        public string TimerName { get; set; }
        /// <summary>
        /// 计算执行一段代码所需时间
        /// </summary>
        /// <param name="timerName">计时器名称</param>
        /// <param name="values">传参</param>
        public SMNTestAttribute(string timerName, params object[] values)
        {
            TimerName = timerName;
            Params = values;
        }
        /// <summary>
        /// 计算执行一段代码所需时间
        /// </summary>
        public SMNTestAttribute(string timerName)
        {
            TimerName = timerName;
        }
    }
    /// <summary>
    /// 用于计时的特性，可绑定返回值
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SMRTestAttribute : SMTestAttribute
    {
        /// <summary>
        /// 返回值
        /// </summary>
        public object ReturnValue { get; set; }
        /// <summary>
        /// 计算执行一段代码所需时间
        /// </summary>
        /// <param name="valuesAndResult">传参</param>
        public SMRTestAttribute(params object[] valuesAndResult)
        {
            ReturnValue = valuesAndResult[valuesAndResult.Length - 1];
            Params = valuesAndResult.Take(valuesAndResult.Length - 1).ToArray();
        }
        /// <summary>
        /// 计算执行一段代码所需时间
        /// </summary>
        public SMRTestAttribute() { }
    }
    /// <summary>
    /// 用于计时的特性，可自定义名称和返回值
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class SMNRTestAttribute : SMRTestAttribute
    {
        /// <summary>
        /// 该次计时标识名
        /// </summary>
        public string TimerName { get; set; }
        /// <summary>
        /// 计算执行一段代码所需时间
        /// </summary>
        /// <param name="timerName">计时器名称</param>
        /// <param name="valuesAndResult">传参</param>
        public SMNRTestAttribute(string timerName, params object[] valuesAndResult)
        {
            TimerName = timerName;
            ReturnValue = valuesAndResult[valuesAndResult.Length - 1];
            Params = valuesAndResult.Take(valuesAndResult.Length - 1).ToArray();
        }
        /// <summary>
        /// 计算执行一段代码所需时间
        /// </summary>
        public SMNRTestAttribute() { }
    }
    /// <summary>
    /// 方法测试
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class MTestAttribute : XFETestAttributeBase
    {
        /// <summary>
        /// 方法测试
        /// </summary>
        /// <param name="values">传参</param>
        public MTestAttribute(params object[] values)
        {
            Params = values;
        }
        /// <summary>
        /// 方法测试
        /// </summary>
        public MTestAttribute() { }
    }
    /// <summary>
    /// 方法测试，可自定义名称
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class MNTestAttribute : MTestAttribute
    {
        /// <summary>
        /// 方法别名
        /// </summary>
        public string MethodOtherName { get; set; }
        /// <summary>
        /// 方法测试
        /// </summary>
        /// <param name="methodOtherName">方法别名</param>
        /// <param name="values">传参</param>
        public MNTestAttribute(string methodOtherName, params object[] values)
        {
            MethodOtherName = methodOtherName;
            Params = values;
        }
        /// <summary>
        /// 方法测试
        /// </summary>
        /// <param name="methodOtherName">方法别名</param>
        public MNTestAttribute(string methodOtherName)
        {
            MethodOtherName = methodOtherName;
        }
        /// <summary>
        /// 方法测试
        /// </summary>
        public MNTestAttribute() { }
    }
    /// <summary>
    /// 方法测试，可自定义返回值
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class MRTestAttribute : MTestAttribute
    {
        /// <summary>
        /// 返回值
        /// </summary>
        public object ReturnValue { get; set; }
        /// <summary>
        /// 方法测试
        /// </summary>
        /// <param name="valuesAndResult">传参</param>
        public MRTestAttribute(params object[] valuesAndResult)
        {
            ReturnValue = valuesAndResult[valuesAndResult.Length - 1];
            Params = valuesAndResult.Take(valuesAndResult.Length - 1).ToArray();
        }
        /// <summary>
        /// 方法测试
        /// </summary>
        public MRTestAttribute() { }
    }
    /// <summary>
    /// 方法测试，可自定义名称和返回值
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class MNRTestAttribute : MRTestAttribute
    {
        /// <summary>
        /// 方法别名
        /// </summary>
        public string MethodOtherName { get; set; }
        /// <summary>
        /// 方法测试
        /// </summary>
        /// <param name="methodOtherName">方法别名</param>
        /// <param name="valuesAndResult">传参</param>
        public MNRTestAttribute(string methodOtherName, params object[] valuesAndResult)
        {
            MethodOtherName = methodOtherName;
            ReturnValue = valuesAndResult[valuesAndResult.Length - 1];
            Params = valuesAndResult.Take(valuesAndResult.Length - 1).ToArray();
        }
        /// <summary>
        /// 方法测试
        /// </summary>
        public MNRTestAttribute() { }
    }
    /// <summary>
    /// 类测试
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CTestAttribute : XFETestAttributeBase
    {
        /// <summary>
        /// 类测试
        /// </summary>
        /// <param name="values">创建类时构造方法的传参</param>
        public CTestAttribute(params object[] values)
        {
            Params = values;
        }
        /// <summary>
        /// 类测试
        /// </summary>
        public CTestAttribute() { }
    }
    /// <summary>
    /// 类测试
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class CNTestAttribute : CTestAttribute
    {
        /// <summary>
        /// 类别名
        /// </summary>
        public string ClassOtherName { get; set; }
        /// <summary>
        /// 类测试
        /// </summary>
        /// <param name="values">创建类时构造方法的传参</param>
        /// <param name="classOtherName">类别名</param>
        public CNTestAttribute(string classOtherName, params object[] values)
        {
            Params = values;
            ClassOtherName = classOtherName;
        }
    }
    /// <summary>
    /// 用于标记测试类初始函数的特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class TSetUpAttribute : XFETestAttributeBase
    {
        /// <summary>
        /// 用于标记测试类构造函数的特性
        /// </summary>
        /// <param name="values"></param>
        public TSetUpAttribute(params object[] values)
        {
            Params = values;
        }
    }
    #endregion
    /// <summary>
    /// 对代码整体书写习惯的拓展
    /// </summary>
    public static class XFEExtension
    {
        /// <summary>
        /// 使得计时器可以直接调用（单位：秒）
        /// </summary>
        /// <param name="waitTime"></param>
        /// <returns></returns>
        public static TaskAwaiter GetAwaiter(this int waitTime)
            => Task.Delay(TimeSpan.FromSeconds(waitTime)).GetAwaiter();
        /// <summary>
        /// 使得计时器可以直接调用（单位：秒）
        /// </summary>
        /// <param name="waitTime"></param>
        /// <returns></returns>
        public static TaskAwaiter GetAwaiter(this double waitTime)
            => Task.Delay(TimeSpan.FromSeconds(waitTime)).GetAwaiter();
    }
    class MethodAndCounter
    {
        public MethodInfo Method { get; set; }
        public int Counter { get; set; }
        public string FailMessage { get; set; }
        public MethodAndCounter(MethodInfo method, int counter, string failMessage)
        {
            Method = method;
            Counter = counter;
            FailMessage = failMessage;
        }
    }
}