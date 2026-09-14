using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float speed = 4f;

    [Header("Sneaking (hold Shift)")]
    public float sneakSpeedMultiplier = 0.5f;
    [Tooltip("How far a listening enemy can hear the player while walking normally.")]
    public float noiseRadiusWalking = 9f;
    [Tooltip("How far a listening enemy can hear the player while sneaking.")]
    public float noiseRadiusSneaking = 2.5f;

    private Rigidbody2D rb;
    private Vector2 input;

    public Vector2 MoveInput => input;
    public bool IsSneaking { get; private set; }

    // Radius within which a listening enemy can hear this player right now.
    // 0 while standing still — no footsteps, no noise.
    public float CurrentNoiseRadius
    {
        get
        {
            if (input.sqrMagnitude < 0.0001f) return 0f;
            return IsSneaking ? noiseRadiusSneaking : noiseRadiusWalking;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        input = input.normalized;

        IsSneaking = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    void FixedUpdate()
    {
        float currentSpeed = IsSneaking ? speed * sneakSpeedMultiplier : speed;
        rb.linearVelocity = input * currentSpeed;
    }
}
