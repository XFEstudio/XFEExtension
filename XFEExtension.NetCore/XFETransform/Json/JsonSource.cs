using System.Buffers;
using System.Text;

namespace XFEExtension.NetCore.XFETransform.Json;

internal sealed class JsonSource(string text, JsonSettings settings)
{
    public string Text { get; } = text;
    public JsonSettings Settings { get; } = settings;
    public long ScannedCharacters => Interlocked.Read(ref _scannedCharacters);

    private long _scannedCharacters;

    public void RecordScan(int count)
    {
        if (count > 0)
            Interlocked.Add(ref _scannedCharacters, count);
    }

    public XFEJsonException Error(string message, int position, string path, Exception? innerException = null)
    {
        position = Math.Clamp(position, 0, Text.Length);
        var line = 1;
        var column = 1;
        for (var index = 0; index < position; index++)
        {
            if (Text[index] == '\n')
            {
                line++;
                column = 1;
            }
            else
            {
                column++;
            }
        }

        var error = new XFEJsonError
        {
            Message = message,
            Path = path,
            Position = position,
            LineNumber = line,
            ColumnNumber = column
        };
        return innerException is null ? new(error) : new(error, innerException);
    }

    public static XFEJsonException ConversionError(string message, string path, Exception? innerException = null)
    {
        var error = new XFEJsonError
        {
            Message = message,
            Path = path,
            Position = -1,
            LineNumber = 0,
            ColumnNumber = 0
        };
        return innerException is null ? new(error) : new(error, innerException);
    }
}

internal static class JsonScanner
{
    public static void ValidateDocument(JsonSource source, string path = "$")
    {
        var position = 0;
        SkipWhitespace(source.Text, ref position);
        if (position >= source.Text.Length)
            throw source.Error("JSON 文本不能为空。", position, path);

        var start = position;
        position = SkipValueCore(source, position, 0, path, out _);
        source.RecordScan(position - start);
        SkipWhitespace(source.Text, ref position);
        if (position != source.Text.Length)
            throw source.Error("根 JSON 值之后存在额外内容。", position, path);
    }

    public static int SkipValue(JsonSource source, int position, string path, out XFEJsonValueKind kind)
    {
        var original = position;
        SkipWhitespace(source.Text, ref position);
        if (position >= source.Text.Length)
            throw source.Error("缺少 JSON 值。", position, path);
        var valueStart = position;
        var end = SkipValueCore(source, position, 0, path, out kind);
        source.RecordScan(end - Math.Min(original, valueStart));
        return end;
    }

    public static int SkipValueAtDepth(JsonSource source, int position, int depth, string path, out XFEJsonValueKind kind)
    {
        var original = position;
        SkipWhitespace(source.Text, ref position);
        if (position >= source.Text.Length)
            throw source.Error("缺少 JSON 值。", position, path);
        var end = SkipValueCore(source, position, depth, path, out kind);
        source.RecordScan(end - original);
        return end;
    }

    public static int SkipWhitespace(string text, int position)
    {
        SkipWhitespace(text, ref position);
        return position;
    }

    public static XFEJsonValueKind Classify(JsonSource source, int position, string path, out int valueStart)
    {
        valueStart = SkipWhitespace(source.Text, position);
        if (valueStart >= source.Text.Length)
            throw source.Error("缺少 JSON 值。", valueStart, path);

        return source.Text[valueStart] switch
        {
            '{' => XFEJsonValueKind.Object,
            '[' => XFEJsonValueKind.Array,
            '"' => XFEJsonValueKind.String,
            't' => XFEJsonValueKind.True,
            'f' => XFEJsonValueKind.False,
            'n' => XFEJsonValueKind.Null,
            '-' or >= '0' and <= '9' => XFEJsonValueKind.Number,
            _ => throw source.Error($"字符“{source.Text[valueStart]}”不能作为 JSON 值的开头。", valueStart, path)
        };
    }

    public static string ReadString(JsonSource source, int position, string path, out int end)
    {
        var text = source.Text;
        if (position >= text.Length || text[position] != '"')
            throw source.Error("应为 JSON 字符串。", position, path);

        var builder = new StringBuilder();
        var index = position + 1;
        while (index < text.Length)
        {
            var current = text[index++];
            if (current == '"')
            {
                end = index;
                source.RecordScan(end - position);
                return builder.ToString();
            }

            if (current < 0x20)
                throw source.Error("JSON 字符串中包含未转义的控制字符。", index - 1, path);

            if (current == '\\')
            {
                if (index >= text.Length)
                    throw source.Error("JSON 字符串的转义序列不完整。", index - 1, path);
                var escaped = text[index++];
                switch (escaped)
                {
                    case '"': builder.Append('"'); break;
                    case '\\': builder.Append('\\'); break;
                    case '/': builder.Append('/'); break;
                    case 'b': builder.Append('\b'); break;
                    case 'f': builder.Append('\f'); break;
                    case 'n': builder.Append('\n'); break;
                    case 'r': builder.Append('\r'); break;
                    case 't': builder.Append('\t'); break;
                    case 'u':
                        {
                            var first = ReadHexCodeUnit(source, ref index, path);
                            if (char.IsHighSurrogate((char)first))
                            {
                                if (index + 1 >= text.Length || text[index] != '\\' || text[index + 1] != 'u')
                                    throw source.Error("高代理项之后缺少低代理项。", index, path);
                                index += 2;
                                var second = ReadHexCodeUnit(source, ref index, path);
                                if (!char.IsLowSurrogate((char)second))
                                    throw source.Error("高代理项之后不是合法的低代理项。", index - 4, path);
                                builder.Append((char)first);
                                builder.Append((char)second);
                            }
                            else if (char.IsLowSurrogate((char)first))
                            {
                                throw source.Error("JSON 字符串包含孤立的低代理项。", index - 4, path);
                            }
                            else
                            {
                                builder.Append((char)first);
                            }
                            break;
                        }
                    default:
                        throw source.Error($"不支持的 JSON 转义字符“\\{escaped}”。", index - 2, path);
                }
                continue;
            }

            if (char.IsHighSurrogate(current))
            {
                if (index >= text.Length || !char.IsLowSurrogate(text[index]))
                    throw source.Error("JSON 字符串包含孤立的高代理项。", index - 1, path);
                builder.Append(current);
                builder.Append(text[index++]);
            }
            else if (char.IsLowSurrogate(current))
            {
                throw source.Error("JSON 字符串包含孤立的低代理项。", index - 1, path);
            }
            else
            {
                builder.Append(current);
            }
        }

        throw source.Error("JSON 字符串缺少结束双引号。", text.Length, path);
    }

    public static string AppendPropertyPath(string parent, string propertyName)
    {
        if (propertyName.Length > 0 && IsIdentifierStart(propertyName[0]) && propertyName.All(IsIdentifierPart))
            return $"{parent}.{propertyName}";
        return $"{parent}['{propertyName.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("'", "\\'", StringComparison.Ordinal)}']";
    }

    private static bool IsIdentifierStart(char value) => char.IsLetter(value) || value is '_' or '$';
    private static bool IsIdentifierPart(char value) => char.IsLetterOrDigit(value) || value is '_' or '$';

    private static int SkipValueCore(JsonSource source, int position, int depth, string path, out XFEJsonValueKind kind)
    {
        var text = source.Text;
        if (position >= text.Length)
            throw source.Error("缺少 JSON 值。", position, path);

        switch (text[position])
        {
            case '{':
                kind = XFEJsonValueKind.Object;
                EnsureDepth(source, depth, position, path);
                return SkipComposite(source, position, depth, path, JsonContainerType.Object);
            case '[':
                kind = XFEJsonValueKind.Array;
                EnsureDepth(source, depth, position, path);
                return SkipComposite(source, position, depth, path, JsonContainerType.Array);
            case '"':
                kind = XFEJsonValueKind.String;
                return SkipString(source, position, path);
            case 't':
                kind = XFEJsonValueKind.True;
                return SkipLiteral(source, position, "true", path);
            case 'f':
                kind = XFEJsonValueKind.False;
                return SkipLiteral(source, position, "false", path);
            case 'n':
                kind = XFEJsonValueKind.Null;
                return SkipLiteral(source, position, "null", path);
            case '-':
            case >= '0' and <= '9':
                kind = XFEJsonValueKind.Number;
                return SkipNumber(source, position, path);
            default:
                throw source.Error($"字符“{text[position]}”不能作为 JSON 值的开头。", position, path);
        }
    }

    private static int SkipComposite(JsonSource source, int position, int baseDepth, string path, JsonContainerType rootType)
    {
        var text = source.Text;
        var frames = ArrayPool<ContainerFrame>.Shared.Rent(source.Settings.MaxDepth + 1);
        var frameCount = 1;
        frames[0] = new ContainerFrame(rootType, 0);
        var index = position + 1;

        try
        {
            while (frameCount > 0)
            {
                ref var frame = ref frames[frameCount - 1];
                SkipWhitespace(text, ref index);
                if (frame.Type == JsonContainerType.Object)
                {
                    switch (frame.State)
                    {
                        case 0: // First property name or end.
                            if (index >= text.Length)
                                throw source.Error("JSON 对象缺少结束大括号。", index, path);
                            if (text[index] == '}')
                            {
                                index++;
                                frameCount--;
                                continue;
                            }
                            if (text[index] != '"')
                                throw source.Error("JSON 对象属性名称必须是双引号字符串。", index, path);
                            index = SkipString(source, index, path);
                            frame.State = 1;
                            continue;
                        case 1: // Colon.
                            if (index >= text.Length || text[index] != ':')
                                throw source.Error("JSON 属性名称之后缺少冒号。", index, path);
                            index++;
                            frame.State = 2;
                            continue;
                        case 2: // Property value.
                            frame.State = 3;
                            ConsumeNestedValue(source, ref index, baseDepth, path, frames, ref frameCount);
                            continue;
                        case 3: // Comma or end.
                            if (index >= text.Length)
                                throw source.Error("JSON 对象缺少结束大括号。", index, path);
                            if (text[index] == '}')
                            {
                                index++;
                                frameCount--;
                                continue;
                            }
                            if (text[index] != ',')
                                throw source.Error("JSON 对象成员之后应为逗号或结束大括号。", index, path);
                            index++;
                            frame.State = 4;
                            continue;
                        case 4: // Property name after comma; end means trailing comma.
                            if (index >= text.Length)
                                throw source.Error("JSON 对象缺少结束大括号。", index, path);
                            if (text[index] == '}')
                                throw source.Error("JSON 对象不允许尾逗号。", index, path);
                            if (text[index] != '"')
                                throw source.Error("JSON 对象属性名称必须是双引号字符串。", index, path);
                            index = SkipString(source, index, path);
                            frame.State = 1;
                            continue;
                    }
                }
                else
                {
                    switch (frame.State)
                    {
                        case 0: // First element or end.
                            if (index >= text.Length)
                                throw source.Error("JSON 数组缺少结束方括号。", index, path);
                            if (text[index] == ']')
                            {
                                index++;
                                frameCount--;
                                continue;
                            }
                            frame.State = 1;
                            ConsumeNestedValue(source, ref index, baseDepth, path, frames, ref frameCount);
                            continue;
                        case 1: // Comma or end.
                            if (index >= text.Length)
                                throw source.Error("JSON 数组缺少结束方括号。", index, path);
                            if (text[index] == ']')
                            {
                                index++;
                                frameCount--;
                                continue;
                            }
                            if (text[index] != ',')
                                throw source.Error("JSON 数组元素之后应为逗号或结束方括号。", index, path);
                            index++;
                            frame.State = 2;
                            continue;
                        case 2: // Element after comma; end means trailing comma.
                            if (index >= text.Length)
                                throw source.Error("JSON 数组缺少结束方括号。", index, path);
                            if (text[index] == ']')
                                throw source.Error("JSON 数组不允许尾逗号。", index, path);
                            frame.State = 1;
                            ConsumeNestedValue(source, ref index, baseDepth, path, frames, ref frameCount);
                            continue;
                    }
                }

                throw source.Error("JSON 容器扫描器进入无效状态。", index, path);
            }
            return index;
        }
        finally
        {
            Array.Clear(frames, 0, frameCount);
            ArrayPool<ContainerFrame>.Shared.Return(frames);
        }
    }

    private static void ConsumeNestedValue(
        JsonSource source,
        ref int position,
        int baseDepth,
        string path,
        ContainerFrame[] frames,
        ref int frameCount)
    {
        SkipWhitespace(source.Text, ref position);
        if (position >= source.Text.Length)
            throw source.Error("缺少 JSON 值。", position, path);

        var current = source.Text[position];
        if (current is '{' or '[')
        {
            EnsureDepth(source, baseDepth + frameCount, position, path);
            frames[frameCount++] = new ContainerFrame(
                current == '{' ? JsonContainerType.Object : JsonContainerType.Array,
                0);
            position++;
            return;
        }

        position = current switch
        {
            '"' => SkipString(source, position, path),
            't' => SkipLiteral(source, position, "true", path),
            'f' => SkipLiteral(source, position, "false", path),
            'n' => SkipLiteral(source, position, "null", path),
            '-' or >= '0' and <= '9' => SkipNumber(source, position, path),
            _ => throw source.Error($"字符“{current}”不能作为 JSON 值的开头。", position, path)
        };
    }

    private static int SkipString(JsonSource source, int position, string path)
    {
        var text = source.Text;
        var index = position + 1;
        while (index < text.Length)
        {
            var current = text[index++];
            if (current == '"')
                return index;
            if (current < 0x20)
                throw source.Error("JSON 字符串中包含未转义的控制字符。", index - 1, path);
            if (current == '\\')
            {
                if (index >= text.Length)
                    throw source.Error("JSON 字符串的转义序列不完整。", index - 1, path);
                var escaped = text[index++];
                if (escaped is '"' or '\\' or '/' or 'b' or 'f' or 'n' or 'r' or 't')
                    continue;
                if (escaped != 'u')
                    throw source.Error($"不支持的 JSON 转义字符“\\{escaped}”。", index - 2, path);
                var first = ReadHexCodeUnit(source, ref index, path);
                if (char.IsHighSurrogate((char)first))
                {
                    if (index + 1 >= text.Length || text[index] != '\\' || text[index + 1] != 'u')
                        throw source.Error("高代理项之后缺少低代理项。", index, path);
                    index += 2;
                    var second = ReadHexCodeUnit(source, ref index, path);
                    if (!char.IsLowSurrogate((char)second))
                        throw source.Error("高代理项之后不是合法的低代理项。", index - 4, path);
                }
                else if (char.IsLowSurrogate((char)first))
                {
                    throw source.Error("JSON 字符串包含孤立的低代理项。", index - 4, path);
                }
                continue;
            }

            if (char.IsHighSurrogate(current))
            {
                if (index >= text.Length || !char.IsLowSurrogate(text[index]))
                    throw source.Error("JSON 字符串包含孤立的高代理项。", index - 1, path);
                index++;
            }
            else if (char.IsLowSurrogate(current))
            {
                throw source.Error("JSON 字符串包含孤立的低代理项。", index - 1, path);
            }
        }

        throw source.Error("JSON 字符串缺少结束双引号。", text.Length, path);
    }

    private static int ReadHexCodeUnit(JsonSource source, ref int index, string path)
    {
        if (index + 4 > source.Text.Length)
            throw source.Error("Unicode 转义序列不足四位。", index, path);
        var value = 0;
        for (var count = 0; count < 4; count++)
        {
            var current = source.Text[index++];
            value = (value << 4) | current switch
            {
                >= '0' and <= '9' => current - '0',
                >= 'a' and <= 'f' => current - 'a' + 10,
                >= 'A' and <= 'F' => current - 'A' + 10,
                _ => throw source.Error("Unicode 转义序列包含非十六进制字符。", index - 1, path)
            };
        }
        return value;
    }

    private static int SkipLiteral(JsonSource source, int position, string literal, string path)
    {
        if (position + literal.Length > source.Text.Length ||
            !source.Text.AsSpan(position, literal.Length).SequenceEqual(literal.AsSpan()))
            throw source.Error($"无效的 JSON 字面量，应为“{literal}”。", position, path);
        return position + literal.Length;
    }

    private static int SkipNumber(JsonSource source, int position, string path)
    {
        var text = source.Text;
        var index = position;
        if (text[index] == '-')
        {
            index++;
            if (index >= text.Length)
                throw source.Error("负号之后缺少数字。", index, path);
        }

        if (text[index] == '0')
        {
            index++;
            if (index < text.Length && char.IsAsciiDigit(text[index]))
                throw source.Error("JSON 数字不允许前导零。", index, path);
        }
        else if (text[index] is >= '1' and <= '9')
        {
            do index++; while (index < text.Length && char.IsAsciiDigit(text[index]));
        }
        else
        {
            throw source.Error("JSON 数字的整数部分无效。", index, path);
        }

        if (index < text.Length && text[index] == '.')
        {
            index++;
            var fractionStart = index;
            while (index < text.Length && char.IsAsciiDigit(text[index])) index++;
            if (index == fractionStart)
                throw source.Error("小数点之后至少需要一位数字。", index, path);
        }

        if (index < text.Length && text[index] is 'e' or 'E')
        {
            index++;
            if (index < text.Length && text[index] is '+' or '-') index++;
            var exponentStart = index;
            while (index < text.Length && char.IsAsciiDigit(text[index])) index++;
            if (index == exponentStart)
                throw source.Error("指数部分至少需要一位数字。", index, path);
        }

        return index;
    }

    private static void EnsureDepth(JsonSource source, int depth, int position, string path)
    {
        if (depth >= source.Settings.MaxDepth)
            throw source.Error($"JSON 嵌套深度超过限制 {source.Settings.MaxDepth}。", position, path);
    }

    private static void SkipWhitespace(string text, ref int position)
    {
        while (position < text.Length && text[position] is ' ' or '\t' or '\r' or '\n')
            position++;
    }

    private enum JsonContainerType : byte
    {
        Object,
        Array
    }

    private struct ContainerFrame(JsonContainerType type, byte state)
    {
        public JsonContainerType Type = type;
        public byte State = state;
    }
}
