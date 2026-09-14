using UnityEngine;

// A solid trigger-free CircleCollider2D blocks the player like an invisible wall
// before they can reach the monster, plus a slightly larger trigger shows a
// one-time warning the first time the player gets close.
public class MonsterFearZone : MonoBehaviour
{
    public float blockRadius = 1.1f;
    public float warnRadius = 1.7f;
    public string warnTitle = "???";
    [TextArea] public string warningMessage =
        "Лучше не стоит подходить ближе. Это может быть очень опасно.";

    DialogueUI dialogueUI;
    bool warned;

    void Awake()
    {
        dialogueUI = FindFirstObjectByType<DialogueUI>();

        var blockCol = gameObject.AddComponent<CircleCollider2D>();
        blockCol.isTrigger = false;
        blockCol.radius = blockRadius;

        var warnCol = gameObject.AddComponent<CircleCollider2D>();
        warnCol.isTrigger = true;
        warnCol.radius = warnRadius;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (warned) return;
        if (other.GetComponent<PlayerMove>() == null) return;
        warned = true;
        dialogueUI?.ShowTimed(warnTitle, warningMessage, 3.5f);
    }
}
