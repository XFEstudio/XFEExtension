using System.Diagnostics;
using System.Security.Principal;
using XFE各类拓展.NetCore.FileExtension;

namespace XFE各类拓展.NetCore.PermissionExtension;

#if WINDOWS
#pragma warning disable CA1416 // 验证平台兼容性
/// <summary>
/// UAC权限（管理员权限）
/// </summary>
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
    /// 
    /// </summary>
    public static void GetPermissionAndReboot()
    {
        try
        {
            "0".WriteIn("pm.xfe");
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
            File.Delete("pm.xfe");
            throw new XFEExtensionException("无法获取管理员权限", ex);
        }
    }
    static AdministratorPermission()
    {
        var result = "pm.xfe".ReadOut();
        var isAdmin = IsAdministrator();
        if (isAdmin)
        {
            PermissionState = CurrentPermissionState.Administration;
        }
        else
        {
            if (result == "-1")
                PermissionState = CurrentPermissionState.Normal;
            else
                PermissionState = CurrentPermissionState.PermissionDenied;
        }
        if (File.Exists("pm.xfe"))
            File.Delete("pm.xfe");
    }
}
#pragma warning restore CA1416 // 验证平台兼容性
#endif