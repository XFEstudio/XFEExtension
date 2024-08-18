using System.Diagnostics;
using System.Reflection;
using XFEExtension.NetCore.ArrayExtension;
using XFEExtension.NetCore.AttributeExtension;
using XFEExtension.NetCore.ReflectionExtension;
using XFEExtension.NetCore.StringExtension;
using XFEExtension.NetCore.TaskExtension;

namespace XFEExtension.NetCore.XUnit;

/// <summary>
/// XFEExtension的XUnit框架依赖
/// </summary>
public abstract class XFECode
{
    private static int cTimeCounter = 0;
    private static bool initialized = false;
    private static bool currentMethodIsAsserted = false;
    private static string currentAssertMessage = string.Empty;
    #region 暂停
    /// <summary>
    /// 暂停
    /// </summary>
    /// <param name="showText">是否显示提示</param>
    protected static void Pause(bool showText = true)
    {
        if (showText)
        {
            Console.WriteLine("按任意键继续...");
        }
        Console.ReadKey();
    }
    /// <summary>
    /// 暂停
    /// </summary>
    /// <param name="showText">显示的文字</param>
    protected static void Pause(string showText)
    {
        Console.WriteLine(showText);
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
    /// <param name="showText">显示的文字</param>
    protected static void Pause(ConsoleKey consoleKey, string showText)
    {
        Console.WriteLine(showText);
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
        cTimeCounter++;
        var elapsedTime = timeCounter.Elapsed;
        if (autoOutPut)
            if (elapsedTime.TotalHours >= 1)
                Console.WriteLine($"标识名：{timerName}\t执行批次：{cTimeCounter}\t执行时间：{elapsedTime}");
            else if (elapsedTime.TotalMinutes >= 1)
                Console.WriteLine($"标识名：{timerName}\t执行批次：{cTimeCounter}\t执行时间: {Math.Floor(elapsedTime.TotalMinutes)} 分 {elapsedTime.Seconds} 秒");
            else if (elapsedTime.TotalSeconds >= 1)
                Console.WriteLine($"标识名：{timerName}\t执行批次：{cTimeCounter}\t执行时间: {elapsedTime.TotalSeconds:F3} 秒");
            else if (elapsedTime.TotalMilliseconds >= 1)
                Console.WriteLine($"标识名：{timerName}\t执行批次：{cTimeCounter}\t执行时间: {elapsedTime.TotalMilliseconds:F3} 毫秒");
            else if (elapsedTime.TotalMicroseconds >= 1)
                Console.WriteLine($"标识名：{timerName}\t执行批次：{cTimeCounter}\t执行时间: {elapsedTime.TotalMicroseconds:F3} 微秒");
            else
                Console.WriteLine($"标识名：{timerName}\t执行批次：{cTimeCounter}\t执行时间: {elapsedTime.TotalNanoseconds:F3} 纳秒");
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
        await action?.Invoke()!;
        timeCounter.Stop();
        cTimeCounter++;
        var elapsedTime = timeCounter.Elapsed;
        if (autoOutPut)
            if (elapsedTime.TotalHours >= 1)
                Console.WriteLine($"标识名：{timerName}\t执行批次：{cTimeCounter}\t执行时间：{elapsedTime}");
            else if (elapsedTime.TotalMinutes >= 1)
                Console.WriteLine($"标识名：{timerName}\t执行批次：{cTimeCounter}\t执行时间: {Math.Floor(elapsedTime.TotalMinutes)} 分 {elapsedTime.Seconds} 秒");
            else if (elapsedTime.TotalSeconds >= 1)
                Console.WriteLine($"标识名：{timerName}\t执行批次：{cTimeCounter}\t执行时间: {elapsedTime.TotalSeconds:F3} 秒");
            else if (elapsedTime.TotalMilliseconds >= 1)
                Console.WriteLine($"标识名：{timerName}\t执行批次：{cTimeCounter}\t执行时间: {elapsedTime.TotalMilliseconds:F3} 毫秒");
            else if (elapsedTime.TotalMicroseconds >= 1)
                Console.WriteLine($"标识名：{timerName}\t执行批次：{cTimeCounter}\t执行时间: {elapsedTime.TotalMicroseconds:F3} 微秒");
            else
                Console.WriteLine($"标识名：{timerName}\t执行批次：{cTimeCounter}\t执行时间: {elapsedTime.TotalNanoseconds:F3} 纳秒");
        return elapsedTime;
    }
    /// <summary>
    /// 计算一段代码执行所需时间
    /// </summary>
    /// <returns></returns>
    /// <exception cref="XFEExtensionException"></exception>
    public static async Task RunTest()
    {
        if (initialized)
            return;
        initialized = true;
        var isFirstClass = true;
        //var subClasses = Assembly.GetEntryAssembly()?.GetTypes().Where(type => type.IsSubclassOf(typeof(XFECode)));
        var allClasses = Assembly.GetEntryAssembly()?.GetTypes();
        if (allClasses is not null)
            foreach (var subClass in allClasses)
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
                    var attributes = method.GetCustomAttributes<SMTestAttribute>();
                    bool useXFEConsole = false;
                    int consolePort = 3280;
                    foreach (var customAttribute in method.CustomAttributes)
                    {
                        if (customAttribute is not null && customAttribute.AttributeType.Name == "UseXFEConsoleAttribute")
                        {
                            useXFEConsole = true;
                            foreach (var arg in customAttribute.ConstructorArguments)
                            {
                                if (arg.ArgumentType == typeof(int))
                                    consolePort = (int)arg.Value!;
                            }
                        }
                    }
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
                            object?[]? paramsForMethod;
                            if (attribute.Params is not null)
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
                            if (timerName is not null)
                            {
                                for (int i = 0; i < borderCount; i++)
                                {
                                    Console.Write("=");
                                }
                                if ((Console.BufferWidth - timerName.DisplayLength()) % 2 == 1)
                                    Console.Write("=");
                                Console.ForegroundColor = mainColor;
                                Console.Write(timerName[..4]);
                                Console.ForegroundColor = methodColor;
                                Console.Write(timerName[4..]);
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
                            object? result = null;
                            try
                            {
                                if (useXFEConsole)
                                {
                                    if (Type.GetType("XFEExtension.NetCore.XFEConsole.XFEConsole, XFEExtension.NetCore.XFEConsole") is Type type)
                                    {
                                        if (type.GetMethod("UseXFEConsole", BindingFlags.Static | BindingFlags.Public, [typeof(int), typeof(string)]) is MethodInfo useXFEConsoleMethodInfo)
                                        {
                                            var connectSuccessful = await (Task<bool>)useXFEConsoleMethodInfo.Invoke(null, [consolePort, ""])!;
                                            if (!connectSuccessful)
                                                Console.WriteLine("XFE控制台连接失败！");
                                        }
                                    }
                                }
                                currentMethodIsAsserted = false;
                                if (method.ReturnType == typeof(Task))
                                {
                                    timeCounter.Start();
                                    await (Task)method.Invoke(Activator.CreateInstance(subClass), paramsForMethod)!;
                                    timeCounter.Stop();
                                }
                                else if (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                                {
                                    timeCounter.Start();
                                    dynamic dynamicResult = method.Invoke(Activator.CreateInstance(subClass), paramsForMethod)!;
                                    result = await dynamicResult;
                                    timeCounter.Stop();
                                }
                                else
                                {
                                    timeCounter.Start();
                                    result = method.Invoke(Activator.CreateInstance(subClass), paramsForMethod);
                                    timeCounter.Stop();
                                    if (attribute is SMRTestAttribute resultAttribute2)
                                    {
                                        isResultEqual = result is null && resultAttribute2.ReturnValue is null || result is not null && result.Equals(resultAttribute2.ReturnValue);
                                        if (!isResultEqual)
                                        {
                                            isSuccessful = false;
                                            failedMessage = $"预期结果：{resultAttribute2.ReturnValue}\t实际结果：{result}";
                                        }
                                    }
                                }
                                if (currentMethodIsAsserted)
                                {
                                    isSuccessful = false;
                                    if (failedMessage == string.Empty)
                                        failedMessage = currentAssertMessage;
                                    else
                                        failedMessage += $"\n{currentAssertMessage}";
                                    currentMethodIsAsserted = false;
                                    currentAssertMessage = string.Empty;
                                }
                            }
                            catch (Exception e)
                            {
                                isSuccessful = false;
                                failedMessage = e.Message;
                            }
                            elapsedTime = timeCounter.Elapsed - selfTimeCounter.Elapsed;
                            cTimeCounter++;
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.WriteLine();
                            Console.BackgroundColor = mainColor;
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.Write("执行完成");
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = mainColor;
                            Console.Write("\t执行批次：");
                            Console.ForegroundColor = counterColor;
                            Console.Write(cTimeCounter);
                            Console.ForegroundColor = mainColor;
                            Console.Write("\t执行时间：");
                            Console.ForegroundColor = timeColor;
                            if (elapsedTime.TotalHours >= 1)
                                Console.Write(elapsedTime);
                            else if (elapsedTime.TotalMinutes >= 1)
                                Console.Write($"{elapsedTime.Minutes} 分 {elapsedTime.Seconds} 秒 {elapsedTime.Milliseconds} 毫秒");
                            else if (elapsedTime.TotalSeconds >= 1)
                                Console.Write($"{elapsedTime.TotalSeconds:F3} 秒");
                            else if (elapsedTime.TotalMilliseconds >= 1)
                                Console.Write($"{elapsedTime.TotalMilliseconds:F3} 毫秒");
                            else if (elapsedTime.TotalMicroseconds >= 1)
                                Console.Write($"{elapsedTime.TotalMicroseconds:F2} 微秒");
                            else
                                Console.Write($"{elapsedTime.TotalMicroseconds:F2} 纳秒");
                            if (result is not null)
                            {
                                Console.ForegroundColor = mainColor;
                                Console.Write("\n\n执行结果：");
                                Console.ForegroundColor = ConsoleColor.Gray;
                                if (result.GetType().IsValueType)
                                    Console.WriteLine($"{result}\n");
                                else
                                    result.X(true, true, "执行结果");
                            }
                            Console.WriteLine();
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
                            if (useXFEConsole)
                            {
                                if (Type.GetType("XFEExtension.NetCore.XFEConsole.XFEConsole") is Type type && type.GetMethod("StopXFEConsole", BindingFlags.Static | BindingFlags.Public) is MethodInfo stopXFEConsoleMethodInfo)
                                {
                                    await (Task)stopXFEConsoleMethodInfo.Invoke(null, null)!;
                                }
                            }
                        }
                    }
                }
                #endregion
                var classAttributes = subClass.GetCustomAttributes<CTestAttribute>();
                var classRunMethods = subClass.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Where(m => m.IsDefined(typeof(MTestAttribute)));
                foreach (var classAttribute in classAttributes.Cast<CTestAttribute>())
                {
                    var isFirstMethod = true;
                    var failedList = new List<MethodAndCounter>();
                    object? classInstance;
                    if (classAttribute.Params is null)
                    {
                        classInstance = subClass.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, [], null)?.Invoke(classAttribute.Params);
                    }
                    else
                    {
                        classInstance = subClass.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, classAttribute.Params.GetTypes()!, null)?.Invoke(classAttribute.Params);
                    }
                    var setUpAttribute = subClass.GetAttribute<SetUpAttribute>();
                    if (setUpAttribute is not null)
                    {
                        var setUpMethod = subClass.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Where(m => m.IsDefined(typeof(SetUpAttribute))).FirstOrDefault();
                        setUpMethod?.Invoke(classInstance, setUpMethod.GetDefaultParameters());
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
                    if (classOtherName is not null)
                    {
                        for (int i = 0; i < borderCount; i++)
                        {
                            Console.Write("=");
                        }
                        if ((Console.BufferWidth - classOtherName.DisplayLength()) % 2 == 1)
                            Console.Write("=");
                        Console.ForegroundColor = mainColor;
                        if (classOtherName[..4] == "标识名：")
                        {
                            Console.Write(classOtherName[..4]);
                            Console.ForegroundColor = classColor;
                            Console.Write(classOtherName[4..]);
                        }
                        else
                        {
                            Console.Write(classOtherName[..3]);
                            Console.ForegroundColor = classColor;
                            Console.Write(classOtherName[3..]);
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
                        var attributes = method.GetCustomAttributes<MTestAttribute>();
                        bool useXFEConsole = false;
                        int consolePort = 3280;
                        foreach (var customAttribute in method.CustomAttributes)
                        {
                            if (customAttribute is not null && customAttribute.AttributeType.Name == "UseXFEConsoleAttribute")
                            {
                                useXFEConsole = true;
                                foreach (var arg in customAttribute.ConstructorArguments)
                                {
                                    if (arg.ArgumentType == typeof(int))
                                        consolePort = (int)arg.Value!;
                                }
                            }
                        }
                        foreach (MTestAttribute attribute in attributes.Cast<MTestAttribute>())
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
                                object?[]? paramsForMethod;
                                if (attribute.Params is not null)
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
                                if (methodOtherName is not null)
                                {
                                    for (int i = 0; i < borderCount; i++)
                                    {
                                        Console.Write("=");
                                    }
                                    if ((Console.BufferWidth - methodOtherName.DisplayLength()) % 2 == 1)
                                        Console.Write("=");
                                    Console.ForegroundColor = mainColor;
                                    Console.Write(methodOtherName[..4]);
                                    Console.ForegroundColor = methodColor;
                                    Console.Write(methodOtherName[4..]);
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
                                object? result = null;
                                try
                                {
                                    currentMethodIsAsserted = false;
                                    if (useXFEConsole)
                                    {
                                        if (Type.GetType("XFEExtension.NetCore.XFEConsole.XFEConsole, XFEExtension.NetCore.XFEConsole") is Type type)
                                        {
                                            if (type.GetMethod("UseXFEConsole", BindingFlags.Static | BindingFlags.Public, [typeof(int), typeof(string)]) is MethodInfo useXFEConsoleMethodInfo)
                                            {
                                                var connectSuccessful = await (Task<bool>)useXFEConsoleMethodInfo.Invoke(null, [consolePort, ""])!;
                                                if (!connectSuccessful)
                                                    Console.WriteLine("XFE控制台连接失败！");
                                            }
                                        }
                                    }
                                    if (method.ReturnType == typeof(Task))
                                    {
                                        timeCounter.Start();
                                        await (Task)method.Invoke(classInstance, paramsForMethod)!;
                                        timeCounter.Stop();
                                    }
                                    else if (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                                    {
                                        timeCounter.Start();
                                        dynamic dynamicResult = method.Invoke(classInstance, paramsForMethod)!;
                                        result = await dynamicResult;
                                        timeCounter.Stop();
                                    }
                                    else
                                    {
                                        timeCounter.Start();
                                        result = method.Invoke(classInstance, paramsForMethod);
                                        timeCounter.Stop();
                                        if (methodAttribute is MRTestAttribute resultAttribute2)
                                        {
                                            isResultEqual = result is null && resultAttribute2.ReturnValue is null || result is not null && result.Equals(resultAttribute2.ReturnValue);
                                            if (!isResultEqual)
                                            {
                                                isSuccessful = false;
                                                failedMessage = $"预期结果：{resultAttribute2.ReturnValue}\t实际结果：{result}";
                                            }
                                        }
                                    }
                                    if (currentMethodIsAsserted)
                                    {
                                        isSuccessful = false;
                                        if (failedMessage == string.Empty)
                                            failedMessage = currentAssertMessage;
                                        else
                                            failedMessage += $"\n{currentAssertMessage}";
                                        currentMethodIsAsserted = false;
                                        currentAssertMessage = string.Empty;
                                    }
                                }
                                catch (Exception e)
                                {
                                    isSuccessful = false;
                                    failedMessage = e.Message;
                                }
                                elapsedTime = timeCounter.Elapsed - selfTimeCounter.Elapsed;
                                cTimeCounter++;
                                Console.BackgroundColor = ConsoleColor.Black;
                                Console.WriteLine();
                                Console.BackgroundColor = mainColor;
                                Console.ForegroundColor = ConsoleColor.Black;
                                Console.Write("执行完成");
                                Console.BackgroundColor = ConsoleColor.Black;
                                Console.ForegroundColor = mainColor;
                                Console.Write("\t执行批次：");
                                Console.ForegroundColor = counterColor;
                                Console.Write(cTimeCounter);
                                Console.ForegroundColor = mainColor;
                                Console.Write("\t执行时间：");
                                Console.ForegroundColor = timeColor;
                                if (elapsedTime.TotalHours >= 1)
                                    Console.Write(elapsedTime);
                                else if (elapsedTime.TotalMinutes >= 1)
                                    Console.Write($"{elapsedTime.Minutes} 分 {elapsedTime.Seconds} 秒 {elapsedTime.Milliseconds} 毫秒");
                                else if (elapsedTime.TotalSeconds >= 1)
                                    Console.Write($"{elapsedTime.TotalSeconds:F3} 秒");
                                else if (elapsedTime.TotalMilliseconds >= 1)
                                    Console.Write($"{elapsedTime.TotalMilliseconds:F3} 毫秒");
                                else if (elapsedTime.TotalMicroseconds >= 1)
                                    Console.Write($"{elapsedTime.TotalMicroseconds:F2} 微秒");
                                else
                                    Console.Write($"{elapsedTime.TotalMicroseconds:F2} 纳秒");
                                if (result is not null)
                                {
                                    Console.ForegroundColor = mainColor;
                                    Console.Write("\n\n执行结果：");
                                    Console.ForegroundColor = ConsoleColor.Gray;
                                    if (result.GetType().IsValueType)
                                        Console.WriteLine($"{result}\n");
                                    else
                                        result.X(true, true, "执行结果");
                                }
                                Console.WriteLine();
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
                                    failedList.Add(new MethodAndCounter(method, cTimeCounter, failedMessage));
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
                                if (useXFEConsole)
                                {
                                    if (Type.GetType("XFEExtension.NetCore.XFEConsole.XFEConsole") is Type type && type.GetMethod("StopXFEConsole", BindingFlags.Static | BindingFlags.Public) is MethodInfo stopXFEConsoleMethodInfo)
                                    {
                                        await (Task)stopXFEConsoleMethodInfo.Invoke(null, null)!;
                                    }
                                }
                            }
                        }
                    }
                    classTimeCounter.Stop();
                    cTimeCounter++;
                    Console.WriteLine("\n\n\n\n\n");
                    Console.BackgroundColor = mainColor;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write("执行完成");
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = mainColor;
                    Console.Write("\t执行批次：");
                    Console.ForegroundColor = counterColor;
                    Console.Write(cTimeCounter);
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
                    else if (classTimeCounter.Elapsed.TotalMicroseconds >= 1)
                        Console.Write($"{classTimeCounter.Elapsed.TotalMicroseconds:F2} 微秒");
                    else
                        Console.Write($"{classTimeCounter.Elapsed.TotalMicroseconds:F2} 纳秒");
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
    public static bool Assert(bool condition)
    {
        if (!condition)
        {
            currentMethodIsAsserted = true;
            currentAssertMessage = "断言失败：期望为真，实际为假";
            return false;
        }
        return true;
    }

    /// <summary>
    /// 断言：判断条件为真
    /// </summary>
    /// <param name="condition">判断条件</param>
    /// <param name="message">消息</param>
    public static bool Assert(bool condition, string message)
    {
        if (!condition)
        {
            currentMethodIsAsserted = true;
            currentAssertMessage = message;
            return false;
        }
        return true;
    }

    /// <summary>
    /// 断言：判断条件为假
    /// </summary>
    /// <param name="condition">判断条件</param>
    public static bool AssertF(bool condition)
    {
        if (condition)
        {
            currentMethodIsAsserted = true;
            currentAssertMessage = "断言失败：期望为假，实际为真";
            return false;
        }
        return true;
    }

    /// <summary>
    /// 断言：判断条件为假
    /// </summary>
    /// <param name="condition">判断条件</param>
    /// <param name="message">消息</param>
    public static bool AssertF(bool condition, string message)
    {
        if (condition)
        {
            currentMethodIsAsserted = true;
            currentAssertMessage = message;
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
    public static bool AssertE<T>(T expected, T actual)
    {
        var result = !EqualityComparer<T>.Default.Equals(expected, actual);
        if (result)
        {
            currentMethodIsAsserted = true;
            currentAssertMessage = $"断言失败：期望 {expected}，实际 {actual}";
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
    public static bool AssertE<T>(T expected, T actual, string message)
    {
        var result = !EqualityComparer<T>.Default.Equals(expected, actual);
        if (result)
        {
            currentMethodIsAsserted = true;
            currentAssertMessage = message;
            return false;
        }
        return true;
    }
}