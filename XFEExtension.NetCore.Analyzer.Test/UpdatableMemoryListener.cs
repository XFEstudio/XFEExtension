using XFEExtension.NetCore.ImplExtension;

namespace XFEExtension.NetCore.Analyzer.Test;

[CreateImpl]
public abstract class UpdatableMemoryListener
{
    internal UpdatableMemoryListener(params int[] args)
    {
        foreach (var arg in args)
            Console.WriteLine(arg);
    }
}
