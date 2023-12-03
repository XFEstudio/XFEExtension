using System.Reflection;

namespace XFE各类拓展.NetCore;

class MethodAndCounter(MethodInfo method, int counter, string failMessage)
{
    public MethodInfo Method { get; set; } = method;
    public int Counter { get; set; } = counter;
    public string FailMessage { get; set; } = failMessage;
}