namespace XFE各类拓展.NetCore.PermissionExtension;

/// <summary>
/// 当前UAC权限状态
/// </summary>
public enum CurrentPermissionState
{
    /// <summary>
    /// 已取得管理员权限
    /// </summary>
    Administration,
    /// <summary>
    /// 管理员权限请求被拒绝
    /// </summary>
    PermissionDenied,
    /// <summary>
    /// 正常状态
    /// </summary>
    Normal
}
