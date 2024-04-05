using System.Net;
using System.Net.NetworkInformation;
using XFEExtension.NetCore.DelegateExtension;
using XFEExtension.NetCore.WebExtension;

namespace XFEExtension.NetCore.WebExtension.LANDeviceDetector;

/// <summary>
/// 局域网设备探测器
/// </summary>
public class LANDeviceDetector
{
    /// <summary>
    /// 找到设备
    /// </summary>
    public event XFEEventHandler<LANDevice>? DeviceFind;

    /// <summary>
    /// 网络地址搜索的起始位置
    /// </summary>
    public string IPStart { get; set; }

    /// <summary>
    /// 每个设备检测超时
    /// </summary>
    public int TimeOut { get; set; }

    /// <summary>
    /// 是否正在检测
    /// </summary>
    public bool IsDetecting { get; set; }

    /// <summary>
    /// 已找到的设备
    /// </summary>
    public List<LANDevice> FindDevices { get; set; } = [];

    /// <summary>
    /// 局域网设备探测器
    /// </summary>
    /// <param name="iPStart">网络地址搜索起始位置，default默认为本机设备所在网络频段</param>
    /// <param name="timeOut">超时</param>
    public LANDeviceDetector(string iPStart = "default", int timeOut = 100)
    {
        if(iPStart == "default")
        {
            var localIPAddress = WebExtension.GetLocalIPAddress();
            if (localIPAddress is not null)
                IPStart = $"{string.Join(".", localIPAddress.ToString().Split('.')[..3])}.*";
            else
                IPStart = "192.168.1.*";
        }
        else
        {
            IPStart = "192.168.1.*";
        }
        TimeOut = timeOut;
    }

    /// <summary>
    /// 开始探测设备
    /// </summary>
    /// <returns></returns>
    public async Task StartDetecting()
    {
        IsDetecting = true;
        while (IsDetecting)
        {
            FindDevices.AddRange(await InnerDetect(reply =>
            {
                IPHostEntry? hostEntry = null;
                try { hostEntry = Dns.GetHostEntry(reply.Address); } catch { }
                var device = new LANDeviceImpl(reply.Address, hostEntry, hostEntry?.HostName);
                if (FindDevices.Any(d => d.IPAddress.ToString() == reply.Address.ToString()))
                    return;
                else
                    FindDevices.Add(device);
                DeviceFind?.Invoke(device);
            }));
        }
    }

    /// <summary>
    /// 检测一次
    /// </summary>
    /// <returns></returns>
    public async Task<List<LANDevice>> Detect() => await InnerDetect(reply =>
    {
        IPHostEntry? hostEntry = null;
        try { hostEntry = Dns.GetHostEntry(reply.Address); } catch { }
        DeviceFind?.Invoke(new LANDeviceImpl(reply.Address, hostEntry, hostEntry?.HostName));
    });

    /// <summary>
    /// 停止探测设备
    /// </summary>
    public void Stop() => IsDetecting = false;

    private async Task<List<LANDevice>> InnerDetect(Action<PingReply> action)
    {
        var tasks = new List<Task>();
        var findDevices = new List<LANDevice>();
        for (int i = 1; i < 256; i++)
        {
            var currentIndex = i;
            tasks.Add(Task.Run(async () =>
            {
                var ipAddress = IPStart.Replace("*", currentIndex.ToString());
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(ipAddress, TimeOut);
                if (reply.Status == IPStatus.Success)
                {
                    IPHostEntry? hostEntry = null;
                    try { hostEntry = Dns.GetHostEntry(ipAddress); } catch { }
                    findDevices.Add(new LANDeviceImpl(reply.Address, hostEntry, hostEntry?.HostName));
                    action.Invoke(reply);
                }
            }));
        }
        await Task.WhenAll(tasks);
        return findDevices;
    }
}