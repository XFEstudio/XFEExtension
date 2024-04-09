using XFEExtension.NetCore.XFETransform.ObjectInfoAnalyzer;

namespace XFEExtension.NetCore.XFETransform.StringConverter;

internal abstract class StringConverter : IStringConverter
{
    public abstract string OutPutObject(IObjectInfo objectInfo);
    internal abstract string OutPutSubObjects(ISubObjects subObjects);
}
