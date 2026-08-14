using System.Text.Json;
using XFEExtension.NetCore.StringExtension.Json;
using XFEExtension.NetCore.XFEChatGPT;
using XFEExtension.NetCore.XFEChatGPT.ChatGPTInnerClass.HelperClass;
using XFEExtension.NetCore.XFETransform.Json;

namespace XFEExtension.NetCore.Test.Json;

public class XFEJsonCodecTests
{
    [Fact]
    public void SerializeAndDeserialize_RoundTripsCommonTypes()
    {
        var expected = new CodecModel
        {
            Id = 42,
            Name = "A \"quoted\" name 😀",
            Enabled = true,
            Price = 12.50m,
            Ratio = 1.25e10,
            State = CodecState.Ready,
            CreatedAt = new DateTime(2025, 4, 3, 2, 1, 0, DateTimeKind.Utc),
            Offset = new DateTimeOffset(2025, 4, 3, 2, 1, 0, TimeSpan.FromHours(8)),
            Duration = TimeSpan.FromMinutes(90),
            Token = Guid.Parse("697843e1-fadc-4f36-aac5-b07c2ddca1b6"),
            Initial = 'X',
            Optional = null,
            Tags = ["one", "two"],
            Scores = new() { ["math"] = 99, ["code"] = 100 },
            Child = new() { Value = 7 }
        };

        var json = XFEJson.Serialize(expected);
        using var document = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Object, document.RootElement.ValueKind);

        var actual = XFEJson.Deserialize<CodecModel>(json)!;
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Enabled, actual.Enabled);
        Assert.Equal(expected.Price, actual.Price);
        Assert.Equal(expected.Ratio, actual.Ratio);
        Assert.Equal(expected.State, actual.State);
        Assert.Equal(expected.CreatedAt, actual.CreatedAt);
        Assert.Equal(expected.Offset, actual.Offset);
        Assert.Equal(expected.Duration, actual.Duration);
        Assert.Equal(expected.Token, actual.Token);
        Assert.Equal(expected.Initial, actual.Initial);
        Assert.Null(actual.Optional);
        Assert.Equal(expected.Tags, actual.Tags);
        Assert.Equal(expected.Scores, actual.Scores);
        Assert.Equal(7, actual.Child.Value);
    }

    [Fact]
    public void Deserialize_IsCaseInsensitiveAndSkipsUnknownMembersByDefault()
    {
        var model = XFEJson.Deserialize<CodecModel>("{\"id\":3,\"NAME\":\"test\",\"unknown\":true}")!;

        Assert.Equal(3, model.Id);
        Assert.Equal("test", model.Name);
    }

    [Fact]
    public void Deserialize_CanRejectUnknownMembers()
    {
        var options = new XFEJsonOptions { UnmappedMemberHandling = XFEJsonUnmappedMemberHandling.Error };
        var exception = Assert.Throws<XFEJsonException>(() => XFEJson.Deserialize<CodecModel>("{\"unknown\":true}", options));

        Assert.Contains("unknown", exception.Path, StringComparison.Ordinal);
    }

    [Fact]
    public void Options_SupportCamelCaseFieldsAndStringEnums()
    {
        var options = new XFEJsonOptions
        {
            PropertyNamingPolicy = XFEJsonPropertyNamingPolicy.CamelCase,
            IncludeFields = true,
            EnumFormat = XFEJsonEnumFormat.String,
            WriteIndented = true
        };
        var value = new FieldModel { PublicField = 5, CurrentState = CodecState.Ready };

        var json = value.ToJson(options);
        Assert.Contains("\"publicField\"", json, StringComparison.Ordinal);
        Assert.Contains("\"currentState\": \"Ready\"", json, StringComparison.Ordinal);

        var roundTrip = json.FromJson<FieldModel>(options)!;
        Assert.Equal(5, roundTrip.PublicField);
        Assert.Equal(CodecState.Ready, roundTrip.CurrentState);
    }

    [Fact]
    public void Serialize_DetectsReferenceCycles()
    {
        var value = new CycleModel();
        value.Next = value;

        var exception = Assert.Throws<XFEJsonException>(() => XFEJson.Serialize(value));
        Assert.Contains("循环", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Next", exception.Path, StringComparison.Ordinal);
    }

    [Fact]
    public void Deserialize_RequiresPublicParameterlessConstructor()
    {
        Assert.Throws<XFEJsonException>(() => XFEJson.Deserialize<ParameterizedModel>("{\"Value\":1}"));
    }

    [Fact]
    public void RootDeserialize_RejectsTrailingContent()
    {
        Assert.Throws<XFEJsonException>(() => XFEJson.Deserialize<int>("1 2"));
    }

    [Fact]
    public void ChatRequestSerialization_PreservesExistingWireNamesAndEnumNumbers()
    {
        var request = new XFEAskGPTMessage(false, true, "gpt-test", null, XFEComProtocol.XFEHARD, "system", "ask");

        var json = XFEJson.Serialize(request);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.False(root.GetProperty("IsSelfEditData").GetBoolean());
        Assert.True(root.GetProperty("Stream").GetBoolean());
        Assert.Equal("gpt-test", root.GetProperty("ChatGPTModel").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("EnvironmentGPTData").ValueKind);
        Assert.Equal(1, root.GetProperty("ComProtocol").GetInt32());
        Assert.Equal("system", root.GetProperty("SystemContent").GetString());
        Assert.Equal("ask", root.GetProperty("AskContent").GetString());
    }

    public sealed class CodecModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public decimal Price { get; set; }
        public double Ratio { get; set; }
        public CodecState State { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTimeOffset Offset { get; set; }
        public TimeSpan Duration { get; set; }
        public Guid Token { get; set; }
        public char Initial { get; set; }
        public int? Optional { get; set; }
        public List<string> Tags { get; set; } = [];
        public Dictionary<string, int> Scores { get; set; } = [];
        public ChildModel Child { get; set; } = new();
    }

    public sealed class ChildModel
    {
        public int Value { get; set; }
    }

    public sealed class FieldModel
    {
        public int PublicField;
        public CodecState CurrentState { get; set; }
    }

    public sealed class CycleModel
    {
        public CycleModel? Next { get; set; }
    }

    public sealed class ParameterizedModel(int value)
    {
        public int Value { get; set; } = value;
    }

    public enum CodecState
    {
        Unknown,
        Ready
    }
}
