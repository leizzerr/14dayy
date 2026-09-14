using UnityEngine;

[RequireComponent(typeof(Interactable))]
[RequireComponent(typeof(SceneDoor))]
public class DepthCheckDoor : MonoBehaviour
{
    public string blockedDescription =
        "Внутри слишком темно, дна совсем не видно. Страшно прыгать вниз — вдруг там очень " +
        "глубоко. Надо бы найти что-нибудь, что можно бросить туда, и проверить.";
    public string checkedDescription =
        "Ты бросаешь камень вниз. Через пару секунд слышен глухой стук о землю — не так уж " +
        "и глубоко. Можно спускаться.";

    Interactable interactable;
    SceneDoor sceneDoor;
    DialogueUI dialogueUI;
    bool checkedSafe;

    void Awake()
    {
        interactable = GetComponent<Interactable>();
        sceneDoor = GetComponent<SceneDoor>();
        dialogueUI = FindFirstObjectByType<DialogueUI>();

        sceneDoor.enabled = false;
        interactable.displayName = "Спуск вниз";
        interactable.description = blockedDescription;
        interactable.onExamine.AddListener(OnExamined);
    }

    void OnExamined()
    {
        if (checkedSafe) return;

        var inv = FindFirstObjectByType<PlayerInventory>();
        if (inv == null || !inv.hasDepthStone) return;

        inv.hasDepthStone = false;
        checkedSafe = true;
        interactable.description = checkedDescription;

        sceneDoor.enabled = true;
        sceneDoor.SetPlayerInRange(true);

        dialogueUI?.Show(interactable.displayName, checkedDescription);
    }
}
