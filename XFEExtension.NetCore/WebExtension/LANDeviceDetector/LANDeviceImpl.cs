using System.Net;

namespace XFEExtension.NetCore.WebExtension.LANDeviceDetector;

internal class LanDeviceImpl(IPAddress iPAddress, IPHostEntry? hostEntry, string? deviceName) : LanDevice(iPAddress, hostEntry,  deviceName);