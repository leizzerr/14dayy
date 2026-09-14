using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class RoomAndPlayerSetup
{
    const string PlayerPrefabPath = "Assets/Prefabs/Player.prefab";
    const string SpritesDir = "Assets/Art/Characters/PlayerDirections";

    [MenuItem("Tools/Rooms/Register 1room-3room-cave2 In Build Settings")]
    public static void RegisterRoomScenesInBuildSettings()
    {
        string[] mustHave = { "Assets/Scenes/1room.unity", "Assets/Scenes/3room.unity", "Assets/Scenes/cave2.unity" };

        var scenes = EditorBuildSettings.scenes.ToList();
        foreach (var path in mustHave)
        {
            if (scenes.Any(s => s.path == path)) continue;
            scenes.Add(new EditorBuildSettingsScene(path, true));
        }

        EditorBuildSettings.scenes = scenes.ToArray();
        AssetDatabase.SaveAssets();
        Debug.Log("RoomAndPlayerSetup: build settings now include " + string.Join(", ", scenes.Select(s => s.path)));
    }

    static Font UiFont => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    [MenuItem("Tools/Player/Upgrade Player Prefab")]
    public static void UpgradePlayerPrefab()
    {
        var root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);

        var sr = root.GetComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("south");
        sr.color = Color.white;

        var dir = root.GetComponent<PlayerDirectionalSprite>();
        if (dir == null) dir = root.AddComponent<PlayerDirectionalSprite>();
        dir.south = LoadSprite("south");
        dir.southEast = LoadSprite("south-east");
        dir.east = LoadSprite("east");
        dir.northEast = LoadSprite("north-east");
        dir.north = LoadSprite("north");
        dir.northWest = LoadSprite("north-west");
        dir.west = LoadSprite("west");
        dir.southWest = LoadSprite("south-west");

        if (root.GetComponent<InteractionManager>() == null)
            root.AddComponent<InteractionManager>();

        if (root.GetComponent<SanityMeter>() == null)
            root.AddComponent<SanityMeter>();

        if (root.GetComponent<PlayerInventory>() == null)
            root.AddComponent<PlayerInventory>();

        // The 56x56 character sprites only have opaque pixels in a ~17x27 box in the
        // middle (rest is transparent padding) — shrink the collider to match instead
        // of the old full-canvas 1x1 box left over from the placeholder square.
        var box = root.GetComponent<BoxCollider2D>();
        if (box != null)
        {
            box.size = new Vector2(0.32f, 0.48f);
            box.offset = Vector2.zero;
        }

        root.tag = "Player";

        PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
        PrefabUtility.UnloadPrefabContents(root);
        Debug.Log("RoomAndPlayerSetup: Player.prefab upgraded with directional sprites + InteractionManager + SanityMeter + PlayerInventory + resized collider.");
    }

    [MenuItem("Tools/Rooms/Setup 1room")]
    public static void Setup1Room()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/1room.unity", OpenSceneMode.Single);

        BuildDialogueCanvas();
        BuildSanityAndGameOverUI();

        var door = GameObject.Find("Hole_to_cave");
        var sceneDoor = door != null ? door.GetComponent<SceneDoor>() : null;
        if (sceneDoor != null) sceneDoor.sceneName = "3room";
        else Debug.LogWarning("RoomAndPlayerSetup: Hole_to_cave/SceneDoor not found in 1room.");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("RoomAndPlayerSetup: 1room configured (dialogue UI + door to 3room).");
    }

    [MenuItem("Tools/Rooms/Setup 3room")]
    public static void Setup3Room()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/3room.unity", OpenSceneMode.Single);

        BuildDialogueCanvas();
        BuildSanityAndGameOverUI();

        var light = GameObject.Find("Spot Light 2D");
        var player = Object.FindFirstObjectByType<PlayerMove>();
        if (light != null && player != null)
            light.transform.SetParent(player.transform, true);
        else
            Debug.LogWarning("RoomAndPlayerSetup: Spot Light 2D or Player not found in 3room.");

        MakeSanityPillVase("Prop_8_12 (3)");
        MakeSanityPillVase("Prop_8_12 (4)");
        MakeDepthStoneVase("Prop_8_12 (9)");
        MakeLightSwitchVase("Prop_8_12 (10)");
        MakeDepthCheckDoor("Hole_to_cave", "cave2");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("RoomAndPlayerSetup: 3room configured (dialogue UI + light now follows player + 2 sanity pill vases + light-switch vase + depth-check descent).");
    }

    static void MakeDepthStoneVase(string objectName)
    {
        var vaseGO = GameObject.Find(objectName);
        if (vaseGO == null)
        {
            Debug.LogWarning($"RoomAndPlayerSetup: '{objectName}' not found, skipping depth stone vase setup.");
            return;
        }

        var interactable = vaseGO.GetComponent<Interactable>();
        if (interactable == null) interactable = vaseGO.AddComponent<Interactable>();

        var col = vaseGO.GetComponent<CircleCollider2D>();
        if (col == null) col = vaseGO.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.6f;

        if (vaseGO.GetComponent<DepthStoneVase>() == null)
            vaseGO.AddComponent<DepthStoneVase>();
    }

    static void MakeLightSwitchVase(string objectName)
    {
        var vaseGO = GameObject.Find(objectName);
        if (vaseGO == null)
        {
            Debug.LogWarning($"RoomAndPlayerSetup: '{objectName}' not found, skipping light switch vase setup.");
            return;
        }

        var interactable = vaseGO.GetComponent<Interactable>();
        if (interactable == null) interactable = vaseGO.AddComponent<Interactable>();

        var col = vaseGO.GetComponent<CircleCollider2D>();
        if (col == null) col = vaseGO.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.6f;

        if (vaseGO.GetComponent<LightSwitchVase>() == null)
            vaseGO.AddComponent<LightSwitchVase>();
    }

    static void MakeDepthCheckDoor(string objectName, string targetScene)
    {
        var doorGO = GameObject.Find(objectName);
        if (doorGO == null)
        {
            Debug.LogWarning($"RoomAndPlayerSetup: '{objectName}' not found, skipping depth-check door setup.");
            return;
        }

        var sceneDoor = doorGO.GetComponent<SceneDoor>();
        if (sceneDoor == null) sceneDoor = doorGO.AddComponent<SceneDoor>();
        sceneDoor.sceneName = targetScene;

        bool hadInteractable = doorGO.GetComponent<Interactable>() != null;
        if (!hadInteractable) doorGO.AddComponent<Interactable>();

        if (!hadInteractable)
        {
            // Interactable's [RequireComponent(CircleCollider2D)] just auto-added a
            // fresh trigger collider (separate from SceneDoor's own BoxCollider2D) —
            // configure it explicitly rather than relying on Reset() having run.
            var examineCol = doorGO.GetComponent<CircleCollider2D>();
            if (examineCol != null)
            {
                examineCol.isTrigger = true;
                examineCol.radius = 0.9f;
            }
        }

        if (doorGO.GetComponent<DepthCheckDoor>() == null)
            doorGO.AddComponent<DepthCheckDoor>();
    }

    static void MakeSanityPillVase(string objectName)
    {
        var vaseGO = GameObject.Find(objectName);
        if (vaseGO == null)
        {
            Debug.LogWarning($"RoomAndPlayerSetup: '{objectName}' not found, skipping sanity pill vase setup.");
            return;
        }

        var interactable = vaseGO.GetComponent<Interactable>();
        if (interactable == null) interactable = vaseGO.AddComponent<Interactable>();

        var col = vaseGO.GetComponent<CircleCollider2D>();
        if (col == null) col = vaseGO.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.6f;

        if (vaseGO.GetComponent<SanityPillVase>() == null)
            vaseGO.AddComponent<SanityPillVase>();
    }

    static Sprite LoadSprite(string direction)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/{direction}.png");
    }

    static void BuildDialogueCanvas()
    {
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();
        }

        var existing = GameObject.Find("DialogueCanvas");
        if (existing != null) Object.DestroyImmediate(existing);

        var canvasGO = new GameObject("DialogueCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

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
        titleText.font = UiFont;
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
        bodyText.font = UiFont;
        bodyText.fontSize = 20;
        bodyText.color = new Color(0.9f, 0.9f, 0.9f);
        bodyText.verticalOverflow = VerticalWrapMode.Overflow;
        bodyText.text = "Описание";

        var promptGO = new GameObject("PromptText");
        promptGO.transform.SetParent(canvasGO.transform, false);
        var promptRect = promptGO.AddComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0.5f, 0f);
        promptRect.anchorMax = new Vector2(0.5f, 0f);
        promptRect.pivot = new Vector2(0.5f, 0f);
        promptRect.sizeDelta = new Vector2(400, 40);
        promptRect.anchoredPosition = new Vector2(0, 230);
        var promptText = promptGO.AddComponent<Text>();
        promptText.font = UiFont;
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
    }

    static void BuildSanityAndGameOverUI()
    {
        var existingSanity = GameObject.Find("SanityCanvas");
        if (existingSanity != null) Object.DestroyImmediate(existingSanity);
        var existingGameOver = GameObject.Find("GameOverCanvas");
        if (existingGameOver != null) Object.DestroyImmediate(existingGameOver);

        // --- Sanity bar, top-left ---
        var sanityCanvasGO = new GameObject("SanityCanvas");
        var sanityCanvas = sanityCanvasGO.AddComponent<Canvas>();
        sanityCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var sanityScaler = sanityCanvasGO.AddComponent<CanvasScaler>();
        sanityScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sanityScaler.referenceResolution = new Vector2(1280, 720);
        sanityScaler.matchWidthOrHeight = 0.5f;
        sanityCanvasGO.AddComponent<GraphicRaycaster>();

        var barBgGO = new GameObject("SanityBarBackground");
        barBgGO.transform.SetParent(sanityCanvasGO.transform, false);
        var barBgRect = barBgGO.AddComponent<RectTransform>();
        barBgRect.anchorMin = new Vector2(0, 1);
        barBgRect.anchorMax = new Vector2(0, 1);
        barBgRect.pivot = new Vector2(0, 1);
        barBgRect.sizeDelta = new Vector2(260, 26);
        barBgRect.anchoredPosition = new Vector2(20, -20);
        var barBgImg = barBgGO.AddComponent<Image>();
        barBgImg.color = new Color(0f, 0f, 0f, 0.6f);

        var barTrailGO = new GameObject("SanityBarTrail");
        barTrailGO.transform.SetParent(barBgGO.transform, false);
        var barTrailRect = barTrailGO.AddComponent<RectTransform>();
        barTrailRect.anchorMin = Vector2.zero;
        barTrailRect.anchorMax = Vector2.one;
        barTrailRect.offsetMin = new Vector2(3, 3);
        barTrailRect.offsetMax = new Vector2(-3, -3);
        var barTrailImg = barTrailGO.AddComponent<Image>();
        barTrailImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        barTrailImg.color = new Color(1f, 0.4f, 0.5f, 0.9f);
        barTrailImg.type = Image.Type.Filled;
        barTrailImg.fillMethod = Image.FillMethod.Horizontal;
        barTrailImg.fillOrigin = (int)Image.OriginHorizontal.Left;
        barTrailImg.fillAmount = 1f;

        var barFillGO = new GameObject("SanityBarFill");
        barFillGO.transform.SetParent(barBgGO.transform, false);
        var barFillRect = barFillGO.AddComponent<RectTransform>();
        barFillRect.anchorMin = Vector2.zero;
        barFillRect.anchorMax = Vector2.one;
        barFillRect.offsetMin = new Vector2(3, 3);
        barFillRect.offsetMax = new Vector2(-3, -3);
        var barFillImg = barFillGO.AddComponent<Image>();
        barFillImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        barFillImg.color = new Color(0.55f, 0.35f, 0.85f);
        barFillImg.type = Image.Type.Filled;
        barFillImg.fillMethod = Image.FillMethod.Horizontal;
        barFillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
        barFillImg.fillAmount = 1f;

        var labelGO = new GameObject("SanityLabel");
        labelGO.transform.SetParent(sanityCanvasGO.transform, false);
        var labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 1);
        labelRect.anchorMax = new Vector2(0, 1);
        labelRect.pivot = new Vector2(0, 1);
        labelRect.sizeDelta = new Vector2(260, 18);
        labelRect.anchoredPosition = new Vector2(20, -2);
        var labelText = labelGO.AddComponent<Text>();
        labelText.font = UiFont;
        labelText.fontSize = 14;
        labelText.color = Color.white;
        labelText.text = "РАССУДОК";

        var sanityUI = sanityCanvasGO.AddComponent<SanityUI>();
        sanityUI.fillImage = barFillImg;
        sanityUI.trailImage = barTrailImg;

        // --- Game over screen ---
        var gameOverCanvasGO = new GameObject("GameOverCanvas");
        var gameOverCanvas = gameOverCanvasGO.AddComponent<Canvas>();
        gameOverCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        gameOverCanvas.sortingOrder = 10;
        var gameOverScaler = gameOverCanvasGO.AddComponent<CanvasScaler>();
        gameOverScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        gameOverScaler.referenceResolution = new Vector2(1280, 720);
        gameOverScaler.matchWidthOrHeight = 0.5f;
        gameOverCanvasGO.AddComponent<GraphicRaycaster>();

        var overlayGO = new GameObject("Overlay");
        overlayGO.transform.SetParent(gameOverCanvasGO.transform, false);
        var overlayRect = overlayGO.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        var overlayImg = overlayGO.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.9f);

        var goTitleGO = new GameObject("GameOverTitle");
        goTitleGO.transform.SetParent(overlayGO.transform, false);
        var goTitleRect = goTitleGO.AddComponent<RectTransform>();
        goTitleRect.anchorMin = new Vector2(0.5f, 0.5f);
        goTitleRect.anchorMax = new Vector2(0.5f, 0.5f);
        goTitleRect.sizeDelta = new Vector2(800, 80);
        goTitleRect.anchoredPosition = new Vector2(0, 60);
        var goTitleText = goTitleGO.AddComponent<Text>();
        goTitleText.font = UiFont;
        goTitleText.fontSize = 40;
        goTitleText.fontStyle = FontStyle.Bold;
        goTitleText.alignment = TextAnchor.MiddleCenter;
        goTitleText.color = Color.white;
        goTitleText.text = "Рассудок иссяк";

        var gameOverUI = gameOverCanvasGO.AddComponent<GameOverUI>();
        gameOverUI.panel = overlayGO;

        var restartBtn = CreateGameOverButton(overlayGO.transform, "RestartButton", "Начать заново", new Vector2(-170, -60));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(restartBtn.onClick, gameOverUI.RestartGame);

        var menuBtn = CreateGameOverButton(overlayGO.transform, "MainMenuButton", "Главное меню", new Vector2(170, -60));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(menuBtn.onClick, gameOverUI.ReturnToMainMenu);
    }

    static Button CreateGameOverButton(Transform parent, string name, string label, Vector2 anchoredPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(280, 60);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
        var img = go.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.12f);
        var btn = go.AddComponent<Button>();

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        var textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        var text = textGO.AddComponent<Text>();
        text.font = UiFont;
        text.fontSize = 24;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = label;

        return btn;
    }
}
