using System.Collections.Generic;
using UnityEngine;

public static class ListExtend
{
    /// <summary>
    /// 随机抽选一个
    /// </summary>
    public static T PickRandom<T>(this List<T> list)
    {
        if (list == null || list.Count == 0)
        {
            Debug.LogError("List is null or empty. Must contain at least one element.");
            return default;
        }

        if (list.Count == 0) return list[0];

        int index = Random.Range(0, list.Count); // [0, Count)
        return list[index];
    }

    /// <summary>
    /// 重复添加一个元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="element"></param>
    /// <param name="count">添加多少个</param>
    public static void AddRepeat<T>(this List<T> list, T element, int count)
    {
        for (int i = 0; i < count; i++)
        {
            list.Add(element);
        }
    }
}
