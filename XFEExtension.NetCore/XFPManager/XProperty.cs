namespace XFEExtension.NetCore.XFPManager;

internal class XProperty
{
    private object? property;
    public object? Property
    {
        get { return property; }
        set { property = value; }
    }
    private string? propertyName;
    public string? Name
    {
        get { return propertyName; }
        set { propertyName = value; }
    }
}
