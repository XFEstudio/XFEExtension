using System.Reflection;
using XFEExtension.NetCore.XFETransform.StringConverter;

namespace XFEExtension.NetCore.XFETransform.ObjectInfoAnalyzer;

internal abstract class ObjectInfo : IObjectInfo
{
    /// <summary>
    /// 获取对象在其父级中的名称。
    /// </summary>
    public string? Name { get; init; }
    /// <summary>
    /// 获取对象相对于父级的成员位置类型。
    /// </summary>
    public ObjectPlace ObjectPlace { get; init; }
    /// <summary>
    /// 获取对象在分析结果树中的层级深度。
    /// </summary>
    public int Layer { get; init; }
    /// <summary>
    /// 获取当前对象所保存的原始值。
    /// </summary>
    public object? Value { get; init; }
    /// <summary>
    /// 获取当前对象的运行时类型。
    /// </summary>
    public Type? Type { get; init; }
    /// <summary>
    /// 获取当前对象是否可作为基础类型直接转换。
    /// </summary>
    public bool IsBasicType { get; init; }
    /// <summary>
    /// 获取当前对象是否表示数组。
    /// </summary>
    public bool IsArray { get; init; }
    /// <summary>
    /// 获取当前对象包含的子对象集合。
    /// </summary>
    public ISubObjects? SubObjects { get; init; }
    /// <summary>
    /// 获取负责输出当前对象信息的字符串转换器。
    /// </summary>
    public IStringConverter? StringConverter { get; init; }
    /// <summary>
    /// 获取当前对象来源字段的反射信息。
    /// </summary>
    public FieldInfo? FieldInfo { get; init; }
    /// <summary>
    /// 获取当前对象来源属性的反射信息。
    /// </summary>
    public PropertyInfo? PropertyInfo { get; init; }
    /// <summary>
    /// 获取或设置当前对象在分析结果树中的父对象。
    /// </summary>
    public IObjectInfo? Parent { get; set; }

    /// <summary>
    /// 使用当前对象配置的字符串转换器输出对象信息。
    /// </summary>
    /// <returns>字符串转换器生成的对象描述。</returns>
    public string OutPutObject()
    {
        return StringConverter!.OutPutObject(this);
    }

    /// <summary>
    /// 创建不包含子对象集合的对象信息，适用于基础值或叶子节点。
    /// </summary>
    /// <param name="fieldInfo">对象来源字段的反射信息。</param>
    /// <param name="propertyInfo">对象来源属性的反射信息。</param>
    /// <param name="stringConverter">用于输出对象信息的字符串转换器。</param>
    /// <param name="name">对象在其父级中的名称。</param>
    /// <param name="objectPlace">对象相对于父级的成员位置类型。</param>
    /// <param name="layer">对象在分析结果树中的层级深度。</param>
    /// <param name="type">对象的运行时类型。</param>
    /// <param name="isBasicType">对象是否可作为基础类型直接转换。</param>
    /// <param name="value">对象的原始值。</param>
    public ObjectInfo(FieldInfo? fieldInfo, PropertyInfo? propertyInfo, IStringConverter? stringConverter, string? name, ObjectPlace objectPlace, int layer, Type? type, bool isBasicType, object? value = null)
    {
        FieldInfo = fieldInfo;
        PropertyInfo = propertyInfo;
        StringConverter = stringConverter;
        Name = name;
        ObjectPlace = objectPlace;
        Layer = layer;
        Type = type;
        IsBasicType = isBasicType;
        IsArray = false;
        Value = value;
    }
    /// <summary>
    /// 创建包含子对象集合的对象信息，并将所给子对象关联到当前对象。
    /// </summary>
    /// <param name="fieldInfo">对象来源字段的反射信息。</param>
    /// <param name="propertyInfo">对象来源属性的反射信息。</param>
    /// <param name="stringConverter">用于输出对象信息的字符串转换器。</param>
    /// <param name="name">对象在其父级中的名称。</param>
    /// <param name="objectPlace">对象相对于父级的成员位置类型。</param>
    /// <param name="layer">对象在分析结果树中的层级深度。</param>
    /// <param name="type">对象的运行时类型。</param>
    /// <param name="isBasicType">对象是否可作为基础类型直接转换。</param>
    /// <param name="isArray">对象是否表示数组。</param>
    /// <param name="value">对象的原始值。</param>
    /// <param name="objectInfoList">作为当前对象子节点的对象信息列表。</param>
    public ObjectInfo(FieldInfo? fieldInfo, PropertyInfo? propertyInfo, IStringConverter? stringConverter, string? name, ObjectPlace objectPlace, int layer, Type? type, bool isBasicType, bool isArray, object? value = null, List<IObjectInfo>? objectInfoList = null)
    {
        FieldInfo = fieldInfo;
        PropertyInfo = propertyInfo;
        StringConverter = stringConverter;
        Name = name;
        ObjectPlace = objectPlace;
        Layer = layer;
        Type = type;
        IsBasicType = isBasicType;
        IsArray = isArray;
        Value = value;
        SubObjects = new SubObjectsImpl(this, objectInfoList);
    }
}
