using System.Collections.Generic;
using UnityEngine;

public static class RandomUtils
{
    /// <summary>
    /// 从 List 中随机选择一个元素（均匀分布）
    /// </summary>
    public static T PickRandom<T>( this List<T> list )
    {
        if ( list == null || list.Count == 0 )
        {
            Debug.LogError( "List is null or empty. Must contain at least one element." );
            return default;
        }

        int index = Random.Range( 0, list.Count ); // [0, Count)
        return list[ index ];
    }
}
