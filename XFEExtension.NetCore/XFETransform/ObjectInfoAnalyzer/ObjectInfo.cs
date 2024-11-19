using System.Reflection;
using XFEExtension.NetCore.XFETransform.StringConverter;

namespace XFEExtension.NetCore.XFETransform.ObjectInfoAnalyzer;

internal abstract class ObjectInfo : IObjectInfo
{
    public string? Name { get; init; }
    public ObjectPlace ObjectPlace { get; init; }
    public int Layer { get; init; }
    public object? Value { get; init; }
    public Type? Type { get; init; }
    public bool IsBasicType { get; init; }
    public bool IsArray { get; init; }
    public ISubObjects? SubObjects { get; init; }
    public IStringConverter? StringConverter { get; init; }
    public FieldInfo? FieldInfo { get; init; }
    public PropertyInfo? PropertyInfo { get; init; }
    public IObjectInfo? Parent { get; set; }

    public string OutPutObject()
    {
        return StringConverter!.OutPutObject(this);
    }

    public ObjectInfo(FieldInfo? fieldInfo, PropertyInfo? propertyInfo, IStringConverter? stringConverter, string? name, ObjectPlace objectPlace, int layer, Type? type, bool isBasicType, object? value = null)
    {
        FieldInfo = fieldInfo;
        PropertyInfo = propertyInfo;
        StringConverter = stringConverter;
        Name = name;
        ObjectPlace = objectPlace;
        Layer = layer;
        Type = type;
        IsBasicType = isBasicType;
        IsArray = false;
        Value = value;
    }
    public ObjectInfo(FieldInfo? fieldInfo, PropertyInfo? propertyInfo, IStringConverter? stringConverter, string? name, ObjectPlace objectPlace, int layer, Type? type, bool isBasicType, bool isArray, object? value = null, List<IObjectInfo>? objectInfoList = null)
    {
        FieldInfo = fieldInfo;
        PropertyInfo = propertyInfo;
        StringConverter = stringConverter;
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
