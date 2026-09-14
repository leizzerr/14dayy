using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public static class InteractionSceneSetup
{
    [MenuItem("Tools/Mockups/Setup Interaction System (Gray Scene)")]
    public static void Setup()
    {
        string scenePath = "Assets/Scenes/Mockup_Gray.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // --- Player ---
        var playerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Characters/player_placeholder.png");

        var existingPlayer = Object.FindFirstObjectByType<PlayerMove>();
        GameObject playerGO;
        if (existingPlayer != null)
        {
            playerGO = existingPlayer.gameObject;
        }
        else
        {
            playerGO = new GameObject("Player");
            var rb = playerGO.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            var col = playerGO.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 0.8f);
            playerGO.AddComponent<SpriteRenderer>();
            playerGO.AddComponent<PlayerMove>();
            playerGO.transform.position = FindSpawnPoint();
        }

        var playerSr = playerGO.GetComponent<SpriteRenderer>();
        if (playerSr == null) playerSr = playerGO.AddComponent<SpriteRenderer>();
        playerSr.sprite = playerSprite != null ? playerSprite : MakeSolidSprite(new Color(1f, 0.92f, 0.02f), 16, 16);
        playerSr.sortingOrder = 10;
        var litMaterial = AssetDatabase.LoadAssetAtPath<Material>(
            AssetDatabase.GUIDToAssetPath("a97c105638bdf8b4a8650670310a4cd3"));
        if (litMaterial != null) playerSr.sharedMaterial = litMaterial;

        // --- EventSystem (for future UI interaction, harmless if unused) ---
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();
        }

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // --- Canvas ---
        var canvasGO = GameObject.Find("DialogueCanvas");
        if (canvasGO != null) Object.DestroyImmediate(canvasGO);
        canvasGO = new GameObject("DialogueCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // --- Dialogue panel ---
        var panelGO = new GameObject("DialoguePanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        var panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.sizeDelta = new Vector2(900, 180);
        panelRect.anchoredPosition = new Vector2(0, 40);
        var panelImg = panelGO.AddComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.85f);

        var titleGO = new GameObject("TitleText");
        titleGO.transform.SetParent(panelGO.transform, false);
        var titleRect = titleGO.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.sizeDelta = new Vector2(-40, 40);
        titleRect.anchoredPosition = new Vector2(0, -10);
        var titleText = titleGO.AddComponent<Text>();
        titleText.font = font;
        titleText.fontSize = 26;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = Color.white;
        titleText.text = "Название";

        var bodyGO = new GameObject("BodyText");
        bodyGO.transform.SetParent(panelGO.transform, false);
        var bodyRect = bodyGO.AddComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0, 0);
        bodyRect.anchorMax = new Vector2(1, 1);
        bodyRect.offsetMin = new Vector2(20, 15);
        bodyRect.offsetMax = new Vector2(-20, -55);
        var bodyText = bodyGO.AddComponent<Text>();
        bodyText.font = font;
        bodyText.fontSize = 20;
        bodyText.color = new Color(0.9f, 0.9f, 0.9f);
        bodyText.verticalOverflow = VerticalWrapMode.Overflow;
        bodyText.text = "Описание";

        // --- Prompt ---
        var promptGO = new GameObject("PromptText");
        promptGO.transform.SetParent(canvasGO.transform, false);
        var promptRect = promptGO.AddComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0.5f, 0f);
        promptRect.anchorMax = new Vector2(0.5f, 0f);
        promptRect.pivot = new Vector2(0.5f, 0f);
        promptRect.sizeDelta = new Vector2(400, 40);
        promptRect.anchoredPosition = new Vector2(0, 230);
        var promptText = promptGO.AddComponent<Text>();
        promptText.font = font;
        promptText.fontSize = 22;
        promptText.alignment = TextAnchor.MiddleCenter;
        promptText.color = Color.white;
        promptText.text = "[E] Осмотреть";
        promptGO.SetActive(false);

        var dialogueUI = canvasGO.AddComponent<DialogueUI>();
        dialogueUI.panel = panelGO;
        dialogueUI.titleText = titleText;
        dialogueUI.bodyText = bodyText;
        dialogueUI.promptObject = promptGO;
        dialogueUI.promptText = promptText;

        // --- Interaction manager on Player ---
        var interactionManager = playerGO.GetComponent<InteractionManager>();
        if (interactionManager == null) interactionManager = playerGO.AddComponent<InteractionManager>();
        interactionManager.dialogueUI = dialogueUI;
        interactionManager.interactKey = KeyCode.E;

        // --- Interactable on every prop ---
        var propsParent = GameObject.Find("Level/Props");
        int count = 0;
        if (propsParent != null)
        {
            foreach (Transform child in propsParent.transform)
            {
                var interactable = child.GetComponent<Interactable>();
                if (interactable == null) interactable = child.gameObject.AddComponent<Interactable>();
                var col = child.GetComponent<CircleCollider2D>();
                if (col == null) col = child.gameObject.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.6f;
                if (string.IsNullOrEmpty(interactable.description) || interactable.description == "Здесь ничего интересного.")
                {
                    interactable.displayName = "Предмет";
                    interactable.description = "Здесь что-то лежит. Присмотрись повнимательнее и впиши описание в компоненте Interactable.";
                }
                count++;
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"InteractionSceneSetup: player, dialogue UI and {count} interactable props wired up in {scenePath}.");
    }

    static Vector3 FindSpawnPoint()
    {
        var ground = GameObject.Find("Level/Ground");
        if (ground == null || ground.transform.childCount == 0) return Vector3.zero;
        Vector3 sum = Vector3.zero;
        int n = 0;
        foreach (Transform child in ground.transform)
        {
            sum += child.position;
            n++;
        }
        return n > 0 ? sum / n : Vector3.zero;
    }

    static Sprite MakeSolidSprite(Color color, int w, int h)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        var pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
    }
}
