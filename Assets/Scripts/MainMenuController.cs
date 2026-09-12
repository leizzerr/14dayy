using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [DllImport("user32.dll")] static extern IntPtr GetActiveWindow();
    [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    const int SW_MINIMIZE = 2;

    public void PlayGame()
    {
        SceneManager.LoadScene("Mockup_Gray");
    }

    public void OpenSettings()
    {
        SceneManager.LoadScene("SettingsMenu");
    }

    public void QuitGame()
    {
        // Свернуть окно можно только в Windows-сборке, на других платформах выходим из игры
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            ShowWindow(GetActiveWindow(), SW_MINIMIZE);
            return;
        }

        Application.Quit();
    }
}