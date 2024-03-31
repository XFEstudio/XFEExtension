using System.Net;

namespace XFEExtension.NetCore.WebExtension.LANDeviceDetector;

internal class LANDeviceImpl(IPAddress iPAddress, IPHostEntry? hostEntry, string? deviceName) : LANDevice(iPAddress, hostEntry,  deviceName) { }