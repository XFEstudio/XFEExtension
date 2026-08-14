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

    /// <summary>
    /// 获取或设置指定索引处的子对象信息。
    /// </summary>
    /// <param name="index">要访问的子对象索引。</param>
    /// <returns>指定索引处的子对象信息。</returns>
    public IObjectInfo this[int index] { get => ObjectInfoList[index]; set => ObjectInfoList[index] = value; }

    /// <summary>
    /// 获取当前集合包含的子对象数量。
    /// </summary>
    public int Count => ObjectInfoList.Count;

    /// <summary>
    /// 获取一个值，指示当前子对象集合是否为只读；该实现始终返回 <see langword="false"/>。
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// 获取拥有当前子对象集合的父对象信息。
    /// </summary>
    public IObjectInfo Parent { get; init; }

    /// <summary>
    /// 将对象信息添加到子对象集合末尾。
    /// </summary>
    /// <param name="item">要添加的对象信息。</param>
    public void Add(IObjectInfo item) => ObjectInfoList.Add(item);

    /// <summary>
    /// 从当前集合中移除全部子对象信息。
    /// </summary>
    public void Clear() => ObjectInfoList.Clear();

    /// <summary>
    /// 确定当前集合是否包含指定的对象信息。
    /// </summary>
    /// <param name="item">要在集合中查找的对象信息。</param>
    /// <returns>集合包含指定对象信息时为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public bool Contains(IObjectInfo item) => ObjectInfoList.Contains(item);

    /// <summary>
    /// 从指定数组索引开始，将当前集合中的全部子对象复制到目标数组。
    /// </summary>
    /// <param name="array">接收子对象信息的目标数组。</param>
    /// <param name="arrayIndex">目标数组中开始写入的从零开始的索引。</param>
    public void CopyTo(IObjectInfo[] array, int arrayIndex) => ObjectInfoList.CopyTo(array, arrayIndex);

    /// <summary>
    /// 获取用于遍历当前子对象集合的泛型枚举器。
    /// </summary>
    /// <returns>当前子对象集合的泛型枚举器。</returns>
    public IEnumerator<IObjectInfo> GetEnumerator() => ObjectInfoList.GetEnumerator();

    /// <summary>
    /// 查找指定对象信息并返回其从零开始的索引。
    /// </summary>
    /// <param name="item">要在集合中查找的对象信息。</param>
    /// <returns>找到指定对象信息时返回其索引；否则返回 -1。</returns>
    public int IndexOf(IObjectInfo item) => ObjectInfoList.IndexOf(item);

    /// <summary>
    /// 将对象信息插入当前集合的指定索引处。
    /// </summary>
    /// <param name="index">插入对象信息的从零开始的索引。</param>
    /// <param name="item">要插入的对象信息。</param>
    public void Insert(int index, IObjectInfo item) => ObjectInfoList.Insert(index, item);

    /// <summary>
    /// 从当前集合中移除首次出现的指定对象信息。
    /// </summary>
    /// <param name="item">要移除的对象信息。</param>
    /// <returns>成功移除对象信息时为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public bool Remove(IObjectInfo item) => ObjectInfoList.Remove(item);

    /// <summary>
    /// 移除当前集合中指定索引处的子对象信息。
    /// </summary>
    /// <param name="index">要移除的子对象索引。</param>
    public void RemoveAt(int index) => ObjectInfoList.RemoveAt(index);

    IEnumerator IEnumerable.GetEnumerator() => ObjectInfoList.GetEnumerator();
}
