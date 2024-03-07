using System.Collections.Generic;

namespace XFEExtension.XFPManager
{
    internal class XProperty
    {
        private object property;
        public object Property
        {
            get { return property; }
            set { property = value; }
        }
        private string propertyName;
        public string Name
        {
            get { return propertyName; }
            set { propertyName = value; }
        }
    }
    internal class XField
    {
        public object Field;
        public string Name;
    }
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
        public object this[string name]
        {
            get
            {
                foreach (var field in Fields)
                {
                    if (field.Name == name)
                    {
                        return field.Field;
                    }
                }
                foreach (var property in Properties)
                {
                    if (property.Name == name)
                    {
                        return property.Property;
                    }
                }
                return null;
            }
            set
            {
                foreach (var field in Fields)
                {
                    if (field.Name == name)
                    {
                        field.Field = value;
                    }
                }
                foreach (var property in Properties)
                {
                    if (property.Name == name)
                    {
                        property.Property = value;
                    }
                }
            }
        }
        /// <summary>
        /// 通过名称获取给定类型的字段或属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public T X<T>(string name)
        {
            foreach (var item in Properties)
            {
                if (item.Name == name)
                {
                    return (T)item.Property;
                }
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
            foreach (var item in Properties)
            {
                if (item.Name == name)
                {
                    item.Property = value;
                }
            }
        }
        /// <summary>
        /// XFE字段属性管理器
        /// </summary>
        public XFManager()
        {
            Properties = new List<XProperty>();
            Fields = new List<XField>();
        }
        /// <summary>
        /// XFE字段属性管理器
        /// </summary>
        /// <param name="nameAndValue">属性/字段名称和其值</param>
        /// <param name="IsProperty">是否为属性</param>
        public XFManager(bool IsProperty, params object[] nameAndValue)
        {
            Properties = new List<XProperty>();
            Fields = new List<XField>();
            if (IsProperty)
            {
                for (int i = 0; i < nameAndValue.Length; i += 2)
                {
                    Properties.Add(new XProperty() { Name = nameAndValue[i].ToString(), Property = nameAndValue[i + 1] });
                }
            }
            else
            {
                for (int i = 0; i < nameAndValue.Length; i += 2)
                {
                    Fields.Add(new XField() { Name = nameAndValue[i].ToString(), Field = nameAndValue[i + 1] });
                }
            }
        }
        /// <summary>
        /// XFE字段属性管理器
        /// </summary>
        /// <param name="nameAndValue">字段名称和其值</param>
        public XFManager(params object[] nameAndValue)
        {
            Properties = new List<XProperty>();
            Fields = new List<XField>();
            for (int i = 0; i < nameAndValue.Length; i += 2)
            {
                Fields.Add(new XField() { Name = nameAndValue[i].ToString(), Field = nameAndValue[i + 1] });
            }
        }
    }
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
        public object this[string name]
        {
            get
            {
                foreach (var property in Properties)
                {
                    if (property.Name == name)
                    {
                        return property.Property;
                    }
                }
                return null;
            }
            set
            {
                foreach (var property in Properties)
                {
                    if (property.Name == name)
                    {
                        property.Property = value;
                    }
                }
            }
        }
        /// <summary>
        /// 通过名称获取给定类型的属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns>属性值</returns>
        public T X<T>(string name)
        {
            foreach (var item in Properties)
            {
                if (item.Name == name)
                {
                    return (T)item.Property;
                }
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
            foreach (var item in Properties)
            {
                if (item.Name == name)
                {
                    item.Property = value;
                }
            }
        }
        /// <summary>
        /// XFE属性管理器
        /// </summary>
        public XFPManager()
        {
            Properties = new List<XProperty>();
        }
        /// <summary>
        /// XFE属性管理器
        /// </summary>
        /// <param name="nameAndValue">属性名称和其值</param>
        public XFPManager(params object[] nameAndValue)
        {
            Properties = new List<XProperty>();
            for (int i = 0; i < nameAndValue.Length; i += 2)
            {
                Properties.Add(new XProperty() { Name = nameAndValue[i].ToString(), Property = nameAndValue[i + 1] });
            }
        }
    }
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
        public object this[string name]
        {
            get
            {
                foreach (var field in Fields)
                {
                    if (field.Name == name)
                    {
                        return field.Field;
                    }
                }
                return null;
            }
            set
            {
                foreach (var field in Fields)
                {
                    if (field.Name == name)
                    {
                        field.Field = value;
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
        public T X<T>(string name)
        {
            foreach (var item in Fields)
            {
                if (item.Name == name)
                {
                    return (T)item.Field;
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
                if (item.Name == name)
                {
                    item.Field = value;
                }
            }
        }
        /// <summary>
        /// XFE字段管理器
        /// </summary>
        public XFFManager()
        {
            Fields = new List<XField>();
        }
        /// <summary>
        /// XFE字段管理器
        /// </summary>
        /// <param name="nameAndValue">字段名称和其值</param>
        public XFFManager(params object[] nameAndValue)
        {
            Fields = new List<XField>();
            for (int i = 0; i < nameAndValue.Length; i += 2)
            {
                Fields.Add(new XField() { Name = nameAndValue[i].ToString(), Field = nameAndValue[i + 1] });
            }
        }
    }
}
