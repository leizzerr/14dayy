using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CircleCollider2D))]
public class Interactable : MonoBehaviour
{
    public string displayName = "Предмет";
    [TextArea] public string description = "Здесь ничего интересного.";
    public float interactRadius = 0.6f;
    public UnityEvent onExamine = new UnityEvent();

    static readonly Color HighlightTint = new Color(1.35f, 1.3f, 0.9f);

    SpriteRenderer sr;
    Color baseColor = Color.white;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) baseColor = sr.color;
    }

    void Reset()
    {
        var col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = interactRadius;
    }

    // Called by InteractionManager when this becomes (or stops being) the closest
    // in-range interactable, so the sprite visibly lights up before the player
    // even presses E — same treatment for every interactable (vases, doors, etc).
    public void SetHighlighted(bool on)
    {
        if (sr == null) return;
        sr.color = on ? HighlightTint : baseColor;
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
