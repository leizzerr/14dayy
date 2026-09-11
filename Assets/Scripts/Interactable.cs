using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class Interactable : MonoBehaviour
{
    public string displayName = "Предмет";
    [TextArea] public string description = "Здесь ничего интересного.";
    public float interactRadius = 0.6f;

    void Reset()
    {
        var col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = interactRadius;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerMove>() == null) return;
        if (InteractionManager.Instance != null) InteractionManager.Instance.Register(this);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerMove>() == null) return;
        if (InteractionManager.Instance != null) InteractionManager.Instance.Unregister(this);
    }
}
