using UnityEngine;

// Plays a short two-line intro the moment 1room loads: the character's own
// confused/scared reaction, followed by the sanity mechanic's rules. Reuses
// DialogueUI's single-slot queue, so it's dismissed the same way as any other
// dialogue (E/Esc/Space). The player can't move until both lines are read.
public class IntroSequence : MonoBehaviour
{
    public string firstTitle = "???";
    [TextArea] public string firstMessage =
        "Где я?.. Ничего не понимаю. Здесь очень страшно — нужно поскорее выбраться отсюда.";

    public string secondTitle = "Подсказка";
    [TextArea] public string secondMessage =
        "Если рассудок упадёт до нуля — всё кончено. А рядом с чудовищами он тает намного быстрее.";

    DialogueUI dialogueUI;
    PlayerMove playerMove;

    void Start()
    {
        dialogueUI = FindFirstObjectByType<DialogueUI>();
        playerMove = FindFirstObjectByType<PlayerMove>();
        if (dialogueUI == null) return;

        if (playerMove != null)
        {
            var rb = playerMove.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
            playerMove.enabled = false;
        }

        dialogueUI.Show(firstTitle, firstMessage);
        dialogueUI.QueueNext(secondTitle, secondMessage);
        dialogueUI.OnFullyClosed += HandleClosed;
    }

    void HandleClosed()
    {
        if (playerMove != null) playerMove.enabled = true;
        if (dialogueUI != null) dialogueUI.OnFullyClosed -= HandleClosed;
    }
}
