using System.Reflection;
using XFEExtension.NetCore.XFETransform.StringConverter;

namespace XFEExtension.NetCore.XFETransform.ObjectInfoAnalyzer;

internal class ObjectInfoImpl : ObjectInfo
{
    /// <summary>
    /// 创建不包含子对象集合的对象信息实现，适用于基础值或叶子节点。
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
    public ObjectInfoImpl(FieldInfo? fieldInfo, PropertyInfo? propertyInfo, IStringConverter? stringConverter, string? name, ObjectPlace objectPlace, int layer, Type? type, bool isBasicType, object? value = null) : base(fieldInfo, propertyInfo, stringConverter, name, objectPlace, layer, type, isBasicType, value)
    {
    }
    /// <summary>
    /// 创建包含子对象集合的对象信息实现，并将所给子对象关联到当前对象。
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
    public ObjectInfoImpl(FieldInfo? fieldInfo, PropertyInfo? propertyInfo, IStringConverter? stringConverter, string? name, ObjectPlace objectPlace, int layer, Type? type, bool isBasicType, bool isArray, object? value, List<IObjectInfo>? objectInfoList = null) : base(fieldInfo, propertyInfo, stringConverter, name, objectPlace, layer, type, isBasicType, isArray, value, objectInfoList)
    {
    }
}
