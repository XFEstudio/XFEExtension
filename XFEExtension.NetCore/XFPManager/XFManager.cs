namespace XFEExtension.NetCore.XFPManager;

/// <summary>
/// XFE字段属性管理器
/// </summary>
public class XFManager
{
    private List<XProperty> Properties { get; set; }
    private List<XField> Fields { get; set; }
    /// <summary>
    /// 通过索引器获取属性或字段
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public object? this[string name]
    {
        get
        {
            foreach (var field in Fields.Where(field => field.Name == name))
            {
                return field.Field;
            }
            return (from property in Properties where property.Name == name select property.Property).FirstOrDefault();
        }
        set
        {
            foreach (var field in Fields.Where(field => field.Name == name))
            {
                field.Field = value;
            }

            foreach (var property in Properties.Where(property => property.Name == name))
            {
                property.Property = value;
            }
        }
    }
    /// <summary>
    /// 通过名称获取给定类型的字段或属性
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public T? X<T>(string name)
    {
        foreach (var item in Properties.Where(item => item.Name == name))
        {
            return (T?)item.Property;
        }

        return default;
    }
    /// <summary>
    /// 通过名称设置给定类型的字段或属性
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public void E(string name, object value)
    {
        foreach (var item in Properties.Where(item => item.Name == name))
        {
            item.Property = value;
        }
    }
    /// <summary>
    /// XFE字段属性管理器
    /// </summary>
    public XFManager()
    {
        Properties = [];
        Fields = [];
    }
    /// <summary>
    /// XFE字段属性管理器
    /// </summary>
    /// <param name="nameAndValue">属性/字段名称和其值</param>
    /// <param name="isProperty">是否为属性</param>
    public XFManager(bool isProperty, params object[] nameAndValue)
    {
        Properties = [];
        Fields = [];
        if (isProperty)
        {
            for (var i = 0; i < nameAndValue.Length; i += 2)
            {
                Properties.Add(new XProperty { Name = nameAndValue[i].ToString(), Property = nameAndValue[i + 1] });
            }
        }
        else
        {
            for (var i = 0; i < nameAndValue.Length; i += 2)
            {
                Fields.Add(new XField { Name = nameAndValue[i].ToString(), Field = nameAndValue[i + 1] });
            }
        }
    }
    /// <summary>
    /// XFE字段属性管理器
    /// </summary>
    /// <param name="nameAndValue">字段名称和其值</param>
    public XFManager(params object[] nameAndValue)
    {
        Properties = [];
        Fields = [];
        for (var i = 0; i < nameAndValue.Length; i += 2)
        {
            Fields.Add(new XField { Name = nameAndValue[i].ToString(), Field = nameAndValue[i + 1] });
        }
    }
}
