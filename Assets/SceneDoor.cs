using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDoor : MonoBehaviour
{
    public string sceneName;
    public string promptMessage = "[Space] Спуститься ниже";
    public KeyCode activateKey = KeyCode.Space;

    DialogueUI dialogueUI;
    bool playerInRange;

    void Awake()
    {
        dialogueUI = FindFirstObjectByType<DialogueUI>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
    }

    // Lets a gating script (e.g. DepthCheckDoor) sync state immediately when it
    // enables this component mid-overlap, without waiting for the player to
    // step out and back into the trigger.
    public void SetPlayerInRange(bool value)
    {
        playerInRange = value;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        dialogueUI?.HidePrompt();
    }

    void Update()
    {
        if (!playerInRange) return;
        if (Input.GetKeyDown(activateKey))
        {
            dialogueUI?.HidePrompt();
            SceneManager.LoadScene(sceneName);
        }
    }

    // Runs after every Update() this frame (including InteractionManager's), so the
    // door prompt wins even though InteractionManager re-hides the shared prompt
    // every frame when no item is nearby.
    void LateUpdate()
    {
        if (playerInRange) dialogueUI?.ShowPrompt(promptMessage);
    }
}
