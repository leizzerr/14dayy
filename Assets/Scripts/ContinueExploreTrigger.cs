using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class ContinueExploreTrigger : MonoBehaviour
{
    public string hintDescription =
        "Проход ведёт дальше, вглубь пещеры. Можно продолжить исследование.";
    public string readyDescription =
        "Проход вглубь пещеры. Теперь можно идти. [Space] чтобы продолжить путь.";

    Interactable interactable;
    DialogueUI dialogueUI;
    bool playerInRange;

    void Awake()
    {
        interactable = GetComponent<Interactable>();
        dialogueUI = FindFirstObjectByType<DialogueUI>();
        interactable.displayName = "Проход вглубь";
        interactable.onExamine.AddListener(OnExamined);
        RefreshDescription();
    }

    void RefreshDescription()
    {
        var mgr = MockupEndingManager.Instance;
        interactable.description = (mgr != null && mgr.BothRead) ? readyDescription : hintDescription;
    }

    void OnExamined()
    {
        MockupEndingManager.Instance?.MarkContinueRead();
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
        // Same staleness issue as DoorExitTrigger: the door can flip BothRead without
        // this trigger being re-examined, so refresh every frame instead of only in
        // OnExamined.
        RefreshDescription();

        if (!playerInRange) return;
        var mgr = MockupEndingManager.Instance;
        if (mgr == null || !mgr.BothRead) return;
        if (Input.GetKeyDown(KeyCode.Space)) mgr.TriggerContinueEnding();
    }

    void LateUpdate()
    {
        var mgr = MockupEndingManager.Instance;
        if (playerInRange && mgr != null && mgr.BothRead)
            dialogueUI?.ShowPrompt("[Space] Идти дальше");
    }
}
