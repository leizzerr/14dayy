using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float speed = 4f;
    private Rigidbody2D rb;
    private Vector2 input;

    public Vector2 MoveInput => input;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        input = input.normalized;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = input * speed;
    }
}