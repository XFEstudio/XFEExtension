namespace XFEExtension.NetCore.XFETransform;

internal abstract class ObjectInfo : IObjectInfo
{
    public string? Name { get; init; }
    public ObjectPlace ObjectPlace { get; init; }
    public int Layer { get; init; }
    public object? Value { get; init; }
    public Type Type { get; init; }
    public bool IsBasicType { get; init; }
    public bool IsArray { get; init; }
    public ISubObjects? SubObjects { get; init; }

    public string OutPut()
    {
        var outPutString = string.Empty;
        if (IsBasicType)
        {
            if (Value is null)
            {
                outPutString += $"{AddObjectPlace()} 空对象 {Name}：null\n";
            }
            else
            {
                outPutString += $"{AddObjectPlace()} {XFEConverter.OutPutTypeName(Type)} {Name}：{Value}\n";
            }
        }
        else
        {
            if (Layer == 0)
            {
                outPutString += $"""
                {AddObjectPlace()} {XFEConverter.OutPutTypeName(Type)} {Name}
                ┌────┬────────────────────────────
                {SubObjects?.OutPutSubObjects()}
                └─────────────────────────────────

                """;
            }
            else
            {
                outPutString += SubObjects?.OutPutSubObjects();
            }
        }
        return outPutString;
    }

    internal string AddObjectPlace() => ObjectPlace switch
    {
        ObjectPlace.Property => "[属性]",
        ObjectPlace.Field => "[字段]",
        ObjectPlace.Main => "[主体]",
        ObjectPlace.Array => "[数组]",
        ObjectPlace.List => "[列表]",
        ObjectPlace.Enum => "[枚举]",
        ObjectPlace.Other => "[其他]",
        _ => string.Empty,
    };

    public ObjectInfo(string? name, ObjectPlace objectPlace, int layer, Type type, bool isBasicType, object? value = null)
    {
        Name = name;
        ObjectPlace = objectPlace;
        Layer = layer;
        Type = type;
        IsBasicType = isBasicType;
        IsArray = false;
        Value = value;
    }
    public ObjectInfo(string? name, ObjectPlace objectPlace, int layer, Type type, bool isBasicType, bool isArray, object? value = null, List<IObjectInfo>? objectInfoList = null)
    {
        Name = name;
        ObjectPlace = objectPlace;
        Layer = layer;
        Type = type;
        IsBasicType = isBasicType;
        IsArray = isArray;
        Value = value;
        SubObjects = new SubObjectsImpl(this, objectInfoList);
    }
}
