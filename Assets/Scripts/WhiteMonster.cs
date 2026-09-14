using UnityEngine;

// A cave monster that patrols a circular route (the "ring") and gives chase once it
// hears the player. Hearing range comes entirely from PlayerMove.CurrentNoiseRadius,
// which is small while the player sneaks (holds Shift) and large while walking normally
// — so staying quiet near the ring is how the player avoids waking it up.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class WhiteMonster : MonoBehaviour
{
    enum State { Patrol, Chase, Returning }

    [Header("Patrol ring (world units, cave2's big ring room)")]
    public Vector2 ringCenter = new Vector2(79.7f, 42.7f);
    public float ringRadius = 4.7f;
    public bool clockwise = true;
    public float patrolSpeed = 1.6f;
    [Tooltip("How strongly the monster corrects back onto the ring if physics nudges it off-radius.")]
    public float radialCorrection = 3f;

    [Header("Chase")]
    public float chaseSpeed = 3.4f;
    [Tooltip("Seconds without hearing the player, while chasing, before giving up.")]
    public float loseInterestTime = 4f;
    [Tooltip("Giving up immediately if the player ever gets this far away, heard or not.")]
    public float loseInterestDistance = 16f;

    [Header("Animation")]
    public Sprite[] frames;
    public float frameTime = 0.22f;

    Rigidbody2D rb;
    SpriteRenderer sr;
    PlayerMove player;
    GameOverUI gameOverUI;

    State state = State.Patrol;
    float angle;
    float sinceHeard;
    int frameIndex;
    float frameTimer;
    bool caught;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        player = FindFirstObjectByType<PlayerMove>();
        gameOverUI = FindFirstObjectByType<GameOverUI>();

        // Start wherever this GameObject was placed near the ring — its angle on the
        // ring is derived from its position, so moving it in the editor just works.
        Vector2 offset = (Vector2)transform.position - ringCenter;
        angle = offset.sqrMagnitude > 0.001f ? Mathf.Atan2(offset.y, offset.x) : 0f;
    }

    void FixedUpdate()
    {
        if (caught || player == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        UpdateHearing();

        switch (state)
        {
            case State.Patrol: TickPatrol(); break;
            case State.Chase: TickChase(); break;
            case State.Returning: TickReturning(); break;
        }
    }

    void UpdateHearing()
    {
        float dist = Vector2.Distance(transform.position, player.transform.position);
        bool heard = dist <= player.CurrentNoiseRadius;

        if (heard)
        {
            state = State.Chase;
            sinceHeard = 0f;
            return;
        }

        if (state != State.Chase) return;

        sinceHeard += Time.fixedDeltaTime;
        if (sinceHeard >= loseInterestTime || dist >= loseInterestDistance)
            state = State.Returning;
    }

    void TickPatrol()
    {
        float angularSpeed = patrolSpeed / Mathf.Max(0.1f, ringRadius);
        angle += (clockwise ? -1f : 1f) * angularSpeed * Time.fixedDeltaTime;

        Vector2 toCenter = ringCenter - (Vector2)transform.position;
        float currentRadius = toCenter.magnitude;
        Vector2 outward = currentRadius > 0.001f ? -toCenter.normalized : Vector2.right;
        Vector2 tangent = clockwise ? new Vector2(outward.y, -outward.x) : new Vector2(-outward.y, outward.x);

        // Gently pull back onto the exact ring radius so small physics nudges (e.g. brushing
        // a wall) don't let the patrol drift off-course over time.
        float radiusError = currentRadius - ringRadius;
        Vector2 correction = (currentRadius > 0.001f ? toCenter.normalized : Vector2.zero) * radiusError * radialCorrection;

        SetVelocity(tangent * patrolSpeed + correction);
    }

    void TickChase()
    {
        SetVelocity(((Vector2)player.transform.position - (Vector2)transform.position).normalized * chaseSpeed);
    }

    void TickReturning()
    {
        Vector2 toCenter = ringCenter - (Vector2)transform.position;
        Vector2 nearestOnRing = toCenter.magnitude > 0.001f
            ? ringCenter - toCenter.normalized * ringRadius
            : ringCenter + Vector2.right * ringRadius;

        Vector2 toRing = nearestOnRing - (Vector2)transform.position;
        SetVelocity(toRing.sqrMagnitude > 0.0001f ? toRing.normalized * patrolSpeed : Vector2.zero);

        if (toRing.magnitude < 0.35f)
        {
            Vector2 offset = (Vector2)transform.position - ringCenter;
            angle = Mathf.Atan2(offset.y, offset.x);
            state = State.Patrol;
        }
    }

    void SetVelocity(Vector2 velocity)
    {
        rb.linearVelocity = velocity;
        if (Mathf.Abs(velocity.x) > 0.01f) sr.flipX = velocity.x < 0f;
    }

    void Update()
    {
        if (frames == null || frames.Length == 0) return;

        // Legs step faster the faster it's actually moving (walk pace on patrol,
        // a quick scuttle while chasing) instead of a fixed cadence.
        float speed = rb.linearVelocity.magnitude;
        if (speed < 0.05f)
        {
            frameTimer = 0f;
            frameIndex = 0;
            sr.sprite = frames[0];
            return;
        }

        float speedRatio = speed / Mathf.Max(0.1f, patrolSpeed);
        float duration = frameTime / Mathf.Clamp(speedRatio, 0.4f, 3f);

        frameTimer += Time.deltaTime;
        if (frameTimer >= duration)
        {
            frameTimer = 0f;
            frameIndex = (frameIndex + 1) % frames.Length;
            sr.sprite = frames[frameIndex];
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (caught) return;
        var pm = collision.collider.GetComponent<PlayerMove>();
        if (pm == null) return;

        caught = true;
        rb.linearVelocity = Vector2.zero;

        var prb = pm.GetComponent<Rigidbody2D>();
        if (prb != null) prb.linearVelocity = Vector2.zero;
        pm.enabled = false;

        gameOverUI?.Show();
    }
}
