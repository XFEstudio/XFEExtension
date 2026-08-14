using System.Buffers;
using System.Collections;
using System.Globalization;
using System.Reflection;

namespace XFEExtension.NetCore.XFETransform.Json;

internal static class JsonCodec
{
    public static string Serialize(object? value, Type declaredType, JsonSettings settings)
    {
        using var writer = new PooledJsonWriter(settings);
        var activeReferences = new HashSet<object>(ReferenceEqualityComparer.Instance);
        WriteValue(writer, value, declaredType, settings, activeReferences, "$", 0);
        return writer.ToString();
    }

    public static object? Deserialize(XFEJsonNode node, Type targetType, JsonSettings settings)
    {
        ArgumentNullException.ThrowIfNull(targetType);
        try
        {
            return ReadValue(node, targetType, settings, 0);
        }
        catch (XFEJsonException)
        {
            throw;
        }
        catch (Exception exception) when (exception is FormatException or OverflowException or InvalidCastException or TargetInvocationException or MemberAccessException)
        {
            throw node.Error($"无法将 JSON 值反序列化为 {targetType.FullName}。", innerException: exception);
        }
    }

    private static void WriteValue(
        PooledJsonWriter writer,
        object? value,
        Type declaredType,
        JsonSettings settings,
        HashSet<object> activeReferences,
        string path,
        int depth)
    {
        if (value is null)
        {
            writer.Append("null");
            return;
        }

        var type = value.GetType();
        var nullableType = Nullable.GetUnderlyingType(type);
        if (nullableType is not null)
            type = nullableType;

        if (WriteScalar(writer, value, type, settings, path))
            return;

        if (depth >= settings.MaxDepth)
            throw JsonSource.ConversionError($"序列化深度超过限制 {settings.MaxDepth}。", path);

        var trackReference = !type.IsValueType;
        if (trackReference && !activeReferences.Add(value))
            throw JsonSource.ConversionError("检测到对象引用循环。", path);

        try
        {
            if (value is IDictionary dictionary)
            {
                WriteDictionary(writer, dictionary, settings, activeReferences, path, depth);
                return;
            }

            if (TryGetStringDictionaryInterface(type, out _) && value is IEnumerable genericDictionary)
            {
                WriteGenericDictionary(writer, genericDictionary, settings, activeReferences, path, depth);
                return;
            }

            if (value is IEnumerable enumerable)
            {
                WriteArray(writer, enumerable, settings, activeReferences, path, depth);
                return;
            }

            WriteObject(writer, value, type, settings, activeReferences, path, depth);
        }
        finally
        {
            if (trackReference)
                activeReferences.Remove(value);
        }
    }

    private static bool WriteScalar(PooledJsonWriter writer, object value, Type type, JsonSettings settings, string path)
    {
        if (type == typeof(string))
        {
            writer.WriteString((string)value, path);
            return true;
        }
        if (type == typeof(char))
        {
            writer.WriteString(value.ToString()!, path);
            return true;
        }
        if (type == typeof(bool))
        {
            writer.Append((bool)value ? "true" : "false");
            return true;
        }
        if (type.IsEnum)
        {
            if (settings.EnumFormat == XFEJsonEnumFormat.String)
                writer.WriteString(value.ToString()!, path);
            else
            {
                var underlyingValue = Convert.ChangeType(value, Enum.GetUnderlyingType(type), CultureInfo.InvariantCulture);
                writer.Append(((IFormattable)underlyingValue).ToString(null, CultureInfo.InvariantCulture)!);
            }
            return true;
        }
        if (type == typeof(DateTime))
        {
            writer.WriteString(((DateTime)value).ToString("O", CultureInfo.InvariantCulture), path);
            return true;
        }
        if (type == typeof(DateTimeOffset))
        {
            writer.WriteString(((DateTimeOffset)value).ToString("O", CultureInfo.InvariantCulture), path);
            return true;
        }
        if (type == typeof(TimeSpan))
        {
            writer.WriteString(((TimeSpan)value).ToString("c", CultureInfo.InvariantCulture), path);
            return true;
        }
        if (type == typeof(Guid))
        {
            writer.WriteString(((Guid)value).ToString("D", CultureInfo.InvariantCulture), path);
            return true;
        }
        if (type == typeof(IntPtr))
        {
            writer.Append(((nint)value).ToString(CultureInfo.InvariantCulture));
            return true;
        }
        if (type == typeof(UIntPtr))
        {
            writer.Append(((nuint)value).ToString(CultureInfo.InvariantCulture));
            return true;
        }

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.SByte:
            case TypeCode.Byte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
            case TypeCode.UInt32:
            case TypeCode.Int64:
            case TypeCode.UInt64:
            case TypeCode.Decimal:
                writer.Append(((IFormattable)value).ToString(null, CultureInfo.InvariantCulture)!);
                return true;
            case TypeCode.Single:
            {
                var number = (float)value;
                if (!float.IsFinite(number))
                    throw JsonSource.ConversionError("JSON 不支持 NaN 或无穷大浮点数。", path);
                writer.Append(number.ToString("R", CultureInfo.InvariantCulture));
                return true;
            }
            case TypeCode.Double:
            {
                var number = (double)value;
                if (!double.IsFinite(number))
                    throw JsonSource.ConversionError("JSON 不支持 NaN 或无穷大浮点数。", path);
                writer.Append(number.ToString("R", CultureInfo.InvariantCulture));
                return true;
            }
            default:
                return false;
        }
    }

    private static void WriteDictionary(
        PooledJsonWriter writer,
        IDictionary dictionary,
        JsonSettings settings,
        HashSet<object> activeReferences,
        string path,
        int depth)
    {
        writer.Append('{');
        var index = 0;
        foreach (DictionaryEntry entry in dictionary)
        {
            if (entry.Key is not string key)
                throw JsonSource.ConversionError("JSON 对象字典只支持 string 键。", path);
            writer.WriteItemPrefix(index++, depth + 1);
            writer.WriteString(key, path);
            writer.WriteNameSeparator();
            WriteValue(writer, entry.Value, entry.Value?.GetType() ?? typeof(object), settings, activeReferences,
                JsonScanner.AppendPropertyPath(path, key), depth + 1);
        }
        writer.WriteContainerEnd('}', index, depth);
    }

    private static void WriteGenericDictionary(
        PooledJsonWriter writer,
        IEnumerable dictionary,
        JsonSettings settings,
        HashSet<object> activeReferences,
        string path,
        int depth)
    {
        writer.Append('{');
        var index = 0;
        foreach (var entry in dictionary)
        {
            if (entry is null)
                continue;
            var entryType = entry.GetType();
            var key = entryType.GetProperty("Key")?.GetValue(entry) as string
                ?? throw JsonSource.ConversionError("JSON 对象字典只支持 string 键。", path);
            var entryValue = entryType.GetProperty("Value")?.GetValue(entry);
            writer.WriteItemPrefix(index++, depth + 1);
            writer.WriteString(key, path);
            writer.WriteNameSeparator();
            WriteValue(writer, entryValue, entryValue?.GetType() ?? typeof(object), settings, activeReferences,
                JsonScanner.AppendPropertyPath(path, key), depth + 1);
        }
        writer.WriteContainerEnd('}', index, depth);
    }

    private static void WriteArray(
        PooledJsonWriter writer,
        IEnumerable values,
        JsonSettings settings,
        HashSet<object> activeReferences,
        string path,
        int depth)
    {
        writer.Append('[');
        var index = 0;
        foreach (var item in values)
        {
            writer.WriteItemPrefix(index, depth + 1);
            WriteValue(writer, item, item?.GetType() ?? typeof(object), settings, activeReferences, $"{path}[{index}]", depth + 1);
            index++;
        }
        writer.WriteContainerEnd(']', index, depth);
    }

    private static void WriteObject(
        PooledJsonWriter writer,
        object value,
        Type type,
        JsonSettings settings,
        HashSet<object> activeReferences,
        string path,
        int depth)
    {
        var contract = JsonContractCache.Get(type, settings);
        writer.Append('{');
        var index = 0;
        foreach (var member in contract.Members)
        {
            object? memberValue;
            try
            {
                memberValue = member.Getter(value);
            }
            catch (Exception exception)
            {
                throw JsonSource.ConversionError($"读取成员 {member.Name} 失败。", JsonScanner.AppendPropertyPath(path, member.Name), exception);
            }
            writer.WriteItemPrefix(index++, depth + 1);
            writer.WriteString(member.Name, path);
            writer.WriteNameSeparator();
            WriteValue(writer, memberValue, member.MemberType, settings, activeReferences,
                JsonScanner.AppendPropertyPath(path, member.Name), depth + 1);
        }
        writer.WriteContainerEnd('}', index, depth);
    }

    private static object? ReadValue(XFEJsonNode node, Type targetType, JsonSettings settings, int depth)
    {
        if (depth > settings.MaxDepth)
            throw node.Error($"反序列化深度超过限制 {settings.MaxDepth}。");

        var nullableType = Nullable.GetUnderlyingType(targetType);
        if (node.Kind == XFEJsonValueKind.Null)
        {
            node.Validate();
            if (!targetType.IsValueType || nullableType is not null)
                return null;
            throw node.Error($"不能将 JSON null 赋给非可空类型 {targetType.FullName}。");
        }

        if (nullableType is not null)
            return ReadValue(node, nullableType, settings, depth);
        if (targetType == typeof(XFEJsonNode))
            return node;
        if (targetType == typeof(object))
            return ReadUntyped(node, settings, depth);
        if (targetType == typeof(string))
            return node.GetString();
        if (targetType == typeof(char))
        {
            var value = node.GetString();
            if (value is { Length: 1 })
                return value[0];
            throw node.Error("char 目标要求 JSON 字符串恰好包含一个 UTF-16 字符。");
        }
        if (targetType == typeof(bool))
            return node.GetBoolean();
        if (targetType.IsEnum)
            return ReadEnum(node, targetType, settings);
        if (IsNumericType(targetType))
            return ReadNumber(node, targetType);
        if (targetType == typeof(DateTime))
            return ParseDateTime(node);
        if (targetType == typeof(DateTimeOffset))
            return ParseDateTimeOffset(node);
        if (targetType == typeof(TimeSpan))
            return ParseTimeSpan(node);
        if (targetType == typeof(Guid))
            return ParseGuid(node);
        if (targetType.IsArray)
            return ReadArray(node, targetType.GetElementType()!, settings, depth + 1);
        if (TryGetStringDictionaryInterface(targetType, out var dictionaryValueType))
            return ReadDictionary(node, targetType, dictionaryValueType, settings, depth + 1);
        if (TryGetListElementType(targetType, out var elementType))
            return ReadList(node, targetType, elementType, settings, depth + 1);

        return ReadObject(node, targetType, settings, depth + 1);
    }

    private static object? ReadUntyped(XFEJsonNode node, JsonSettings settings, int depth)
    {
        return node.Kind switch
        {
            XFEJsonValueKind.Null => null,
            XFEJsonValueKind.String => node.GetString(),
            XFEJsonValueKind.True or XFEJsonValueKind.False => node.GetBoolean(),
            XFEJsonValueKind.Number => ReadUntypedNumber(node),
            XFEJsonValueKind.Array => node.EnumerateArray().Select(item => ReadUntyped(item, settings, depth + 1)).ToList(),
            XFEJsonValueKind.Object => ReadUntypedObject(node, settings, depth + 1),
            _ => throw node.Error("无法识别 JSON 值类型。")
        };
    }

    private static object ReadUntypedNumber(XFEJsonNode node)
    {
        var span = RawSpan(node);
        if (long.TryParse(span, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var integer))
            return integer;
        if (decimal.TryParse(span, NumberStyles.Float, CultureInfo.InvariantCulture, out var decimalValue))
            return decimalValue;
        if (double.TryParse(span, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleValue) && double.IsFinite(doubleValue))
            return doubleValue;
        throw node.Error("JSON 数字超出受支持的动态数值范围。") ;
    }

    private static Dictionary<string, object?> ReadUntypedObject(XFEJsonNode node, JsonSettings settings, int depth)
    {
        var result = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var property in node.EnumerateObject())
            result.TryAdd(property.Name, ReadUntyped(property.Value, settings, depth));
        return result;
    }

    private static object ReadEnum(XFEJsonNode node, Type enumType, JsonSettings settings)
    {
        if (node.Kind == XFEJsonValueKind.String)
        {
            var name = node.GetString()!;
            if (Enum.TryParse(enumType, name, settings.PropertyNameCaseInsensitive, out var result))
                return result!;
            throw node.Error($"“{name}”不是 {enumType.FullName} 的有效枚举名称。");
        }
        var underlying = Enum.GetUnderlyingType(enumType);
        var number = ReadNumber(node, underlying);
        return Enum.ToObject(enumType, number);
    }

    private static object ReadNumber(XFEJsonNode node, Type type)
    {
        if (node.Kind != XFEJsonValueKind.Number)
            throw node.Error($"当前节点不是数字，无法转换为 {type.FullName}。");
        var span = RawSpan(node);
        try
        {
            if (type == typeof(sbyte)) return sbyte.Parse(span, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
            if (type == typeof(byte)) return byte.Parse(span, NumberStyles.None, CultureInfo.InvariantCulture);
            if (type == typeof(short)) return short.Parse(span, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
            if (type == typeof(ushort)) return ushort.Parse(span, NumberStyles.None, CultureInfo.InvariantCulture);
            if (type == typeof(int)) return int.Parse(span, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
            if (type == typeof(uint)) return uint.Parse(span, NumberStyles.None, CultureInfo.InvariantCulture);
            if (type == typeof(long)) return long.Parse(span, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
            if (type == typeof(ulong)) return ulong.Parse(span, NumberStyles.None, CultureInfo.InvariantCulture);
            if (type == typeof(nint)) return (nint)long.Parse(span, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
            if (type == typeof(nuint)) return (nuint)ulong.Parse(span, NumberStyles.None, CultureInfo.InvariantCulture);
            if (type == typeof(decimal)) return decimal.Parse(span, NumberStyles.Float, CultureInfo.InvariantCulture);
            if (type == typeof(float))
            {
                var value = float.Parse(span, NumberStyles.Float, CultureInfo.InvariantCulture);
                if (!float.IsFinite(value)) throw new OverflowException();
                return value;
            }
            if (type == typeof(double))
            {
                var value = double.Parse(span, NumberStyles.Float, CultureInfo.InvariantCulture);
                if (!double.IsFinite(value)) throw new OverflowException();
                return value;
            }
        }
        catch (Exception exception) when (exception is FormatException or OverflowException)
        {
            throw node.Error($"JSON 数字无法转换为 {type.FullName}。", innerException: exception);
        }
        throw node.Error($"不支持的数值类型 {type.FullName}。");
    }

    private static DateTime ParseDateTime(XFEJsonNode node)
    {
        var text = node.GetString();
        if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var value))
            return value;
        throw node.Error("JSON 字符串不是有效的 DateTime 往返格式。");
    }

    private static DateTimeOffset ParseDateTimeOffset(XFEJsonNode node)
    {
        var text = node.GetString();
        if (DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var value))
            return value;
        throw node.Error("JSON 字符串不是有效的 DateTimeOffset 往返格式。");
    }

    private static TimeSpan ParseTimeSpan(XFEJsonNode node)
    {
        var text = node.GetString();
        if (TimeSpan.TryParseExact(text, "c", CultureInfo.InvariantCulture, out var value))
            return value;
        throw node.Error("JSON 字符串不是有效的 TimeSpan 常量格式。");
    }

    private static Guid ParseGuid(XFEJsonNode node)
    {
        var text = node.GetString();
        if (Guid.TryParseExact(text, "D", out var value))
            return value;
        throw node.Error("JSON 字符串不是有效的 Guid D 格式。");
    }

    private static Array ReadArray(XFEJsonNode node, Type elementType, JsonSettings settings, int depth)
    {
        if (node.Kind != XFEJsonValueKind.Array)
            throw node.Error("目标数组要求 JSON 数组。");
        var elements = node.EnumerateArray().ToArray();
        var result = Array.CreateInstance(elementType, elements.Length);
        for (var index = 0; index < elements.Length; index++)
            result.SetValue(ReadValue(elements[index], elementType, settings, depth), index);
        return result;
    }

    private static object ReadList(XFEJsonNode node, Type targetType, Type elementType, JsonSettings settings, int depth)
    {
        if (node.Kind != XFEJsonValueKind.Array)
            throw node.Error($"类型 {targetType.FullName} 要求 JSON 数组。");
        var concreteType = targetType.IsInterface || targetType.IsAbstract
            ? typeof(List<>).MakeGenericType(elementType)
            : targetType;
        if (Activator.CreateInstance(concreteType) is not IList result)
            throw node.Error($"集合类型 {targetType.FullName} 必须具有公开无参构造并实现 IList。");
        foreach (var element in node.EnumerateArray())
            result.Add(ReadValue(element, elementType, settings, depth));
        return result;
    }

    private static object ReadDictionary(XFEJsonNode node, Type targetType, Type valueType, JsonSettings settings, int depth)
    {
        if (node.Kind != XFEJsonValueKind.Object)
            throw node.Error($"类型 {targetType.FullName} 要求 JSON 对象。");
        var concreteType = targetType.IsInterface || targetType.IsAbstract
            ? typeof(Dictionary<,>).MakeGenericType(typeof(string), valueType)
            : targetType;
        if (Activator.CreateInstance(concreteType) is not IDictionary result)
            throw node.Error($"字典类型 {targetType.FullName} 必须具有公开无参构造并实现 IDictionary。");
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in node.EnumerateObject())
        {
            if (seen.Add(property.Name))
                result.Add(property.Name, ReadValue(property.Value, valueType, settings, depth));
        }
        return result;
    }

    private static object ReadObject(XFEJsonNode node, Type targetType, JsonSettings settings, int depth)
    {
        if (node.Kind != XFEJsonValueKind.Object)
            throw node.Error($"类型 {targetType.FullName} 要求 JSON 对象。");
        var contract = JsonContractCache.Get(targetType, settings);
        if (contract.Factory is null)
            throw node.Error($"类型 {targetType.FullName} 必须是非抽象类型并具有公开无参构造。");
        var instance = contract.Factory();
        var seen = new HashSet<string>(settings.PropertyNameCaseInsensitive ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        foreach (var property in node.EnumerateObject())
        {
            if (!seen.Add(property.Name))
                continue;
            if (!contract.TryGetMember(property.Name, settings.PropertyNameCaseInsensitive, out var member) || member.Setter is null)
            {
                if (settings.UnmappedMemberHandling == XFEJsonUnmappedMemberHandling.Error)
                    throw property.Value.Error($"JSON 属性“{property.Name}”无法映射到 {targetType.FullName} 的可写成员。");
                continue;
            }

            var memberValue = ReadValue(property.Value, member.MemberType, settings, depth);
            try
            {
                member.Setter(instance, memberValue);
            }
            catch (Exception exception)
            {
                throw property.Value.Error($"设置成员 {targetType.FullName}.{member.Name} 失败。", innerException: exception);
            }
        }
        return instance;
    }

    private static ReadOnlySpan<char> RawSpan(XFEJsonNode node) =>
        node.Source.Text.AsSpan(node.Start, node.End - node.Start);

    private static bool IsNumericType(Type type) => type == typeof(sbyte) || type == typeof(byte) ||
        type == typeof(short) || type == typeof(ushort) || type == typeof(int) || type == typeof(uint) ||
        type == typeof(long) || type == typeof(ulong) || type == typeof(nint) || type == typeof(nuint) ||
        type == typeof(float) || type == typeof(double) || type == typeof(decimal);

    private static bool TryGetStringDictionaryInterface(Type type, out Type valueType)
    {
        foreach (var candidate in EnumerateTypeAndInterfaces(type))
        {
            if (!candidate.IsGenericType)
                continue;
            var definition = candidate.GetGenericTypeDefinition();
            if (definition != typeof(IDictionary<,>) && definition != typeof(IReadOnlyDictionary<,>) &&
                definition != typeof(Dictionary<,>))
                continue;
            var arguments = candidate.GetGenericArguments();
            if (arguments[0] != typeof(string))
                throw JsonSource.ConversionError($"JSON 对象字典不支持键类型 {arguments[0].FullName}。", "$");
            valueType = arguments[1];
            return true;
        }
        valueType = null!;
        return false;
    }

    private static bool TryGetListElementType(Type type, out Type elementType)
    {
        if (type == typeof(IList) || type == typeof(ArrayList))
        {
            elementType = typeof(object);
            return true;
        }
        foreach (var candidate in EnumerateTypeAndInterfaces(type))
        {
            if (!candidate.IsGenericType)
                continue;
            var definition = candidate.GetGenericTypeDefinition();
            if (definition == typeof(List<>) || definition == typeof(IList<>) ||
                definition == typeof(ICollection<>) || definition == typeof(IEnumerable<>) ||
                definition == typeof(IReadOnlyList<>) || definition == typeof(IReadOnlyCollection<>))
            {
                elementType = candidate.GetGenericArguments()[0];
                return true;
            }
        }
        elementType = null!;
        return false;
    }

    private static IEnumerable<Type> EnumerateTypeAndInterfaces(Type type)
    {
        yield return type;
        foreach (var interfaceType in type.GetInterfaces())
            yield return interfaceType;
    }
}

internal sealed class PooledJsonWriter(JsonSettings settings) : IDisposable
{
    private char[] _buffer = ArrayPool<char>.Shared.Rent(256);
    private int _length;

    public void Append(char value)
    {
        EnsureCapacity(1);
        _buffer[_length++] = value;
    }

    public void Append(string value)
    {
        EnsureCapacity(value.Length);
        value.AsSpan().CopyTo(_buffer.AsSpan(_length));
        _length += value.Length;
    }

    public void WriteString(string value, string path)
    {
        Append('"');
        for (var index = 0; index < value.Length; index++)
        {
            var current = value[index];
            switch (current)
            {
                case '"': Append("\\\""); break;
                case '\\': Append("\\\\"); break;
                case '\b': Append("\\b"); break;
                case '\f': Append("\\f"); break;
                case '\n': Append("\\n"); break;
                case '\r': Append("\\r"); break;
                case '\t': Append("\\t"); break;
                case < ' ':
                    Append("\\u");
                    Append(((int)current).ToString("X4", CultureInfo.InvariantCulture));
                    break;
                default:
                    if (char.IsHighSurrogate(current))
                    {
                        if (index + 1 >= value.Length || !char.IsLowSurrogate(value[index + 1]))
                            throw JsonSource.ConversionError("字符串包含孤立的高代理项。", path);
                        Append(current);
                        Append(value[++index]);
                    }
                    else if (char.IsLowSurrogate(current))
                    {
                        throw JsonSource.ConversionError("字符串包含孤立的低代理项。", path);
                    }
                    else
                    {
                        Append(current);
                    }
                    break;
            }
        }
        Append('"');
    }

    public void WriteItemPrefix(int index, int depth)
    {
        if (index > 0)
            Append(',');
        if (settings.WriteIndented)
            WriteNewLineAndIndent(depth);
    }

    public void WriteNameSeparator()
    {
        Append(':');
        if (settings.WriteIndented)
            Append(' ');
    }

    public void WriteContainerEnd(char close, int itemCount, int depth)
    {
        if (settings.WriteIndented && itemCount > 0)
            WriteNewLineAndIndent(depth);
        Append(close);
    }

    public override string ToString() => new(_buffer, 0, _length);

    public void Dispose()
    {
        var buffer = Interlocked.Exchange(ref _buffer, []);
        if (buffer.Length > 0)
            ArrayPool<char>.Shared.Return(buffer);
        _length = 0;
    }

    private void WriteNewLineAndIndent(int depth)
    {
        Append(Environment.NewLine);
        EnsureCapacity(depth * 2);
        for (var index = 0; index < depth * 2; index++)
            _buffer[_length++] = ' ';
    }

    private void EnsureCapacity(int additionalLength)
    {
        if (_length + additionalLength <= _buffer.Length)
            return;
        var newSize = Math.Max(_length + additionalLength, _buffer.Length * 2);
        var replacement = ArrayPool<char>.Shared.Rent(newSize);
        _buffer.AsSpan(0, _length).CopyTo(replacement);
        ArrayPool<char>.Shared.Return(_buffer);
        _buffer = replacement;
    }
}
