using UnityEngine;

// Attach to any sound-effect AudioSource (not music) so it scales with the
// separate "Громкость звуков" slider in Settings instead of the music one.
[RequireComponent(typeof(AudioSource))]
public class SfxSource : MonoBehaviour
{
    AudioSource source;
    float baseVolume;
    float lastAppliedSfxVolume = -1f;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        baseVolume = source.volume;
        Apply();
    }

    void Update()
    {
        // Cheap at this scale, and picks up live changes while the settings menu is open.
        if (!Mathf.Approximately(lastAppliedSfxVolume, SfxVolume.Get())) Apply();
    }

    void Apply()
    {
        lastAppliedSfxVolume = SfxVolume.Get();
        source.volume = baseVolume * lastAppliedSfxVolume;
    }
}
