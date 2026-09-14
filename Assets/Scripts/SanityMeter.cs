using UnityEngine;

public class SanityMeter : MonoBehaviour
{
    public float duration = 90f;
    public float enemyProximityRadius = 4f;
    public float enemyMultiplier = 1.5f;

    public float Sanity { get; private set; } = 1f;
    public int PillCount { get; private set; }

    SanityUI ui;
    GameOverUI gameOverUI;
    DialogueUI dialogueUI;
    PlayerMove playerMove;
    Rigidbody2D rb;
    bool gameOver;

    void Awake()
    {
        ui = FindFirstObjectByType<SanityUI>();
        gameOverUI = FindFirstObjectByType<GameOverUI>();
        dialogueUI = FindFirstObjectByType<DialogueUI>();
        playerMove = GetComponent<PlayerMove>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void AddPill()
    {
        PillCount++;
    }

    void Update()
    {
        if (gameOver) return;

        float rate = 1f / duration;
        if (IsNearEnemy()) rate *= enemyMultiplier;

        Sanity = Mathf.Max(0f, Sanity - rate * Time.deltaTime);
        ui?.SetFill(Sanity);

        if (Sanity <= 0f)
        {
            if (PillCount > 0)
            {
                PillCount--;
                Sanity = 1f;
                ui?.SetFill(Sanity);
                dialogueUI?.ShowTimed("Таблетка от нервов", "Ты автоматически принял таблетку. Рассудок полностью восстановлен.", 3f);
                return;
            }

            gameOver = true;
            if (rb != null) rb.linearVelocity = Vector2.zero;
            if (playerMove != null) playerMove.enabled = false;
            gameOverUI?.Show();
        }
    }

    bool IsNearEnemy()
    {
        var enemies = FindObjectsByType<EnemyPatrol>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            float sqrDist = (enemy.transform.position - transform.position).sqrMagnitude;
            if (sqrDist <= enemyProximityRadius * enemyProximityRadius) return true;
        }
        return false;
    }
}
