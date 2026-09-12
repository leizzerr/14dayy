using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

public class MainMenuController : MonoBehaviour
{
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    [DllImport("user32.dll")] static extern System.IntPtr GetActiveWindow();
    [DllImport("user32.dll")] static extern bool ShowWindow(System.IntPtr hWnd, int nCmdShow);
    const int SW_MINIMIZE = 2;
#endif

    public void PlayGame()
    {
        SceneManager.LoadScene("Mockup_Gray");
    }

    public void OpenSettings()
    {
        SceneManager.LoadScene("SettingsMenu");
    }

    // Minimizes the window on Windows builds (matches the "свернуть" request).
    // No cross-platform window API exists in Unity, so other platforms/editor just quit/stop play mode.
    public void QuitGame()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        ShowWindow(GetActiveWindow(), SW_MINIMIZE);
#elif UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
