using UnityEngine;

public static class SfxVolume
{
    const string PrefKey = "SfxVolume";

    public static float Get()
    {
        return PlayerPrefs.GetFloat(PrefKey, 1f);
    }

    public static void Set(float value)
    {
        PlayerPrefs.SetFloat(PrefKey, value);
    }
}
