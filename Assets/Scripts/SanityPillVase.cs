using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class SanityPillVase : MonoBehaviour
{
    public string foundDescription =
        "В вазе лежат таблетки от нервов. Ты забираешь их с собой: если рассудок " +
        "упадёт до нуля, ты автоматически проглотишь одну и он полностью восстановится.";
    public string emptyDescription = "Ваза пуста.";

    Interactable interactable;
    DialogueUI dialogueUI;
    bool collected;

    void Awake()
    {
        interactable = GetComponent<Interactable>();
        dialogueUI = FindFirstObjectByType<DialogueUI>();
        interactable.displayName = "Ваза";
        interactable.description = foundDescription;
        interactable.onExamine.AddListener(OnExamined);
    }

    void OnExamined()
    {
        if (collected) return;
        collected = true;

        var sanity = FindFirstObjectByType<SanityMeter>();
        sanity?.AddPill();

        interactable.description = emptyDescription;
        dialogueUI?.QueueNext("Таблетка от нервов", "Вы взяли её.");
    }
}
