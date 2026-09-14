using UnityEngine;

public static class GameTimer
{
    public static float StartTime;
    public static float FinalElapsed;
    public static bool HasResult;

    public static void Start()
    {
        StartTime = Time.time;
        HasResult = false;
        FinalElapsed = 0f;
    }

    public static void Finish()
    {
        FinalElapsed = Time.time - StartTime;
        HasResult = true;
    }
}
