using System.Diagnostics;
using System.Security.Principal;

namespace XFE各类拓展.NetCore.PermissionExtension;

#if WINDOWS
/// <summary>
/// UAC权限（管理员权限）
/// </summary>
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public static partial class AdministratorPermission
{
    /// <summary>
    /// 当前请求的状态
    /// </summary>
    public static CurrentPermissionState PermissionState { get; private set; }
    /// <summary>
    /// 当前是否以管理员身份运行
    /// </summary>
    /// <returns></returns>
    public static bool IsAdministrator()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
    /// <summary>
    /// 请求管理员权限并重启程序
    /// </summary>
    public static void GetPermissionAndReboot()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = Environment.ProcessPath,
                Verb = "runas"  // 使用UAC权限提升
            };
            Process.Start(startInfo);
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            PermissionState = CurrentPermissionState.PermissionDenied;
            throw new XFEExtensionException("无法获取管理员权限", ex);
        }
    }
    static AdministratorPermission()
    {
        var isAdmin = IsAdministrator();
        if (isAdmin)
            PermissionState = CurrentPermissionState.Administration;
        else
            PermissionState = CurrentPermissionState.Normal;
    }
}
#endif