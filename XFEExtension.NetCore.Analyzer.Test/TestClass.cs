using XFEExtension.NetCore.WebExtension.LANDeviceDetector;

namespace XFEExtension.NetCore.Analyzer.Test;

internal class TestClass
{
    static async Task Main(string[] args)
    {
        var lANDeviceDetector = new LANDeviceDetector();
        lANDeviceDetector.DeviceFind += LANDeviceDetector_DeviceFind;
        await lANDeviceDetector.StartDetecting();
    }

    private static void LANDeviceDetector_DeviceFind(LANDevice sender)
    {
        Console.WriteLine($"IP:{sender.IPAddress}\tHost:{sender.DeviceName}");
    }
}