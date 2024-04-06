namespace XFEExtension.NetCore.XFETransform;

internal class SubObjectsImpl(ObjectInfo parent, List<IObjectInfo>? objectInfoList = null) : SubObjects(objectInfoList)
{
    protected override ObjectInfo Parent { get; init; } = parent;
}