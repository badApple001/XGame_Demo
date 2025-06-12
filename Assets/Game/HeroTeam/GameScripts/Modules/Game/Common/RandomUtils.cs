using System.Collections.Generic;
using UnityEngine;

public static class RandomUtils
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
}
