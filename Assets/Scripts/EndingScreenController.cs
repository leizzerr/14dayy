using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndingScreenController : MonoBehaviour
{
    public static string PendingMessage = "";

    public Text messageText;
    public AudioSource musicSource;
    public float autoAdvanceDelay = 5f;

    float timer;
    bool advanced;

    void Start()
    {
        if (messageText != null) messageText.text = PendingMessage;

        if (musicSource != null)
        {
            musicSource.volume = MusicManager.GetVolume();
            musicSource.Play();
        }
    }

    void Update()
    {
        if (advanced) return;
        timer += Time.deltaTime;
        if (timer >= autoAdvanceDelay ||
            Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.Escape))
        {
            advanced = true;
            SceneManager.LoadScene("MainMenu");
        }
    }
}
