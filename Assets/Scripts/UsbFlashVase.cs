using UnityEngine;
using UnityEngine.Rendering.Universal;

// Reworked "switch vase" for cave2: instead of a plain switch, the player finds a
// USB flash drive plugged near a cable running into the wall. It takes three tries
// (wrong way, flipped, flipped back) before it finally goes in and the lights come on.
[RequireComponent(typeof(Interactable))]
public class UsbFlashVase : MonoBehaviour
{
    public string foundDescription =
        "В вазе лежит USB-флешка, а рядом из стены торчит разъём с уходящим внутрь проводом.";
    public string tryAgainDescription = "Может, попробовать вставить её по-другому?";
    public string finalDescription = "Флешка вставлена, разъём тихо гудит.";

    [TextArea] public string firstAttemptMessage = "Ты пытаешься вставить флешку в разъём... Не входит.";
    [TextArea] public string secondAttemptMessage = "Ты переворачиваешь флешку и пробуешь снова... Опять не входит.";
    [TextArea] public string successMessage = "Ты переворачиваешь её обратно и вставляешь... Есть! Флешка наконец входит.";

    public string globalLightName = "Global Light 2D";
    public Color globalLightColor = Color.white;
    public float globalLightIntensity = 1f;

    Interactable interactable;
    DialogueUI dialogueUI;
    GameObject globalLight;
    int attempt;

    void Awake()
    {
        interactable = GetComponent<Interactable>();
        dialogueUI = FindFirstObjectByType<DialogueUI>();
        globalLight = GameObject.Find(globalLightName);

        interactable.displayName = "Ваза";
        interactable.description = foundDescription;
        interactable.onExamine.AddListener(OnExamined);

        // Present in the editor for the developer's convenience only — off by
        // default until the player actually plugs the flash drive in.
        if (globalLight != null) globalLight.SetActive(false);
    }

    void OnExamined()
    {
        attempt++;

        if (attempt == 1)
        {
            interactable.description = tryAgainDescription;
            dialogueUI?.QueueNext("Флешка", firstAttemptMessage);
        }
        else if (attempt == 2)
        {
            interactable.description = tryAgainDescription;
            dialogueUI?.QueueNext("Флешка", secondAttemptMessage);
        }
        else if (attempt == 3)
        {
            interactable.description = finalDescription;
            dialogueUI?.QueueNext("Флешка", successMessage);
            // Wait for the player to actually close the queued message before the
            // light comes on — spawning it right away made it pop in too early.
            if (dialogueUI != null) dialogueUI.OnFullyClosed += HandleFinalMessageClosed;
        }
    }

    void HandleFinalMessageClosed()
    {
        if (dialogueUI != null) dialogueUI.OnFullyClosed -= HandleFinalMessageClosed;
        SpawnOrActivateGlobalLight();
    }

    void SpawnOrActivateGlobalLight()
    {
        if (globalLight != null)
        {
            globalLight.SetActive(true);
            return;
        }

        // No pre-placed light in this scene — spawn one instead of relying on it.
        var lightGO = new GameObject(globalLightName);
        var light = lightGO.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Global;
        light.color = globalLightColor;
        light.intensity = globalLightIntensity;
        globalLight = lightGO;
    }
}
