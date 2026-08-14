using System.Text.Json;
using XFEExtension.NetCore.XFETransform.Json;

namespace XFEExtension.NetCore.Test.Json;

public class XFEJsonDifferentialTests
{
    [Fact]
    public void RandomValidDocuments_RoundTripSemantically()
    {
        var random = new Random(0x584645);
        for (var iteration = 0; iteration < 200; iteration++)
        {
            var value = CreateValue(random, 0);
            var systemJson = JsonSerializer.Serialize(value);

            XFEJson.Validate(systemJson);
            var dynamicValue = XFEJson.Deserialize<object>(systemJson);
            var xfeJson = XFEJson.Serialize(dynamicValue);
            using var expected = JsonDocument.Parse(systemJson);
            using var actual = JsonDocument.Parse(xfeJson);

            Assert.True(JsonElement.DeepEquals(expected.RootElement, actual.RootElement),
                $"Semantic mismatch. expected={systemJson}; actual={xfeJson}");
        }
    }

    [Fact]
    public void DuplicateProperties_FirstOccurrenceWinsForQueryAndBinding()
    {
        var node = XFEJson.Parse("{\"value\":1,\"value\":2}");
        Assert.Equal(1, node["value"]!.GetInt32());

        var model = XFEJson.Deserialize<DuplicateModel>("{\"value\":1,\"value\":2}")!;
        Assert.Equal(1, model.Value);
    }

    [Fact]
    public void MaxDepth_IsEnforcedDuringValidationAndNavigation()
    {
        var options = new XFEJsonOptions { MaxDepth = 3 };
        const string json = "{\"a\":{\"b\":{\"c\":{\"d\":1}}}}";

        Assert.Throws<XFEJsonException>(() => XFEJson.Validate(json, options));
        var node = XFEJson.Parse(json, options);
        Assert.Throws<XFEJsonException>(() => _ = node["a"]!["b"]!["c"]!.Kind);
    }

    [Fact]
    public void Validation_UsesExplicitStackForVeryDeepJson()
    {
        const int depth = 1_000;
        var json = new string('[', depth) + "0" + new string(']', depth);

        XFEJson.Validate(json, new XFEJsonOptions { MaxDepth = depth + 1 });
    }

    private static object? CreateValue(Random random, int depth)
    {
        if (depth >= 3)
            return CreateScalar(random);
        return random.Next(6) switch
        {
            0 => CreateScalar(random),
            1 or 2 => Enumerable.Range(0, random.Next(0, 5)).Select(_ => CreateValue(random, depth + 1)).ToList(),
            _ => Enumerable.Range(0, random.Next(0, 5)).ToDictionary(
                index => $"p{depth}_{index}_{random.Next(1000)}",
                _ => CreateValue(random, depth + 1))
        };
    }

    private static object? CreateScalar(Random random) => random.Next(7) switch
    {
        0 => null,
        1 => random.Next(2) == 0,
        2 => random.Next(-1_000_000, 1_000_000),
        3 => random.NextDouble() * 1000 - 500,
        4 => $"text-{random.Next()}-\"-\\-\n-你好-😀",
        5 => (decimal)random.NextDouble() * 10_000m,
        _ => random.NextInt64()
    };

    public sealed class DuplicateModel
    {
        public int Value { get; set; }
    }
}
