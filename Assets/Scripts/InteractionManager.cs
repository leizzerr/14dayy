using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; private set; }

    public DialogueUI dialogueUI;
    public KeyCode interactKey = KeyCode.E;

    readonly HashSet<Interactable> inRange = new HashSet<Interactable>();
    Interactable current;
    Interactable openedFor;

    void Awake()
    {
        Instance = this;
        if (dialogueUI == null) dialogueUI = FindFirstObjectByType<DialogueUI>();
    }

    public void Register(Interactable interactable)
    {
        inRange.Add(interactable);
    }

    public void Unregister(Interactable interactable)
    {
        inRange.Remove(interactable);
        if (current == interactable) current = null;

        // The player walked out of range of whatever opened the current dialogue —
        // close it instead of leaving it hanging on screen.
        if (openedFor == interactable)
        {
            dialogueUI?.Hide();
            openedFor = null;
        }
    }

    void Update()
    {
        if (dialogueUI != null && dialogueUI.IsOpen)
        {
            if (Input.GetKeyDown(interactKey) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
            {
                dialogueUI.Hide();
                openedFor = null;
            }
            return;
        }

        current = FindClosest();
        if (dialogueUI != null)
        {
            if (current != null) dialogueUI.ShowPrompt("[E] Осмотреть");
            else dialogueUI.HidePrompt();
        }

        if (current != null && Input.GetKeyDown(interactKey) && dialogueUI != null)
        {
            dialogueUI.Show(current.displayName, current.description);
            openedFor = current;
            current.onExamine?.Invoke();
        }
    }

    Interactable FindClosest()
    {
        Interactable best = null;
        float bestDist = float.MaxValue;
        foreach (var i in inRange)
        {
            if (i == null) continue;
            float d = ((Vector2)i.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = i;
            }
        }
        return best;
    }
}
