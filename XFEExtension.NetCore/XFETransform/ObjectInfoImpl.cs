namespace XFEExtension.NetCore.XFETransform;

internal class ObjectInfoImpl : ObjectInfo
{
    public ObjectInfoImpl(string? name, ObjectPlace objectPlace, int layer, Type type, bool isBasicType, object? value = null) : base(name, objectPlace, layer, type, isBasicType, value)
    {
    }
    public ObjectInfoImpl(string? name, ObjectPlace objectPlace, int layer, Type type, bool isBasicType, bool isArray, object? value, ISubObjects? subObjects = null) : base(name, objectPlace, layer, type, isBasicType, isArray, value, subObjects)
    {
    }
}