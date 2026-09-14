using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class MockupGrayEndingSetup
{
    const string PlayerPrefabPath = "Assets/Prefabs/Player.prefab";

    [MenuItem("Tools/Mockups/Setup Mockup_Gray Endings")]
    public static void Setup()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Mockup_Gray.unity", OpenSceneMode.Single);

        EnsurePlayer();

        var mgrGO = GameObject.Find("MockupEndingManager");
        if (mgrGO == null) mgrGO = new GameObject("MockupEndingManager");
        if (mgrGO.GetComponent<MockupEndingManager>() == null)
            mgrGO.AddComponent<MockupEndingManager>();

        SetupTrigger("Prop_4_26", typeof(DoorExitTrigger));
        SetupTrigger("Ground_13_9", typeof(ContinueExploreTrigger));
        AddHintIcon("Prop_4_26");
        AddHintIcon("Ground_13_9");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("MockupGrayEndingSetup: Mockup_Gray configured with two-ending exit logic.");
    }

    [MenuItem("Tools/Mockups/Build Ending Screen Scene")]
    public static void BuildEndingScreen()
    {
        AssetDatabase.ImportAsset("Assets/Audio/Music/Spacetime.mp3", ImportAssetOptions.ForceUpdate);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.orthographic = true;
        camGO.AddComponent<AudioListener>();

        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();
        esGO.AddComponent<StandaloneInputModule>();

        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        var textGO = new GameObject("MessageText");
        textGO.transform.SetParent(canvasGO.transform, false);
        var textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(1000, 200);
        textRect.anchoredPosition = Vector2.zero;
        var text = textGO.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 32;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = "";

        var musicClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/Spacetime.mp3");
        var musicGO = new GameObject("MusicSource");
        var musicSource = musicGO.AddComponent<AudioSource>();
        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;

        var controller = canvasGO.AddComponent<EndingScreenController>();
        controller.messageText = text;
        controller.musicSource = musicSource;

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/EndingScreen.unity");
        Debug.Log("MockupGrayEndingSetup: EndingScreen.unity created.");
    }

    static void EnsurePlayer()
    {
        var existing = Object.FindFirstObjectByType<PlayerMove>();
        if (existing != null)
        {
            existing.transform.position = FindSpawnPoint();
            return;
        }

        // The scene's old static establishing-shot camera (built by MockupTilemapBuilder)
        // gets replaced by Player.prefab's own follow-camera child, like 1room/3room/cave2.
        var oldCamGO = GameObject.Find("Main Camera");
        if (oldCamGO != null) Object.DestroyImmediate(oldCamGO);

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        if (prefab == null)
        {
            Debug.LogError("MockupGrayEndingSetup: Player.prefab not found.");
            return;
        }

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.transform.position = FindSpawnPoint();
    }

    static Vector3 FindSpawnPoint()
    {
        // The player starts on the small walled platform/island in the water room.
        var platformTile = GameObject.Find("Ground_18_27");
        if (platformTile != null) return platformTile.transform.position;

        Debug.LogWarning("MockupGrayEndingSetup: platform spawn tile not found, falling back to level average.");
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

    static void SetupTrigger(string objectName, System.Type triggerType)
    {
        var go = GameObject.Find(objectName);
        if (go == null)
        {
            Debug.LogWarning($"MockupGrayEndingSetup: '{objectName}' not found.");
            return;
        }

        var interactable = go.GetComponent<Interactable>();
        if (interactable == null) interactable = go.AddComponent<Interactable>();

        var col = go.GetComponent<CircleCollider2D>();
        if (col == null) col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.7f;

        if (go.GetComponent(triggerType) == null)
            go.AddComponent(triggerType);
    }

    static void AddHintIcon(string objectName)
    {
        var go = GameObject.Find(objectName);
        if (go == null) return;
        if (go.GetComponent<InteractionHintIcon>() == null)
            go.AddComponent<InteractionHintIcon>();
    }
}
