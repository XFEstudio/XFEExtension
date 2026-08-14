#pragma warning disable CS0618
using XFEExtension.NetCore.XFETransform.JsonConverter;

namespace XFEExtension.NetCore.Test.Json;

public class XFEJsonCompatibilityTests
{
    [Fact]
    public void QueryableJsonNode_UsesLazyEngineAndPreservesNavigation()
    {
        QueryableJsonNode node = "{\"status\":500,\"data\":{\"name\":\"ok\"},\"values\":[1,2,3]}";

        Assert.Equal("500", node["status"]!.Value);
        Assert.Equal("ok", (node > "data" > "name")!.Value);
        Assert.Equal(new[] { 1, 2, 3 }, node["values"]!.GetList<int>());
    }

    [Fact]
    public void QueryableJsonNode_PreservesPackageOperations()
    {
        QueryableJsonNode node = "{\"code\":0,\"message\":\"ok\",\"items\":[{\"id\":1,\"name\":\"one\",\"trash\":true},{\"id\":2,\"name\":\"two\"}]}";

        var objectPackage = node["package:object", "code", "message"]!.PackageObject();
        Assert.Equal("0", objectPackage["code"].Value);
        Assert.Equal("ok", objectPackage["message"].Value);

        var listPackage = node["items"]!["package:list", "id", "name"]!.PackageInListObject();
        Assert.Equal(2, listPackage.Count);
        Assert.Equal("two", listPackage[1]["name"].Value);
    }

    [Fact]
    public void JsonNodeConverter_StillProvidesExplicitEagerTree()
    {
        var tree = new JsonNodeConverter("{\"data\":{\"value\":7}}").ConvertToJsonNode();
        var root = Assert.IsType<JsonComplexPropertyNode>(tree);
        var data = Assert.IsType<JsonComplexPropertyNode>(Assert.Single(root.DescendingNodes));
        var value = Assert.IsType<JsonPropertyNode>(Assert.Single(data.DescendingNodes));

        Assert.Equal("data", data.PropertyName);
        Assert.Equal("value", value.PropertyName);
        Assert.Equal("7", value.Value);
    }
}
#pragma warning restore CS0618
