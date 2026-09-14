using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PlayerMove))]
public class PlayerDirectionalSprite : MonoBehaviour
{
    public Sprite south;
    public Sprite southEast;
    public Sprite east;
    public Sprite northEast;
    public Sprite north;
    public Sprite northWest;
    public Sprite west;
    public Sprite southWest;

    const float MoveThreshold = 0.1f;

    SpriteRenderer sr;
    PlayerMove playerMove;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        playerMove = GetComponent<PlayerMove>();
        sr.sprite = south;
    }

    void LateUpdate()
    {
        var move = playerMove.MoveInput;
        if (move.sqrMagnitude < MoveThreshold * MoveThreshold) return;

        float angle = Mathf.Atan2(move.y, move.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        int octant = Mathf.RoundToInt(angle / 45f) % 8;
        sr.sprite = octant switch
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
    }
}
