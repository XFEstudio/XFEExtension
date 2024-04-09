using XFEExtension.NetCore.XFETransform.ObjectInfoAnalyzer;

namespace XFEExtension.NetCore.XFETransform.StringConverter;

/// <summary>
/// 字符串转换接口
/// </summary>
public interface IStringConverter
{
    /// <summary>
    /// 输出对象信息
    /// </summary>
    /// <param name="objectInfo">对象信息</param>
    /// <returns></returns>
    string OutPutObject(IObjectInfo objectInfo);
}
