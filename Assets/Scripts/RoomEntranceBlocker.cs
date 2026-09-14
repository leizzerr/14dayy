using UnityEngine;

// Attach to any Collider2D placed at a room entrance (solid or trigger, doesn't
// matter which) to show a one-time warning when the player bumps into it.
public class RoomEntranceBlocker : MonoBehaviour
{
    public string warnTitle = "???";
    [TextArea] public string warningMessage = "Лучше не подходить ближе.";

    public string reflectionTitle = "???";
    [TextArea] public string reflectionMessage =
        "Хотя... он <color=red>красный</color> — такие вроде только пугают, вряд ли и правда что-то сделают. " +
        "Но даже представить страшно, что будет, если попадётся <color=white>белый</color>.";

    DialogueUI dialogueUI;
    bool warned;

    void Awake()
    {
        dialogueUI = FindFirstObjectByType<DialogueUI>();
    }

    void OnCollisionEnter2D(Collision2D collision) => TryWarn(collision.collider);
    void OnTriggerEnter2D(Collider2D other) => TryWarn(other);

    void TryWarn(Collider2D other)
    {
        if (warned) return;
        if (other.GetComponent<PlayerMove>() == null) return;
        warned = true;
        // Two chained messages via DialogueUI's queue: the bump warning shows first,
        // then the reflection on red-vs-white monsters once the player dismisses it.
        dialogueUI?.Show(warnTitle, warningMessage);
        dialogueUI?.QueueNext(reflectionTitle, reflectionMessage);
    }
}
