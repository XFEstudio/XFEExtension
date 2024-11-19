using System.Reflection;
using XFEExtension.NetCore.XFETransform.StringConverter;

namespace XFEExtension.NetCore.XFETransform.ObjectInfoAnalyzer;

internal class ObjectInfoImpl : ObjectInfo
{
    public ObjectInfoImpl(FieldInfo? fieldInfo, PropertyInfo? propertyInfo, IStringConverter? stringConverter, string? name, ObjectPlace objectPlace, int layer, Type? type, bool isBasicType, object? value = null) : base(fieldInfo, propertyInfo, stringConverter, name, objectPlace, layer, type, isBasicType, value)
    {
    }
    public ObjectInfoImpl(FieldInfo? fieldInfo, PropertyInfo? propertyInfo, IStringConverter? stringConverter, string? name, ObjectPlace objectPlace, int layer, Type? type, bool isBasicType, bool isArray, object? value, List<IObjectInfo>? objectInfoList = null) : base(fieldInfo, propertyInfo, stringConverter, name, objectPlace, layer, type, isBasicType, isArray, value, objectInfoList)
    {
    }
}