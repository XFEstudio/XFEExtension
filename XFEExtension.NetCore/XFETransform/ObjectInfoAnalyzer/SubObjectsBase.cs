using System.Collections;

namespace XFEExtension.NetCore.XFETransform.ObjectInfoAnalyzer;

internal abstract class SubObjectsBase : ISubObjects
{

    protected readonly List<IObjectInfo> ObjectInfoList;

    protected SubObjectsBase(List<IObjectInfo>? objectInfoList, IObjectInfo parent)
    {
        this.ObjectInfoList = objectInfoList ?? [];
        Parent = parent;
        objectInfoList?.ForEach(objectInfo => objectInfo.Parent = parent);
    }

    public IObjectInfo this[int index] { get => ObjectInfoList[index]; set => ObjectInfoList[index] = value; }

    public int Count => ObjectInfoList.Count;

    public bool IsReadOnly => false;

    public IObjectInfo Parent { get; init; }

    public void Add(IObjectInfo item) => ObjectInfoList.Add(item);

    public void Clear() => ObjectInfoList.Clear();

    public bool Contains(IObjectInfo item) => ObjectInfoList.Contains(item);

    public void CopyTo(IObjectInfo[] array, int arrayIndex) => ObjectInfoList.CopyTo(array, arrayIndex);

    public IEnumerator<IObjectInfo> GetEnumerator() => ObjectInfoList.GetEnumerator();

    public int IndexOf(IObjectInfo item) => ObjectInfoList.IndexOf(item);

    public void Insert(int index, IObjectInfo item) => ObjectInfoList.Insert(index, item);

    public bool Remove(IObjectInfo item) => ObjectInfoList.Remove(item);

    public void RemoveAt(int index) => ObjectInfoList.RemoveAt(index);

    IEnumerator IEnumerable.GetEnumerator() => ObjectInfoList.GetEnumerator();
}