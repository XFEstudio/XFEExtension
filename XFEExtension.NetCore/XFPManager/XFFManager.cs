namespace XFEExtension.NetCore.XFPManager;

/// <summary>
/// XFE字段管理器
/// </summary>
public class XFFManager
{
    private List<XField> Fields { get; set; }
    /// <summary>
    /// 通过索引器获取字段
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public object? this[string name]
    {
        get
        {
            foreach (var field in Fields)
            {
                if (field.name == name)
                {
                    return field.field;
                }
            }
            return null;
        }
        set
        {
            foreach (var field in Fields)
            {
                if (field.name == name)
                {
                    field.field = value;
                }
            }
        }
    }
    /// <summary>
    /// 通过名称获取给定类型的字段
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns>字段值</returns>
    public T? X<T>(string name)
    {
        foreach (var item in Fields)
        {
            if (item.name == name)
            {
                return item.field is null ? default : (T)item.field;
            }
        }
        return default;
    }
    /// <summary>
    /// 通过名称设置给定类型的字段
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public void E(string name, object value)
    {
        foreach (var item in Fields)
        {
            if (item.name == name)
            {
                item.field = value;
            }
        }
    }
    /// <summary>
    /// XFE字段管理器
    /// </summary>
    public XFFManager()
    {
        Fields = [];
    }
    /// <summary>
    /// XFE字段管理器
    /// </summary>
    /// <param name="nameAndValue">字段名称和其值</param>
    public XFFManager(params object[] nameAndValue)
    {
        Fields = [];
        for (int i = 0; i < nameAndValue.Length; i += 2)
        {
            Fields.Add(new XField() { name = nameAndValue[i].ToString(), field = nameAndValue[i + 1] });
        }
    }
}
