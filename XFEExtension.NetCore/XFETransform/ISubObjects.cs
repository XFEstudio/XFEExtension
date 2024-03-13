namespace XFEExtension.NetCore.XFETransform;

/// <summary>
/// 子对象接口
/// </summary>
public interface ISubObjects : IList<IObjectInfo>
{
    /// <summary>
    /// 输出子对象信息
    /// </summary>
    /// <returns></returns>
    string OutPutSubObjects();
}