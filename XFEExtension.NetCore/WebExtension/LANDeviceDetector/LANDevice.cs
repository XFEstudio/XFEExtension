using System.Net;

namespace XFEExtension.NetCore.WebExtension.LANDeviceDetector;

/// <summary>
/// 局域网设备
/// </summary>
public abstract class LANDevice(IPAddress iPAddress, IPHostEntry? hostEntry, string? deviceName)
{
    /// <summary>
    /// 设备名称
    /// </summary>
    public string? DeviceName { get; set; } = deviceName;
    /// <summary>
    /// Host入口
    /// </summary>
    public IPHostEntry? HostEntry { get; set; } = hostEntry;
    /// <summary>
    /// IP地址
    /// </summary>
    public IPAddress IPAddress { get; set; } = iPAddress;
}