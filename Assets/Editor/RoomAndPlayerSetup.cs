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
    const string AnimSpritesDir = "Assets/Art/Player";

    [MenuItem("Tools/Rooms/Register 1room-3room-cave2 In Build Settings")]
    public static void RegisterRoomScenesInBuildSettings()
    {
        string[] mustHave = { "Assets/Scenes/1room.unity", "Assets/Scenes/3room.unity", "Assets/Scenes/cave2.unity", "Assets/Scenes/EndingScreen.unity" };

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

    // EditorSceneManager.OpenScene called from script silently reloads from disk —
    // no "save changes?" prompt like Ctrl+O gives. If the target scene is already
    // the active one, keep it as-is instead of discarding any unsaved edits.
    static UnityEngine.SceneManagement.Scene OpenSceneSafely(string path)
    {
        var active = EditorSceneManager.GetActiveScene();
        if (active.path == path) return active;
        return EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
    }

    [MenuItem("Tools/Player/Upgrade Player Prefab")]
    public static void UpgradePlayerPrefab()
    {
        var root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);

        var dir = root.GetComponent<PlayerDirectionalSprite>();
        if (dir == null) dir = root.AddComponent<PlayerDirectionalSprite>();
        dir.south = LoadDirection("south");
        dir.southEast = LoadDirection("south-east");
        dir.east = LoadDirection("east");
        dir.northEast = LoadDirection("north-east");
        dir.north = LoadDirection("north");
        dir.northWest = LoadDirection("north-west");
        dir.west = LoadDirection("west");
        dir.southWest = LoadDirection("south-west");

        var sr = root.GetComponent<SpriteRenderer>();
        sr.sprite = dir.south.idle.Length > 0 ? dir.south.idle[0] : LoadSprite("south");
        sr.color = Color.white;

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
        var scene = OpenSceneSafely("Assets/Scenes/1room.unity");

        BuildDialogueCanvas();
        BuildSanityAndGameOverUI();

        var door = GameObject.Find("Hole_to_cave");
        var sceneDoor = door != null ? door.GetComponent<SceneDoor>() : null;
        if (sceneDoor != null) sceneDoor.sceneName = "3room";
        else Debug.LogWarning("RoomAndPlayerSetup: Hole_to_cave/SceneDoor not found in 1room.");

        var introGO = GameObject.Find("IntroSequence");
        if (introGO == null) introGO = new GameObject("IntroSequence");
        if (introGO.GetComponent<IntroSequence>() == null)
            introGO.AddComponent<IntroSequence>();

        var enemyGO = GameObject.Find("Enemy_SpikyRed");
        if (enemyGO != null && enemyGO.GetComponent<MonsterFearZone>() == null)
            enemyGO.AddComponent<MonsterFearZone>();
        else if (enemyGO == null)
            Debug.LogWarning("RoomAndPlayerSetup: Enemy_SpikyRed not found in 1room.");

        var storageGO = GameObject.Find("Floor_Storage");
        if (storageGO != null && storageGO.GetComponent<RoomEntranceBlocker>() == null)
            storageGO.AddComponent<RoomEntranceBlocker>();
        else if (storageGO == null)
            Debug.LogWarning("RoomAndPlayerSetup: Floor_Storage not found in 1room.");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("RoomAndPlayerSetup: 1room configured (dialogue UI + door to 3room + intro + monster fear zone + storage entrance blocker).");
    }

    [MenuItem("Tools/Rooms/Setup 3room")]
    public static void Setup3Room()
    {
        var scene = OpenSceneSafely("Assets/Scenes/3room.unity");

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

    [MenuItem("Tools/Rooms/Setup Cave2")]
    public static void SetupCave2()
    {
        var scene = OpenSceneSafely("Assets/Scenes/cave2.unity");

        BuildDialogueCanvas();
        BuildSanityAndGameOverUI();

        // The vase here (Prop_8_12 (4)) already ships from the friend's export with
        // Interactable + CircleCollider2D + SanityPillVase wired up — it just needed
        // the dialogue/sanity UI above to actually be visible in this scene.
        var vaseGO = GameObject.Find("Prop_8_12 (4)");
        if (vaseGO != null && vaseGO.GetComponent<SanityPillVase>() == null)
            MakeSanityPillVase("Prop_8_12 (4)");
        else if (vaseGO == null)
            Debug.LogWarning("RoomAndPlayerSetup: Prop_8_12 (4) not found in cave2.");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("RoomAndPlayerSetup: cave2 configured (dialogue UI + sanity/game-over UI, vase interaction confirmed).");
    }

    // The user copy-pasted the 1room->3room door into cave2 as a new exit point;
    // it kept its old target ("3room") from the copy. Point it at Mockup_Gray instead.
    [MenuItem("Tools/Rooms/Wire Cave2 Exit To Mockup Gray")]
    public static void WireCave2ExitToMockupGray()
    {
        var scene = OpenSceneSafely("Assets/Scenes/cave2.unity");

        var doorGO = GameObject.Find("Hole_to_cave");
        var sceneDoor = doorGO != null ? doorGO.GetComponent<SceneDoor>() : null;
        if (sceneDoor == null)
        {
            Debug.LogWarning("RoomAndPlayerSetup: Hole_to_cave/SceneDoor not found in cave2, nothing to wire.");
            return;
        }

        sceneDoor.sceneName = "Mockup_Gray";

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("RoomAndPlayerSetup: cave2's Hole_to_cave now leads to Mockup_Gray.");
    }

    // The vase in cave2 was duplicated from a sanity-pill vase before being reworked
    // into the USB flash-drive puzzle — strip the leftover pill/switch behavior first
    // so it doesn't also fire alongside the new script.
    [MenuItem("Tools/Rooms/Wire Cave2 Usb Vase")]
    public static void WireCave2UsbVase()
    {
        var scene = OpenSceneSafely("Assets/Scenes/cave2.unity");

        MakeUsbFlashVase("Prop_8_12 (10)");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("RoomAndPlayerSetup: cave2's Prop_8_12 (10) now runs the USB flash-drive puzzle (3 tries, then spawns the Global Light).");
    }

    // Every vase's only Collider2D is the trigger Interactable needs for range
    // detection, so the player has always been able to walk straight through them.
    // Scans every room scene and gives each vase a small solid collider too.
    [MenuItem("Tools/Rooms/Block Player Walking Through Vases")]
    public static void BlockPlayerWalkingThroughVases()
    {
        string[] scenePaths =
        {
            "Assets/Scenes/1room.unity",
            "Assets/Scenes/3room.unity",
            "Assets/Scenes/cave2.unity",
            "Assets/Scenes/Mockup_Gray.unity",
        };

        foreach (var path in scenePaths)
        {
            var scene = OpenSceneSafely(path);

            int fixedCount = 0;
            foreach (var interactable in Object.FindObjectsByType<Interactable>(FindObjectsSortMode.None))
            {
                if (!interactable.name.StartsWith("Prop_8_12")) continue;
                if (AddSolidBlockCollider(interactable.gameObject)) fixedCount++;
            }

            if (fixedCount > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            Debug.Log($"RoomAndPlayerSetup: {path} — added solid colliders to {fixedCount} vase(s) that were missing one.");
        }
    }

    static bool AddSolidBlockCollider(GameObject go, float radius = 0.3f)
    {
        if (go.GetComponents<CircleCollider2D>().Any(c => !c.isTrigger)) return false;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = false;
        col.radius = radius;
        return true;
    }

    static void MakeUsbFlashVase(string objectName)
    {
        var vaseGO = GameObject.Find(objectName);
        if (vaseGO == null)
        {
            Debug.LogWarning($"RoomAndPlayerSetup: '{objectName}' not found, skipping USB flash vase setup.");
            return;
        }

        var oldPillVase = vaseGO.GetComponent<SanityPillVase>();
        if (oldPillVase != null) Object.DestroyImmediate(oldPillVase);
        var oldLightSwitch = vaseGO.GetComponent<LightSwitchVase>();
        if (oldLightSwitch != null) Object.DestroyImmediate(oldLightSwitch);

        var interactable = vaseGO.GetComponent<Interactable>();
        if (interactable == null) interactable = vaseGO.AddComponent<Interactable>();

        var col = vaseGO.GetComponent<CircleCollider2D>();
        if (col == null) col = vaseGO.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.6f;

        if (vaseGO.GetComponent<UsbFlashVase>() == null)
            vaseGO.AddComponent<UsbFlashVase>();

        AddSolidBlockCollider(vaseGO);
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

        AddSolidBlockCollider(vaseGO);
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

        AddSolidBlockCollider(vaseGO);
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

        if (doorGO.GetComponent<InteractionHintIcon>() == null)
            doorGO.AddComponent<InteractionHintIcon>();
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

        AddSolidBlockCollider(vaseGO);
    }

    static Sprite LoadSprite(string direction)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/{direction}.png");
    }

    // Loads every sliced sub-sprite of a multi-sprite sheet (walk_sheet_south_0, _1, ...),
    // in frame order, for use as a DirectionalAnimation's idle/walk clip.
    static Sprite[] LoadFrames(string sheetName)
    {
        var path = $"{AnimSpritesDir}/{sheetName}.png";
        return AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<Sprite>()
            .OrderBy(s => s.name, System.StringComparer.Ordinal)
            .ToArray();
    }

    static PlayerDirectionalSprite.DirectionalAnimation LoadDirection(string direction)
    {
        return new PlayerDirectionalSprite.DirectionalAnimation
        {
            idle = LoadFrames($"idle_sheet_{direction}"),
            walk = LoadFrames($"walk_sheet_{direction}"),
        };
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
