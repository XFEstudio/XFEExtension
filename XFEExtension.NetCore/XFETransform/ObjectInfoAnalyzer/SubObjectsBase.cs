﻿using System.Collections;

namespace XFEExtension.NetCore.XFETransform.ObjectInfoAnalyzer;

internal abstract class SubObjectsBase(List<IObjectInfo>? objectInfoList, IObjectInfo parent) : ISubObjects
{
    protected readonly List<IObjectInfo> objectInfoList = objectInfoList ?? [];

    public IObjectInfo this[int index] { get => objectInfoList[index]; set => objectInfoList[index] = value; }

    public int Count => objectInfoList.Count;

    public bool IsReadOnly => false;

    public IObjectInfo Parent { get; init; } = parent;

    public void Add(IObjectInfo item) => objectInfoList.Add(item);

    public void Clear() => objectInfoList.Clear();

    public bool Contains(IObjectInfo item) => objectInfoList.Contains(item);

    public void CopyTo(IObjectInfo[] array, int arrayIndex) => objectInfoList.CopyTo(array, arrayIndex);

    public IEnumerator<IObjectInfo> GetEnumerator() => objectInfoList.GetEnumerator();

    public int IndexOf(IObjectInfo item) => objectInfoList.IndexOf(item);

    public void Insert(int index, IObjectInfo item) => objectInfoList.Insert(index, item);

    public bool Remove(IObjectInfo item) => objectInfoList.Remove(item);

    public void RemoveAt(int index) => objectInfoList.RemoveAt(index);

    IEnumerator IEnumerable.GetEnumerator() => objectInfoList.GetEnumerator();
}