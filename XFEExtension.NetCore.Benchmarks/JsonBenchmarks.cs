using System.Text;
using BenchmarkDotNet.Attributes;
using XFEExtension.NetCore.XFETransform.Json;

namespace XFEExtension.NetCore.Benchmarks;

[MemoryDiagnoser]
public class JsonBenchmarks
{
    private string _json = null!;
    private XFEJsonNode _node = null!;

    [Params(1_000, 40_000)]
    public int ElementCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var builder = new StringBuilder(ElementCount * 32);
        builder.Append("{\"first\":1,\"items\":[");
        for (var index = 0; index < ElementCount; index++)
        {
            if (index > 0) builder.Append(',');
            builder.Append("{\"id\":").Append(index).Append(",\"text\":\"item-").Append(index).Append("\"}");
        }
        builder.Append("],\"last\":2}");
        _json = builder.ToString();
        _node = XFEJson.Parse(_json);
    }

    [Benchmark(Baseline = true)]
    public XFEJsonNode ParseOnly() => XFEJson.Parse(_json);

    [Benchmark]
    public int EarlyProperty() => XFEJson.Parse(_json)["first"]!.GetInt32();

    [Benchmark]
    public int DeepArrayElement() => XFEJson.Parse(_json)["items"]![ElementCount - 1]!["id"]!.GetInt32();

    [Benchmark]
    public int CachedDeepArrayElement() => _node["items"]![ElementCount - 1]!["id"]!.GetInt32();

    [Benchmark]
    public void ValidateDocument() => XFEJson.Validate(_json);
}
