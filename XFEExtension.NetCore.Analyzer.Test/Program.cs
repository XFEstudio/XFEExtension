using System.Diagnostics;
using System.IO.Pipes;

namespace XFEExtension.NetCore.Analyzer.Test;

internal class Program
{
    [SMTest]
    public static async Task TestNamedPipeAsync()
    {
        try
        {
            if (Console.ReadLine() == "s")
            {
                using var pipeServer = new NamedPipeServerStream("testpipe", PipeDirection.InOut);
                Console.WriteLine("Named Pipe Server waiting for connection...");
                while (true)
                {
                    await pipeServer.WaitForConnectionAsync();
                    _ = Task.Run(async () =>
                    {
                        Console.WriteLine("Client Connected!");
                        using var reader = new StreamReader(pipeServer);
                        using var writer = new StreamWriter(pipeServer);
                        var message = await reader.ReadLineAsync();
                        Console.WriteLine("Received from client: " + message);
                        string response = "Hello from server!";
                        await writer.WriteLineAsync(response);
                        writer.Flush();
                        await Task.Delay(1000);
                        await writer.WriteLineAsync(response);
                        writer.Flush();
                        await Task.Delay(1000);
                        await writer.WriteLineAsync(response);
                        writer.Flush();
                        await Task.Delay(1000);
                        await writer.WriteLineAsync(response);
                        writer.Flush();
                        await Task.Delay(1000);
                        await writer.WriteLineAsync(response);
                        writer.Flush();
                        await Task.Delay(1000);
                        await writer.WriteLineAsync(response);
                        writer.Flush();
                        await Task.Delay(1000);
                        await writer.WriteLineAsync(response);
                        writer.Flush();
                    });
                }
            }
            else
            {
                using var pipeClient = new NamedPipeClientStream(".", "testpipe", PipeDirection.InOut);
                Console.WriteLine("Named Pipe Client connecting to server...");
                await pipeClient.ConnectAsync();
                using var reader = new StreamReader(pipeClient);
                using var writer = new StreamWriter(pipeClient);
                string message = "Hello from client!";
                await writer.WriteLineAsync(message);
                writer.Flush();
                while (true)
                {
                    var response = await reader.ReadLineAsync();
                    Console.WriteLine("Received from server: " + response);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        Console.WriteLine("Program End");
        Console.ReadLine();
    }
}
enum MyEnum
{
    Test1 = 3,
    Test2 = 12,
    Test3 = 1
}