using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class LightSwitchVase : MonoBehaviour
{
    public string switchDescription = "В вазе нащупывается странный переключатель. [F] чтобы щёлкнуть его.";
    public string afterDescription = "Переключатель уже щёлкнут — в комнате светло.";
    public KeyCode activateKey = KeyCode.F;
    public string globalLightName = "Global Light 2D";

    Interactable interactable;
    DialogueUI dialogueUI;
    GameObject globalLight;
    bool activated;
    bool playerInRange;

    void Awake()
    {
        interactable = GetComponent<Interactable>();
        dialogueUI = FindFirstObjectByType<DialogueUI>();
        globalLight = GameObject.Find(globalLightName);

        interactable.displayName = "Ваза";
        interactable.description = switchDescription;

        // Present in the editor for the developer's convenience only — off by
        // default so the final build only has the player's own flashlight.
        if (globalLight != null) globalLight.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerMove>() != null) playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerMove>() != null) playerInRange = false;
    }

    void Update()
    {
        if (activated || !playerInRange) return;
        if (Input.GetKeyDown(activateKey))
        {
            activated = true;
            if (globalLight != null) globalLight.SetActive(true);
            interactable.description = afterDescription;
            dialogueUI?.Show("Ваза", "Щёлк! Свет заливает всю локацию.");
        }
    }
}
