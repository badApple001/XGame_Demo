using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatUtils
{

    private static readonly System.Random s_Random = new System.Random();

    public static int ApplyRandomVariance(int baseValue, float variancePercent = 0.05f)
    {
        float min = 1f - variancePercent;
        float max = 1f + variancePercent;
        float factor = (float)(s_Random.NextDouble() * (max - min) + min);
        return Mathf.RoundToInt(baseValue * factor);
    }

}
