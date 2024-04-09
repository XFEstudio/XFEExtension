using XFEExtension.NetCore.XFETransform.ObjectInfoAnalyzer;

namespace XFEExtension.NetCore.XFETransform.StringConverter;

/// <summary>
/// 字符串转换器
/// </summary>
public abstract class StringConverter : IStringConverter
{
    /// <summary>
    /// 输出对象信息
    /// </summary>
    /// <param name="objectInfo"></param>
    /// <returns></returns>
    public abstract string OutPutObject(IObjectInfo objectInfo);
    internal abstract string OutPutSubObjects(ISubObjects subObjects);
}
