using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PlayerMove))]
public class PlayerDirectionalSprite : MonoBehaviour
{
    [System.Serializable]
    public class DirectionalAnimation
    {
        public Sprite[] idle;
        public Sprite[] walk;
    }

    public DirectionalAnimation south;
    public DirectionalAnimation southEast;
    public DirectionalAnimation east;
    public DirectionalAnimation northEast;
    public DirectionalAnimation north;
    public DirectionalAnimation northWest;
    public DirectionalAnimation west;
    public DirectionalAnimation southWest;

    public float framesPerSecond = 8f;
    [Tooltip("Animation plays at framesPerSecond * this while the player sneaks, for a slower, careful gait.")]
    public float sneakFrameRateMultiplier = 0.5f;

    const float MoveThreshold = 0.1f;

    SpriteRenderer sr;
    PlayerMove playerMove;

    int currentOctant = 6; // south, matches the previous default facing
    bool wasMoving;
    float frameTimer;
    int frameIndex;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        playerMove = GetComponent<PlayerMove>();
        ApplyInitialSprite();
    }

    void LateUpdate()
    {
        var move = playerMove.MoveInput;
        bool isMoving = move.sqrMagnitude >= MoveThreshold * MoveThreshold;

        if (isMoving)
        {
            float angle = Mathf.Atan2(move.y, move.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f;
            currentOctant = Mathf.RoundToInt(angle / 45f) % 8;
        }

        if (isMoving != wasMoving)
        {
            // Restart the cycle so idle/walk always begins on frame 0.
            frameTimer = 0f;
            frameIndex = 0;
        }
        wasMoving = isMoving;

        var anim = GetAnimation(currentOctant);
        Sprite[] frames = isMoving ? anim.walk : anim.idle;
        if (frames == null || frames.Length == 0) return;

        AdvanceFrame(frames.Length);
        sr.sprite = frames[frameIndex];
    }

    void AdvanceFrame(int frameCount)
    {
        float rate = framesPerSecond * (playerMove.IsSneaking ? sneakFrameRateMultiplier : 1f);
        float frameDuration = 1f / Mathf.Max(0.01f, rate);
        frameTimer += Time.deltaTime;
        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            frameIndex = (frameIndex + 1) % frameCount;
        }
    }

    DirectionalAnimation GetAnimation(int octant) => octant switch
    {
        0 => east,
        1 => northEast,
        2 => north,
        3 => northWest,
        4 => west,
        5 => southWest,
        6 => south,
        _ => southEast,
    };

    void ApplyInitialSprite()
    {
        var anim = GetAnimation(currentOctant);
        if (anim?.idle != null && anim.idle.Length > 0)
        {
            sr.sprite = anim.idle[0];
        }
    }
}
