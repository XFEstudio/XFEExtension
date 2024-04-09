namespace XFEExtension.NetCore.XFETransform;

internal class SubObjectsImpl(IObjectInfo parent, List<IObjectInfo>? objectInfoList = null) : SubObjects(objectInfoList, parent)
{
}