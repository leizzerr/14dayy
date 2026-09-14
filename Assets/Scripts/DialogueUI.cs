using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    public GameObject panel;
    public Text titleText;
    public Text bodyText;
    public GameObject promptObject;
    public Text promptText;

    public bool IsOpen => panel != null && panel.activeSelf;

    float autoHideTimer = -1f;
    string queuedTitle;
    string queuedBody;
    bool hasQueued;

    void Awake()
    {
        if (panel != null) panel.SetActive(false);
    }

    void Update()
    {
        if (autoHideTimer < 0f) return;
        autoHideTimer -= Time.deltaTime;
        if (autoHideTimer <= 0f)
        {
            autoHideTimer = -1f;
            Hide();
        }
    }

    public void Show(string title, string body)
    {
        autoHideTimer = -1f;
        hasQueued = false;
        if (titleText != null) titleText.text = title;
        if (bodyText != null) bodyText.text = body;
        if (panel != null) panel.SetActive(true);
    }

    public void ShowTimed(string title, string body, float duration)
    {
        Show(title, body);
        autoHideTimer = duration;
    }

    // Shows right after the currently open dialogue is dismissed (E/Esc/Space, or
    // auto-hide timeout), instead of the panel actually closing. Used to chain a
    // "you picked it up" message after an item's description.
    public void QueueNext(string title, string body)
    {
        queuedTitle = title;
        queuedBody = body;
        hasQueued = true;
    }

    public void Hide()
    {
        autoHideTimer = -1f;
        if (hasQueued)
        {
            hasQueued = false;
            Show(queuedTitle, queuedBody);
            return;
        }
        if (panel != null) panel.SetActive(false);
    }

    public void ShowPrompt(string text)
    {
        if (promptText != null) promptText.text = text;
        if (promptObject != null) promptObject.SetActive(true);
    }

    public void HidePrompt()
    {
        if (promptObject != null) promptObject.SetActive(false);
    }
}
