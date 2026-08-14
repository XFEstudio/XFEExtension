using System.Diagnostics;
using BenchmarkDotNet.Running;
using XFEExtension.NetCore.Benchmarks;
using XFEExtension.NetCore.XFETransform.Json;

if (args is ["--sample", var samplePath])
{
    var json = File.ReadAllText(samplePath);
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
    var before = GC.GetAllocatedBytesForCurrentThread();
    var stopwatch = Stopwatch.StartNew();
    var node = XFEJson.Parse(json);
    var value = (node > "data" > "cursor" > "is_begin")?.GetBoolean();
    stopwatch.Stop();
    var queryMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
    var queryAllocations = GC.GetAllocatedBytesForCurrentThread() - before;
    stopwatch.Restart();
    XFEJson.Validate(json);
    stopwatch.Stop();
    Console.WriteLine($"chars={json.Length}; value={value}; query_ms={queryMilliseconds:F3}; query_allocated_bytes={queryAllocations}; validate_ms={stopwatch.Elapsed.TotalMilliseconds:F3}");
    return;
}

BenchmarkSwitcher.FromAssembly(typeof(JsonBenchmarks).Assembly).Run(args);
