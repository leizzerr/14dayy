using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    const string VolumePrefKey = "MusicVolume";
    static readonly HashSet<string> MusicScenes = new HashSet<string> { "MainMenu", "SettingsMenu" };

    AudioSource source;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        source = GetComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = false;
        source.volume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);

        SceneManager.sceneLoaded += OnSceneLoaded;
        HandleScene(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this) Instance = null;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HandleScene(scene.name);
    }

    void HandleScene(string sceneName)
    {
        if (MusicScenes.Contains(sceneName))
        {
            if (!source.isPlaying) source.Play();
        }
        else
        {
            source.Stop();
            Destroy(gameObject);
        }
    }

    public static void SetVolume(float value)
    {
        PlayerPrefs.SetFloat(VolumePrefKey, value);
        if (Instance != null) Instance.source.volume = value;
    }

    public static float GetVolume()
    {
        return PlayerPrefs.GetFloat(VolumePrefKey, 1f);
    }
}
