namespace XFEExtension.NetCore.XFETransform;

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
