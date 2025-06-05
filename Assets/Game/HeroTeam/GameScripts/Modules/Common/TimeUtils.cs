using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TimeUtils
{
    public static float CurrentTime { get; private set; }
    public static float DeltaTime { get; private set; }
    public static float UnscaledTime { get; private set; }
    public static float UnscaledDeltaTime { get; private set; }
    public static int FrameCount { get; private set; }

    public static void Update()
    {
        CurrentTime = Time.time;
        DeltaTime = Time.deltaTime;
        UnscaledTime = Time.unscaledTime;
        UnscaledDeltaTime = Time.unscaledDeltaTime;
        FrameCount = Time.frameCount;
    }
}
