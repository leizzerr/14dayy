using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    void Start()
    {
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = MusicManager.GetVolume();

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = SfxVolume.Get();
    }

    public void OnMusicVolumeChanged(float value)
    {
        MusicManager.SetVolume(value);
    }

    public void OnSfxVolumeChanged(float value)
    {
        SfxVolume.Set(value);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
