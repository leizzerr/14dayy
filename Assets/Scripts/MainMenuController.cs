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
        GameTimer.Start();
        SceneManager.LoadScene("1room");
    }

    public void OpenSettings()
    {
        SceneManager.LoadScene("SettingsMenu");
    }

    public void QuitGame()
    {
        // В редакторе выходим из режима воспроизведения
        if (Application.isEditor)
        {
            StopPlayMode();
            return;
        }

        // В Windows-сборке сворачиваем окно
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            ShowWindow(GetActiveWindow(), SW_MINIMIZE);
            return;
        }

        Application.Quit();
    }

    static void StopPlayMode()
    {
        // UnityEditor недоступен в сборке, поэтому обращаемся к нему через рефлексию
        var editorApplication = Type.GetType("UnityEditor.EditorApplication, UnityEditor");
        if (editorApplication == null) return;

        var isPlaying = editorApplication.GetProperty("isPlaying");
        if (isPlaying != null) isPlaying.SetValue(null, false);
    }
}