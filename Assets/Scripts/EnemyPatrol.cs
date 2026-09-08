using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    public float speed = 1.5f;
    public float patrolDistance = 3f;
    public Sprite[] walkFrames;
    public float frameTime = 0.18f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 startPos;
    private int direction = 1;
    private int frameIndex;
    private float frameTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        startPos = rb.position;
    }

    void FixedUpdate()
    {
        float offset = rb.position.x - startPos.x;
        if (offset >= patrolDistance && direction > 0) direction = -1;
        else if (offset <= 0f && direction < 0) direction = 1;

        rb.linearVelocity = new Vector2(direction * speed, 0f);
        sr.flipX = direction < 0;
    }

    void Update()
    {
        if (walkFrames == null || walkFrames.Length == 0) return;
        frameTimer += Time.deltaTime;
        if (frameTimer >= frameTime)
        {
            frameTimer = 0f;
            frameIndex = (frameIndex + 1) % walkFrames.Length;
            sr.sprite = walkFrames[frameIndex];
        }
    }
}
