using System.Collections.Concurrent;
using XFEExtension.NetCore.XFETransform.Json;

namespace XFEExtension.NetCore.Test.Json;

public class XFEJsonNavigationTests
{
    [Fact]
    public void Parse_IsLazy_AndEarlyPropertyDoesNotReadInvalidTail()
    {
        var node = XFEJson.Parse("{\"first\":1,\"invalid\":[}");

        Assert.Equal(0, node.ScannedCharacters);
        Assert.Equal(1, node["first"]!.GetInt32());
        var afterFirstRead = node.ScannedCharacters;
        Assert.True(afterFirstRead < 20);
        Assert.Equal(1, node["first"]!.GetInt32());
        Assert.Equal(afterFirstRead, node.ScannedCharacters);
        Assert.Throws<XFEJsonException>(node.Validate);
    }

    [Fact]
    public void Navigation_SupportsObjectsArraysOperatorsAndProjection()
    {
        XFEJsonNode node = """
            {
              "data": {
                "items": [
                  { "id": 1, "name": "one", "ignored": { "deep": true } },
                  { "id": 2, "name": "two" }
                ]
              }
            }
            """;

        Assert.Equal(2, (node > "data" > "items" > 1 > "id")!.GetInt32());
        Assert.Equal("one", node["data"]!["items"]![0]!["name"]!.GetString());
        Assert.Null(node["missing"]);
        Assert.Null(node["data"]!["items"]![99]);

        var projections = node["data"]!["items"]!.ProjectArray("id", "name", "missing").ToArray();
        Assert.Equal(2, projections.Length);
        Assert.Equal(1, projections[0]["id"]!.GetInt32());
        Assert.Null(projections[0]["missing"]);
    }

    [Fact]
    public void StringsAndNumbers_FollowStrictJsonRules()
    {
        var node = XFEJson.Parse("""
            {
              "text": "quote:\" slash:\\ solidus:\/ controls:\b\f\n\r\t",
              "unicode": "\u4F60\u597D \uD83D\uDE00",
              "number": -1.25e+3,
              "null": null
            }
            """);

        Assert.Equal("quote:\" slash:\\ solidus:/ controls:\b\f\n\r\t", node["text"]!.GetString());
        Assert.Equal("你好 😀", node["unicode"]!.GetString());
        Assert.Equal(-1250d, node["number"]!.GetDouble());
        Assert.True(node["null"]!.IsNull);
        node.Validate();
    }

    [Theory]
    [InlineData("")]
    [InlineData("01")]
    [InlineData("1.")]
    [InlineData("1e")]
    [InlineData("TRUE")]
    [InlineData("{'a':1}")]
    [InlineData("{\"a\":1,}")]
    [InlineData("[1,]")]
    [InlineData("\"\\x\"")]
    [InlineData("true false")]
    public void Validate_RejectsInvalidJson(string json)
    {
        Assert.False(XFEJson.TryValidate(json, out var error));
        Assert.NotNull(error);
        Assert.True(error.Position >= 0);
        Assert.StartsWith("$", error.Path);
    }

    [Fact]
    public void LargeArray_DoesNotOverflowShortIndex()
    {
        const int count = 40_000;
        var text = "[" + string.Join(',', Enumerable.Range(0, count)) + "]";
        var node = XFEJson.Parse(text);

        Assert.Equal(count - 1, node[count - 1]!.GetInt32());
        Assert.Equal(count, node.Count);
        node.Validate();
    }

    [Fact]
    public void ConcurrentReads_AreSafeAndConsistent()
    {
        var text = "{" + string.Join(',', Enumerable.Range(0, 500).Select(index => $"\"p{index}\":{index}")) + "}";
        var node = XFEJson.Parse(text);
        var failures = new ConcurrentQueue<int>();

        Parallel.For(0, 2_000, index =>
        {
            var expected = index % 500;
            if (node[$"p{expected}"]?.GetInt32() != expected)
                failures.Enqueue(expected);
        });

        Assert.Empty(failures);
        Assert.Equal(500, node.Count);
    }

    [Fact]
    public void FullValidation_ScansLinearlyWithInputLength()
    {
        var smallText = "[" + string.Join(',', Enumerable.Range(0, 1_000)) + "]";
        var largeText = "[" + string.Join(',', Enumerable.Range(0, 10_000)) + "]";
        var small = XFEJson.Parse(smallText);
        var large = XFEJson.Parse(largeText);

        small.Validate();
        large.Validate();

        var inputRatio = (double)largeText.Length / smallText.Length;
        var scanRatio = (double)large.ScannedCharacters / small.ScannedCharacters;
        Assert.InRange(scanRatio / inputRatio, 0.95, 1.05);
    }
}
