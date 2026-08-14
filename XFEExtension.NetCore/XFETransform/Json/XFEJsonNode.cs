using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace XFEExtension.NetCore.XFETransform.Json;

/// <summary>
/// JSON 对象属性及其惰性值节点。
/// </summary>
/// <param name="Name">属性名称。</param>
/// <param name="Value">属性值节点。</param>
public readonly record struct XFEJsonProperty(string Name, XFEJsonNode Value);

/// <summary>
/// 引用源 JSON 文本范围的不可变惰性节点。
/// </summary>
public sealed class XFEJsonNode
{
    private readonly JsonSource _source;
    private readonly int _candidateStart;
    private readonly int _depth;
    private readonly bool _isRoot;
    private readonly string _path;
    private readonly object _stateGate = new();

    private int _valueStart = -1;
    private int _end;
    private XFEJsonValueKind _kind;
    private ObjectState? _objectState;
    private ArrayState? _arrayState;

    internal XFEJsonNode(
        JsonSource source,
        int candidateStart,
        int end,
        XFEJsonValueKind kind,
        string path,
        int depth,
        bool isRoot = false)
    {
        _source = source;
        _candidateStart = candidateStart;
        _end = end;
        _kind = kind;
        _path = path;
        _depth = depth;
        _isRoot = isRoot;
        if (kind != XFEJsonValueKind.Undefined)
            _valueStart = JsonScanner.SkipWhitespace(source.Text, candidateStart);
    }

    /// <summary>
    /// 当前节点的 JSON 值类型。读取此属性只检查值的首字符，不解析完整值。
    /// </summary>
    public XFEJsonValueKind Kind
    {
        get
        {
            EnsureHeader();
            return _kind;
        }
    }

    /// <summary>
    /// 当前节点是否为 JSON null。
    /// </summary>
    public bool IsNull => Kind == XFEJsonValueKind.Null;

    /// <summary>
    /// 当前对象属性数、数组元素数；标量节点读取时引发异常。
    /// </summary>
    public int Count => Kind switch
    {
        XFEJsonValueKind.Object => GetObjectSnapshot().Length,
        XFEJsonValueKind.Array => GetArraySnapshot().Length,
        _ => throw new InvalidOperationException("只有 JSON 对象或数组具有 Count。")
    };

    /// <summary>
    /// 按名称获取对象属性；不存在时返回 <see langword="null"/>。
    /// </summary>
    public XFEJsonNode? this[string propertyName]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(propertyName);
            return TryGetProperty(propertyName, out var value) ? value : null;
        }
    }

    /// <summary>
    /// 按零基索引获取数组元素；越界时返回 <see langword="null"/>。
    /// </summary>
    public XFEJsonNode? this[int index]
    {
        get => TryGetElement(index, out var value) ? value : null;
    }

    /// <summary>
    /// 尝试获取对象属性。
    /// </summary>
    public bool TryGetProperty(string propertyName, [NotNullWhen(true)] out XFEJsonNode? value)
    {
        ArgumentNullException.ThrowIfNull(propertyName);
        EnsureKind(XFEJsonValueKind.Object, "按名称访问属性");
        lock (_stateGate)
        {
            var state = GetOrCreateObjectState();
            if (state.FirstProperties.TryGetValue(propertyName, out value))
                return true;

            while (!state.Completed)
            {
                ScanNextObjectProperty(state, propertyName);
                if (state.FirstProperties.TryGetValue(propertyName, out value))
                    return true;
            }
        }

        value = null;
        return false;
    }

    /// <summary>
    /// 尝试获取数组元素。
    /// </summary>
    public bool TryGetElement(int index, [NotNullWhen(true)] out XFEJsonNode? value)
    {
        EnsureKind(XFEJsonValueKind.Array, "按索引访问元素");
        if (index < 0)
        {
            value = null;
            return false;
        }

        lock (_stateGate)
        {
            var state = GetOrCreateArrayState();
            while (state.Elements.Count <= index && !state.Completed)
                ScanNextArrayElement(state, index);
            if (index < state.Elements.Count)
            {
                value = state.Elements[index];
                return true;
            }
        }

        value = null;
        return false;
    }

    /// <summary>
    /// 读取并解码字符串；JSON null 返回 <see langword="null"/>。
    /// </summary>
    public string? GetString()
    {
        if (Kind == XFEJsonValueKind.Null)
        {
            EnsureEnd();
            return null;
        }
        EnsureKind(XFEJsonValueKind.String, "读取字符串");
        EnsureEnd();
        return JsonScanner.ReadString(_source, ValueStart, _path, out _);
    }

    /// <summary>
    /// 读取布尔值。
    /// </summary>
    public bool GetBoolean()
    {
        EnsureEnd();
        return Kind switch
        {
            XFEJsonValueKind.True => true,
            XFEJsonValueKind.False => false,
            _ => throw Error("当前节点不是布尔值。")
        };
    }

    /// <summary>
    /// 读取 Int32 数字。
    /// </summary>
    public int GetInt32() => GetNumber<int>(int.TryParse, nameof(Int32));
    /// <summary>
    /// 读取 Int64 数字。
    /// </summary>
    public long GetInt64() => GetNumber<long>(long.TryParse, nameof(Int64));
    /// <summary>
    /// 读取 UInt64 数字。
    /// </summary>
    public ulong GetUInt64() => GetNumber<ulong>(ulong.TryParse, nameof(UInt64));
    /// <summary>
    /// 读取 Single 数字。
    /// </summary>
    public float GetSingle() => GetFloatingNumber<float>(float.TryParse, nameof(Single));
    /// <summary>
    /// 读取 Double 数字。
    /// </summary>
    public double GetDouble() => GetFloatingNumber<double>(double.TryParse, nameof(Double));
    /// <summary>
    /// 读取 Decimal 数字。
    /// </summary>
    public decimal GetDecimal() => GetFloatingNumber<decimal>(decimal.TryParse, nameof(Decimal));

    /// <summary>
    /// 将当前节点反序列化为指定类型。
    /// </summary>
    public T? GetValue<T>() => Deserialize<T>();

    /// <summary>
    /// 尝试将当前节点转换为指定类型。
    /// </summary>
    public bool TryGetValue<T>([MaybeNullWhen(false)] out T value)
    {
        try
        {
            value = Deserialize<T>()!;
            return true;
        }
        catch (XFEJsonException)
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    /// 获取当前节点的原始 JSON 文本。
    /// </summary>
    public string GetRawText()
    {
        EnsureEnd();
        return _source.Text.Substring(ValueStart, _end - ValueStart);
    }

    /// <summary>
    /// 枚举对象属性。重复名称会按原顺序保留。
    /// </summary>
    public IEnumerable<XFEJsonProperty> EnumerateObject() => GetObjectSnapshot();

    /// <summary>
    /// 枚举数组元素。
    /// </summary>
    public IEnumerable<XFEJsonNode> EnumerateArray() => GetArraySnapshot();

    /// <summary>
    /// 从对象中选择指定属性；缺失属性对应的值为 <see langword="null"/>。
    /// </summary>
    public IReadOnlyDictionary<string, XFEJsonNode?> SelectProperties(params string[] propertyNames)
    {
        ArgumentNullException.ThrowIfNull(propertyNames);
        EnsureKind(XFEJsonValueKind.Object, "选择属性");
        var result = new Dictionary<string, XFEJsonNode?>(propertyNames.Length, StringComparer.Ordinal);
        foreach (var propertyName in propertyNames)
        {
            ArgumentNullException.ThrowIfNull(propertyName);
            result[propertyName] = this[propertyName];
        }
        return result;
    }

    /// <summary>
    /// 从数组内每个对象中惰性投影指定属性。
    /// </summary>
    public IEnumerable<IReadOnlyDictionary<string, XFEJsonNode?>> ProjectArray(params string[] propertyNames)
    {
        ArgumentNullException.ThrowIfNull(propertyNames);
        foreach (var element in EnumerateArray())
            yield return element.SelectProperties(propertyNames);
    }

    /// <summary>
    /// 将当前节点反序列化为指定类型。
    /// </summary>
    public T? Deserialize<T>(XFEJsonOptions? options = null)
    {
        var settings = options is null ? _source.Settings : JsonSettings.Create(options);
        return (T?)JsonCodec.Deserialize(this, typeof(T), settings);
    }

    /// <summary>
    /// 校验当前节点的完整 JSON 语法。根节点还会校验其后不存在额外内容。
    /// </summary>
    public void Validate()
    {
        if (_isRoot)
        {
            JsonScanner.ValidateDocument(_source, _path);
            return;
        }

        var end = JsonScanner.SkipValueAtDepth(_source, ValueStart, _depth, _path, out var kind);
        lock (_stateGate)
        {
            if (_end >= 0 && _end != end)
                throw Error("节点边界与 JSON 值边界不一致。", end);
            _end = end;
            _kind = kind;
        }
    }

    /// <summary>
    /// 返回当前节点的原始 JSON 文本。
    /// </summary>
    public override string ToString() => GetRawText();

    /// <summary>
    /// 从字符串惰性创建 JSON 节点。
    /// </summary>
    [return: NotNullIfNotNull(nameof(json))]
    public static implicit operator XFEJsonNode?(string? json) => json is null ? null : XFEJson.Parse(json);

    /// <summary>
    /// 链式访问对象属性。
    /// </summary>
    public static XFEJsonNode? operator >(XFEJsonNode? node, string propertyName) => node?[propertyName];
    /// <summary>
    /// 链式访问对象属性。
    /// </summary>
    public static XFEJsonNode? operator <(XFEJsonNode? node, string propertyName) => node?[propertyName];
    /// <summary>
    /// 链式访问数组元素。
    /// </summary>
    public static XFEJsonNode? operator >(XFEJsonNode? node, int index) => node?[index];
    /// <summary>
    /// 链式访问数组元素。
    /// </summary>
    public static XFEJsonNode? operator <(XFEJsonNode? node, int index) => node?[index];

    internal JsonSource Source => _source;
    internal int Start => ValueStart;
    internal int End
    {
        get
        {
            EnsureEnd();
            return _end;
        }
    }
    internal string Path => _path;
    internal JsonSettings Settings => _source.Settings;
    internal long ScannedCharacters => _source.ScannedCharacters;

    internal XFEJsonException Error(string message, int? position = null, Exception? innerException = null) =>
        _source.Error(message, position ?? Math.Max(ValueStart, 0), _path, innerException);

    private int ValueStart
    {
        get
        {
            EnsureHeader();
            return _valueStart;
        }
    }

    private void EnsureHeader()
    {
        if (_valueStart >= 0 && _kind != XFEJsonValueKind.Undefined)
        {
            EnsureContainerDepth();
            return;
        }
        lock (_stateGate)
        {
            if (_valueStart >= 0 && _kind != XFEJsonValueKind.Undefined)
            {
                EnsureContainerDepth();
                return;
            }
            _kind = JsonScanner.Classify(_source, _candidateStart, _path, out _valueStart);
            EnsureContainerDepth();
        }
    }

    private void EnsureContainerDepth()
    {
        if (_kind is XFEJsonValueKind.Object or XFEJsonValueKind.Array && _depth >= _source.Settings.MaxDepth)
            throw _source.Error($"JSON 嵌套深度超过限制 {_source.Settings.MaxDepth}。", Math.Max(_valueStart, 0), _path);
    }

    private void EnsureEnd()
    {
        if (_end >= 0)
            return;
        lock (_stateGate)
        {
            if (_end >= 0)
                return;
            _end = JsonScanner.SkipValueAtDepth(_source, ValueStart, _depth, _path, out var kind);
            _kind = kind;
        }
    }

    private void EnsureKind(XFEJsonValueKind expected, string operation)
    {
        if (Kind != expected)
            throw new InvalidOperationException($"无法对 {Kind} 节点执行“{operation}”；需要 {expected} 节点。");
    }

    private ObjectState GetOrCreateObjectState()
    {
        if (_objectState is not null)
            return _objectState;
        _objectState = new ObjectState(ValueStart + 1);
        return _objectState;
    }

    private ArrayState GetOrCreateArrayState()
    {
        if (_arrayState is not null)
            return _arrayState;
        _arrayState = new ArrayState(ValueStart + 1);
        return _arrayState;
    }

    private void ScanNextObjectProperty(ObjectState state, string? requestedPropertyName)
    {
        CompletePendingObjectValue(state);
        var text = _source.Text;
        var position = JsonScanner.SkipWhitespace(text, state.Cursor);
        if (!state.HasItems)
        {
            if (position < text.Length && text[position] == '}')
            {
                state.Completed = true;
                _end = position + 1;
                return;
            }
        }
        else
        {
            if (position >= text.Length)
                throw Error("JSON 对象缺少结束大括号。", position);
            if (text[position] == '}')
            {
                state.Completed = true;
                _end = position + 1;
                return;
            }
            if (text[position] != ',')
                throw Error("JSON 对象成员之后应为逗号或结束大括号。", position);
            position = JsonScanner.SkipWhitespace(text, position + 1);
            if (position < text.Length && text[position] == '}')
                throw Error("JSON 对象不允许尾逗号。", position);
        }

        if (position >= text.Length || text[position] != '"')
            throw Error("JSON 对象属性名称必须是双引号字符串。", position);
        var name = JsonScanner.ReadString(_source, position, _path, out position);
        position = JsonScanner.SkipWhitespace(text, position);
        if (position >= text.Length || text[position] != ':')
            throw Error("JSON 属性名称之后缺少冒号。", position);
        position = JsonScanner.SkipWhitespace(text, position + 1);
        var childPath = JsonScanner.AppendPropertyPath(_path, name);
        var kind = JsonScanner.Classify(_source, position, childPath, out var valueStart);
        var node = new XFEJsonNode(_source, valueStart, -1, kind, childPath, _depth + 1);
        var property = new XFEJsonProperty(name, node);
        state.OrderedProperties.Add(property);
        state.FirstProperties.TryAdd(name, node);
        state.HasItems = true;
        state.Cursor = valueStart;
        state.PendingValue = node;
        if (!string.Equals(name, requestedPropertyName, StringComparison.Ordinal))
            CompletePendingObjectValue(state);
    }

    private void ScanNextArrayElement(ArrayState state, int requestedIndex)
    {
        CompletePendingArrayValue(state);
        var text = _source.Text;
        var position = JsonScanner.SkipWhitespace(text, state.Cursor);
        if (!state.HasItems)
        {
            if (position < text.Length && text[position] == ']')
            {
                state.Completed = true;
                _end = position + 1;
                return;
            }
        }
        else
        {
            if (position >= text.Length)
                throw Error("JSON 数组缺少结束方括号。", position);
            if (text[position] == ']')
            {
                state.Completed = true;
                _end = position + 1;
                return;
            }
            if (text[position] != ',')
                throw Error("JSON 数组元素之后应为逗号或结束方括号。", position);
            position = JsonScanner.SkipWhitespace(text, position + 1);
            if (position < text.Length && text[position] == ']')
                throw Error("JSON 数组不允许尾逗号。", position);
        }

        var index = state.Elements.Count;
        var childPath = $"{_path}[{index}]";
        var kind = JsonScanner.Classify(_source, position, childPath, out var valueStart);
        var node = new XFEJsonNode(_source, valueStart, -1, kind, childPath, _depth + 1);
        state.Elements.Add(node);
        state.HasItems = true;
        state.Cursor = valueStart;
        state.PendingValue = node;
        if (index != requestedIndex)
            CompletePendingArrayValue(state);
    }

    private XFEJsonProperty[] GetObjectSnapshot()
    {
        EnsureKind(XFEJsonValueKind.Object, "枚举对象属性");
        lock (_stateGate)
        {
            var state = GetOrCreateObjectState();
            while (!state.Completed)
                ScanNextObjectProperty(state, null);
            return [.. state.OrderedProperties];
        }
    }

    private XFEJsonNode[] GetArraySnapshot()
    {
        EnsureKind(XFEJsonValueKind.Array, "枚举数组元素");
        lock (_stateGate)
        {
            var state = GetOrCreateArrayState();
            while (!state.Completed)
                ScanNextArrayElement(state, -1);
            return [.. state.Elements];
        }
    }

    private static void CompletePendingObjectValue(ObjectState state)
    {
        if (state.PendingValue is null)
            return;
        state.Cursor = state.PendingValue.End;
        state.PendingValue = null;
    }

    private static void CompletePendingArrayValue(ArrayState state)
    {
        if (state.PendingValue is null)
            return;
        state.Cursor = state.PendingValue.End;
        state.PendingValue = null;
    }

    private delegate bool IntegerParser<T>(ReadOnlySpan<char> text, NumberStyles styles, IFormatProvider? provider, out T value);
    private delegate bool FloatingParser<T>(ReadOnlySpan<char> text, NumberStyles styles, IFormatProvider? provider, out T value);

    private T GetNumber<T>(IntegerParser<T> parser, string typeName)
    {
        EnsureKind(XFEJsonValueKind.Number, $"读取 {typeName}");
        EnsureEnd();
        var span = _source.Text.AsSpan(ValueStart, _end - ValueStart);
        if (parser(span, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var result))
            return result;
        throw Error($"JSON 数字无法转换为 {typeName}。", ValueStart);
    }

    private T GetFloatingNumber<T>(FloatingParser<T> parser, string typeName)
    {
        EnsureKind(XFEJsonValueKind.Number, $"读取 {typeName}");
        EnsureEnd();
        var span = _source.Text.AsSpan(ValueStart, _end - ValueStart);
        if (parser(span, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
            return result;
        throw Error($"JSON 数字无法转换为 {typeName}。", ValueStart);
    }

    private sealed class ObjectState(int cursor)
    {
        public int Cursor { get; set; } = cursor;
        public bool HasItems { get; set; }
        public bool Completed { get; set; }
        public XFEJsonNode? PendingValue { get; set; }
        public Dictionary<string, XFEJsonNode> FirstProperties { get; } = new(StringComparer.Ordinal);
        public List<XFEJsonProperty> OrderedProperties { get; } = [];
    }

    private sealed class ArrayState(int cursor)
    {
        public int Cursor { get; set; } = cursor;
        public bool HasItems { get; set; }
        public bool Completed { get; set; }
        public XFEJsonNode? PendingValue { get; set; }
        public List<XFEJsonNode> Elements { get; } = [];
    }
}
