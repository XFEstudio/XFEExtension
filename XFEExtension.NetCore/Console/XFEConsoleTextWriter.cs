using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace XFEExtension.NetCore.XFEConsole;

/// <summary>
/// XFE控制台文本写入器
/// </summary>
public class XFEConsoleTextWriter : TextWriter
{
    /// <inheritdoc/>
    public override Encoding Encoding => Encoding.UTF8;

    /// <inheritdoc/>
    public override async void Write(bool value)
    {
        base.Write(value);
        await XFEConsole.Write(value.ToString());
    }

    /// <inheritdoc/>
    public override async void Write(char value)
    {
        base.Write(value);
        await XFEConsole.Write(value.ToString());
    }

    /// <inheritdoc/>
    public override async void Write(char[]? buffer)
    {
        base.Write(buffer);
        if(buffer is not null)
        {
            var stringBuilder = new StringBuilder();
            foreach (var sigChar in buffer)
            {
                stringBuilder.Append(sigChar);
            }
            await XFEConsole.Write(stringBuilder.ToString());
        }
    }

    /// <inheritdoc/>
    public override async void Write(char[] buffer, int index, int count)
    {
        base.Write(buffer, index, count);
        for (int i = index; i < count; i++)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(buffer[i]);
            await XFEConsole.Write(stringBuilder.ToString());
        }
    }

    /// <inheritdoc/>
    public override async void Write(decimal value)
    {
        base.Write(value);
        await XFEConsole.Write(value.ToString());
    }

    /// <inheritdoc/>
    public override async void Write(double value)
    {
        base.Write(value);
        await XFEConsole.Write(value.ToString());
    }

    /// <inheritdoc/>
    public override async void Write(int value)
    {
        base.Write(value);
        await XFEConsole.Write(value.ToString());
    }

    /// <inheritdoc/>
    public override async void Write(long value)
    {
        base.Write(value);
        await XFEConsole.Write(value.ToString());
    }

    /// <inheritdoc/>
    public override async void Write(object? value)
    {
        base.Write(value);
        await XFEConsole.Write(value?.ToString());
    }

    /// <inheritdoc/>
    public override async void Write(float value)
    {
        base.Write(value);
        await XFEConsole.Write(value.ToString());
    }

    /// <inheritdoc/>
    public override async void Write(string? value)
    {
        base.Write(value);
        await XFEConsole.Write(value);
    }

    /// <inheritdoc/>
    public override async void Write(StringBuilder? value)
    {
        base.Write(value);
        await XFEConsole.Write(value?.ToString());
    }

    /// <inheritdoc/>
    public override async void Write(uint value)
    {
        base.Write(value);
        await XFEConsole.Write(value.ToString());
    }

    /// <inheritdoc/>
    public override async void Write(ulong value)
    {
        base.Write(value);
        await XFEConsole.Write(value.ToString());
    }

    /// <inheritdoc/>
    public override async Task WriteAsync(char value)
    {
        await base.WriteAsync(value);
        await XFEConsole.Write(value.ToString());
    }

    /// <inheritdoc/>
    public override async Task WriteAsync(char[] buffer, int index, int count)
    {
        await base.WriteAsync(buffer, index, count);
        for (int i = index; i < count; i++)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(buffer[i]);
            await XFEConsole.Write(stringBuilder.ToString());
        }
    }

    /// <inheritdoc/>
    public override async Task WriteAsync(string? value)
    {
        await base.WriteAsync(value);
        await XFEConsole.Write(value);
    }

    /// <inheritdoc/>
    public override async void WriteLine()
    {
        base.WriteLine();
        await XFEConsole.WriteLine("");
    }

    /// <inheritdoc/>
    public override async void WriteLine(bool value)
    {
        base.WriteLine(value);
        await XFEConsole.WriteLine(value.ToString());
    }

    /// <inheritdoc/>
    public override async void WriteLine(char value)
    {
        base.WriteLine(value);
        await XFEConsole.WriteLine(value.ToString());
    }

    /// <inheritdoc/>
    public override async void WriteLine(char[]? buffer)
    {
        base.WriteLine(buffer);
        if (buffer is not null)
        {
            var stringBuilder = new StringBuilder();
            foreach (var sigChar in buffer)
            {
                stringBuilder.Append(sigChar);
            }
            await XFEConsole.WriteLine(stringBuilder.ToString());
        }
    }

    /// <inheritdoc/>
    public override async void WriteLine(char[] buffer, int index, int count)
    {
        base.WriteLine(buffer, index, count);
        for (int i = index; i < count; i++)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(buffer[i]);
            await XFEConsole.WriteLine(stringBuilder.ToString());
        }
    }

    /// <inheritdoc/>
    public override async void WriteLine(decimal value)
    {
        base.WriteLine(value);
        await XFEConsole.WriteLine(value.ToString());
    }

    /// <inheritdoc/>
    public override async void WriteLine(double value)
    {
        base.WriteLine(value);
        await XFEConsole.WriteLine(value.ToString());
    }

    /// <inheritdoc/>
    public override async void WriteLine(int value)
    {
        base.WriteLine(value);
        await XFEConsole.WriteLine(value.ToString());
    }

    /// <inheritdoc/>
    public override async void WriteLine(long value)
    {
        base.WriteLine(value);
        await XFEConsole.WriteLine(value.ToString());
    }

    /// <inheritdoc/>
    public override async void WriteLine(object? value)
    {
        base.WriteLine(value);
        if(XFEConsole.AutoAnalyzeObject)
            await XFEConsole.WriteObject(value);
        else
            await XFEConsole.WriteLine(value?.ToString());
    }

    /// <inheritdoc/>
    public override async void WriteLine(float value)
    {
        base.WriteLine(value);
        await XFEConsole.WriteLine(value.ToString());
    }

    /// <inheritdoc/>
    public override async void WriteLine(string? value)
    {
        base.WriteLine(value);
        await XFEConsole.WriteLine(value);
    }

    /// <inheritdoc/>
    public override async void WriteLine(StringBuilder? value)
    {
        base.WriteLine(value);
        await XFEConsole.WriteLine(value?.ToString());
    }

    /// <inheritdoc/>
    public override async void WriteLine(uint value)
    {
        base.WriteLine(value);
        await XFEConsole.WriteLine(value.ToString());
    }

    /// <inheritdoc/>
    public override async void WriteLine(ulong value)
    {
        base.WriteLine(value);
        await XFEConsole.WriteLine(value.ToString());
    }

    /// <inheritdoc/>
    public override async Task WriteLineAsync()
    {
        await base.WriteLineAsync();
        await XFEConsole.WriteLine("");
    }

    /// <inheritdoc/>
    public override async Task WriteLineAsync(char value)
    {
        await base.WriteLineAsync(value);
        await XFEConsole.WriteLine(value.ToString());
    }

    /// <inheritdoc/>
    public override async Task WriteLineAsync(char[] buffer, int index, int count)
    {
        await base.WriteLineAsync(buffer, index, count);
        for (int i = index; i < count; i++)
        {
            var stringBuilder = new StringBuilder(); 
            stringBuilder.Append(buffer[i]);
            await XFEConsole.WriteLine(stringBuilder.ToString());
        }
    }

    /// <inheritdoc/>
    public override async Task WriteLineAsync(string? value)
    {
        await base.WriteLineAsync(value);
        await XFEConsole.WriteLine(value);
    }
}
