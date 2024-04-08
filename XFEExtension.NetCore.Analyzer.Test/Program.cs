using XFEExtension.NetCore.FileExtension;
using XFEExtension.NetCore.WebExtension;

namespace XFEExtension.NetCore.Analyzer.Test;

internal class Program
{
    public static async Task Main(string[] args)
    {
        var xFEDownloader = new XFEDownloader()
        {
            DownloadUrl = "https://www.xfegzs.com/com.xfegzs.xccchatroom.apk",
            SavePath = @"D:\work\C#\OtherProject\XUnitConsole\XUnitConsole\bin\Debug\net8.0\com.xfegzs.xccchatroom.apk"
        };
        xFEDownloader.BufferDownloaded += XFEDownloader_BufferDownloaded;
        var task = Task.Run(async () =>
        {
            await Task.Delay(5000);
            xFEDownloader.Pause();
            await Console.Out.WriteLineAsync("已暂停，将于10秒后开始");
            await Task.Delay(10000);
            await xFEDownloader.Continue();
        });
        await xFEDownloader.Download();
        await task;
    }

    private static void XFEDownloader_BufferDownloaded(XFEDownloader sender, FileDownloadedEventArgs e)
    {
        Console.WriteLine($"{e.DownloadedBufferSize.FileSize()} /{((long)e.TotalBufferSize!).FileSize()}");
    }
}