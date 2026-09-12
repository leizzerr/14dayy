using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    public Slider musicVolumeSlider;

    void Start()
    {
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = MusicManager.GetVolume();
    }

    public void OnMusicVolumeChanged(float value)
    {
        MusicManager.SetVolume(value);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
