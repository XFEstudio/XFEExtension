namespace XFEExtension.NetCore.XFETransform.ObjectInfoAnalyzer;

internal class SubObjectsImpl(IObjectInfo parent, List<IObjectInfo>? objectInfoList = null) : SubObjects(objectInfoList, parent)
{
}