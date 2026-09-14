using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class DepthStoneVase : MonoBehaviour
{
    public string foundDescription =
        "В вазе лежит увесистый камень. Такой можно бросить вниз, в темноту спуска, " +
        "чтобы проверить, насколько там глубоко.";
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

        var inv = FindFirstObjectByType<PlayerInventory>();
        if (inv != null) inv.hasDepthStone = true;

        interactable.description = emptyDescription;
        dialogueUI?.QueueNext("Камень", "Вы взяли его.");
    }
}
