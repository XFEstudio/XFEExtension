﻿namespace XFEExtension.NetCore.XFETransform;

/// <summary>
/// 子对象接口
/// </summary>
public interface ISubObjects : IList<IObjectInfo>
{
    /// <summary>
    /// 父类
    /// </summary>
    IObjectInfo Parent { get; init; }
}