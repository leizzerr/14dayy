using UnityEngine;
using UnityEngine.UI;

public class GameTimerDisplay : MonoBehaviour
{
    public Text timerText;

    void Start()
    {
        if (timerText == null) return;

        if (GameTimer.HasResult)
        {
            int minutes = Mathf.FloorToInt(GameTimer.FinalElapsed / 60f);
            int seconds = Mathf.FloorToInt(GameTimer.FinalElapsed % 60f);
            timerText.text = $"Пройдено за {minutes:00}:{seconds:00}";
            timerText.gameObject.SetActive(true);
        }
        else
        {
            timerText.gameObject.SetActive(false);
        }
    }
}
