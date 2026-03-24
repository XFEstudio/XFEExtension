namespace XFEExtension.NetCore.XFPManager;
/// <summary>
/// XFE属性管理器
/// </summary>
public class XFPManager
{
    private List<XProperty> Properties { get; set; }
    /// <summary>
    /// 通过索引器获取属性
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public object? this[string name]
    {
        get
        {
            return (from property in Properties where property.Name == name select property.Property).FirstOrDefault();
        }
        set
        {
            foreach (var property in Properties.Where(property => property.Name == name))
            {
                property.Property = value;
            }
        }
    }
    /// <summary>
    /// 通过名称获取给定类型的属性
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns>属性值</returns>
    public T? X<T>(string name)
    {
        foreach (var item in Properties.Where(item => item.Name == name))
        {
            return (T?)item.Property;
        }

        return default;
    }
    /// <summary>
    /// 通过名称设置给定类型的属性
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
    /// XFE属性管理器
    /// </summary>
    public XFPManager()
    {
        Properties = [];
    }
    /// <summary>
    /// XFE属性管理器
    /// </summary>
    /// <param name="nameAndValue">属性名称和其值</param>
    public XFPManager(params object[] nameAndValue)
    {
        Properties = [];
        for (var i = 0; i < nameAndValue.Length; i += 2)
        {
            Properties.Add(new XProperty { Name = nameAndValue[i].ToString()!, Property = nameAndValue[i + 1] });
        }
    }
}