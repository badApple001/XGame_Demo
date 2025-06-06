using UnityEngine;

public static class Vector3Extensions
{
    /// <summary> 返回a.x = x </summary>
    public static Vector3 WithX(this Vector3 a, float x)
    {
        a.x = x;
        return a;
    }

    /// <summary> 返回a.y = y </summary>
    public static Vector3 WithY(this Vector3 a, float y)
    {
        a.y = y;
        return a;
    }

    /// <summary> 返回a.z = z </summary>
    public static Vector3 WithZ(this Vector3 a, float z)
    {
        a.z = z;
        return a;
    }


    /// <summary> 通过float数组去获取 </summary>
    public static Vector3 FromArray(this Vector3 a, float[] float3Value)
    {
        if (float3Value == null || float3Value.Length < 3)
        {

#if UNITY_EDITOR
            throw new UnityException($"Vector3.FromArray 数组长度小于3: " + float3Value.Length);
#else
            Debug.LogError($"Vector3.FromArray 数组长度小于3: " + float3Value == null ? "nullptr" : float3Value.Length);
#endif
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                a[i] = float3Value[i];
            }
        }
        return a;
    }


    /// <summary> 忽略 Y 值，只比较 XZ 平面距离 </summary>
    public static float FlatDistance(this Vector3 a, Vector3 b)
    {
        a.y = b.y = 0;
        return Vector3.Distance(a, b);
    }

    /// <summary> 扁平化向量，保留 XZ，Y 设为 0 </summary>
    public static Vector3 Flatten(this Vector3 v)
    {
        return new Vector3(v.x, 0f, v.z);
    }

    /// <summary> 是否与目标方向大致一致（夹角小于一定角度）</summary>
    public static bool IsSameDirection(this Vector3 a, Vector3 b, float toleranceDegrees = 5f)
    {
        return Vector3.Angle(a, b) <= toleranceDegrees;
    }

    /// <summary> 判断是否几乎相等（避免精度误差）</summary>
    public static bool Approximately(this Vector3 a, Vector3 b, float epsilon = 0.0001f)
    {
        return (a - b).sqrMagnitude < epsilon * epsilon;
    }

    /// <summary> 向目标移动，但不超过最大距离 </summary>
    public static Vector3 MoveTowardsClamp(this Vector3 current, Vector3 target, float maxDistance)
    {
        return Vector3.Distance(current, target) <= maxDistance ? target : Vector3.MoveTowards(current, target, maxDistance);
    }

    /// <summary> 将向量投影到某个平面上 </summary>
    public static Vector3 ProjectOnPlane(this Vector3 vector, Vector3 planeNormal)
    {
        return vector - Vector3.Dot(vector, planeNormal.normalized) * planeNormal.normalized;
    }

    /// <summary> 找到点在指定线段 AB 上的最近点 </summary>
    public static Vector3 ClosestPointOnLine(this Vector3 point, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(point - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        return a + t * ab;
    }

    /// <summary> 替代 Lerp，避免加速慢停，用平滑曲线过渡 </summary>
    public static Vector3 SmoothDampTo(this Vector3 current, Vector3 target, ref Vector3 velocity, float smoothTime)
    {
        return Vector3.SmoothDamp(current, target, ref velocity, smoothTime);
    }

    /// <summary> 判断是否在给定距离范围内 </summary>
    public static bool IsInRange(this Vector3 a, Vector3 b, float range)
    {
        return (a - b).sqrMagnitude <= range * range;
    }
}
