using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class DoorExitTrigger : MonoBehaviour
{
    public string hesitateDescription =
        "Дверь наружу. Можно было бы уйти... но что-то останавливает — кажется, здесь ещё не всё осмотрено.";
    public string readyDescription =
        "Дверь наружу. Кажется, я осмотрел здесь всё. [Space] чтобы покинуть пещеру.";

    Interactable interactable;
    DialogueUI dialogueUI;
    bool playerInRange;

    void Awake()
    {
        interactable = GetComponent<Interactable>();
        dialogueUI = FindFirstObjectByType<DialogueUI>();
        interactable.displayName = "Дверь наружу";
        interactable.onExamine.AddListener(OnExamined);
        RefreshDescription();
    }

    void RefreshDescription()
    {
        var mgr = MockupEndingManager.Instance;
        interactable.description = (mgr != null && mgr.BothRead) ? readyDescription : hesitateDescription;
    }

    void OnExamined()
    {
        MockupEndingManager.Instance?.MarkDoorRead();
        RefreshDescription();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerMove>() != null) playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerMove>() != null) playerInRange = false;
    }

    void Update()
    {
        // The other exploration trigger (Ground_13_9) can flip MockupEndingManager's
        // BothRead flag without this door ever being re-examined — refresh here every
        // frame instead of only in OnExamined, so the door's text doesn't stay stale
        // ("still need to look at something") after the player already finished both.
        RefreshDescription();

        if (!playerInRange) return;
        var mgr = MockupEndingManager.Instance;
        if (mgr == null || !mgr.BothRead) return;
        if (Input.GetKeyDown(KeyCode.Space)) mgr.TriggerLeaveEnding();
    }

    // LateUpdate so this prompt wins over InteractionManager's per-frame hide
    // when nothing else is currently in examine range (see SceneDoor for the
    // same pattern).
    void LateUpdate()
    {
        var mgr = MockupEndingManager.Instance;
        if (playerInRange && mgr != null && mgr.BothRead)
            dialogueUI?.ShowPrompt("[Space] Покинуть пещеру");
    }
}
