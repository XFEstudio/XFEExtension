namespace XFEExtension.NetCore.XFETransform;

internal abstract class StringConverter : IStringConverter
{
    public abstract string OutPutObject(IObjectInfo objectInfo);
    public abstract string OutPutSubObjects(ISubObjects subObjects);
}
