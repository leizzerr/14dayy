using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class MainMenuSceneSetup
{
    [MenuItem("Tools/Menu/Build Main Menu Scenes")]
    public static void Build()
    {
        AssetDatabase.ImportAsset("Assets/Audio/Music/Flags.mp3", ImportAssetOptions.ForceUpdate);

        BuildSettingsMenuScene();
        BuildMainMenuScene();
        RegisterScenesInBuildSettings();
        Debug.Log("MainMenuSceneSetup: MainMenu.unity and SettingsMenu.unity created and added to Build Settings.");
    }

    static Font UiFont => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    static void BuildMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        SetupBlackCamera();
        SetupEventSystem();
        var canvasGO = SetupCanvas();

        var controller = new GameObject("MainMenuController").AddComponent<MainMenuController>();

        var playBtn = CreateButton(canvasGO.transform, "PlayButton", "Играть", new Vector2(0, 90));
        UnityEventTools.AddPersistentListener(playBtn.onClick, controller.PlayGame);

        var settingsBtn = CreateButton(canvasGO.transform, "SettingsButton", "Настройки", new Vector2(0, 0));
        UnityEventTools.AddPersistentListener(settingsBtn.onClick, controller.OpenSettings);

        var quitBtn = CreateButton(canvasGO.transform, "QuitButton", "Выход", new Vector2(0, -90));
        UnityEventTools.AddPersistentListener(quitBtn.onClick, controller.QuitGame);

        SetupMusicManager();
        SetupTimerDisplay(canvasGO.transform);

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
    }

    static void SetupTimerDisplay(Transform canvasTransform)
    {
        var go = new GameObject("TimerText");
        go.transform.SetParent(canvasTransform, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.sizeDelta = new Vector2(360, 40);
        rect.anchoredPosition = new Vector2(-20, -20);
        var text = go.AddComponent<Text>();
        text.font = UiFont;
        text.fontSize = 20;
        text.alignment = TextAnchor.MiddleRight;
        text.color = Color.white;
        text.text = "";

        var display = go.AddComponent<GameTimerDisplay>();
        display.timerText = text;
    }

    static void SetupMusicManager()
    {
        var musicClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/Flags.mp3");
        var musicGO = new GameObject("MusicManager");
        var audioSource = musicGO.AddComponent<AudioSource>();
        audioSource.clip = musicClip;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        musicGO.AddComponent<MusicManager>();
    }

    static void BuildSettingsMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        SetupBlackCamera();
        SetupEventSystem();
        var canvasGO = SetupCanvas();

        var controller = new GameObject("SettingsMenuController").AddComponent<SettingsMenuController>();

        CreateLabel(canvasGO.transform, "Title", "Настройки", new Vector2(0, 150), 36, FontStyle.Bold);
        CreateLabel(canvasGO.transform, "MusicVolumeLabel", "Громкость музыки", new Vector2(0, 70), 20, FontStyle.Normal);

        var musicSlider = CreateSlider(canvasGO.transform, "MusicVolumeSlider", new Vector2(0, 30));
        controller.musicVolumeSlider = musicSlider;
        UnityEventTools.AddPersistentListener(musicSlider.onValueChanged, controller.OnMusicVolumeChanged);

        CreateLabel(canvasGO.transform, "SfxVolumeLabel", "Громкость звуков", new Vector2(0, -20), 20, FontStyle.Normal);

        var sfxSlider = CreateSlider(canvasGO.transform, "SfxVolumeSlider", new Vector2(0, -60));
        controller.sfxVolumeSlider = sfxSlider;
        UnityEventTools.AddPersistentListener(sfxSlider.onValueChanged, controller.OnSfxVolumeChanged);

        var backBtn = CreateButton(canvasGO.transform, "BackButton", "Назад", new Vector2(0, -150));
        UnityEventTools.AddPersistentListener(backBtn.onClick, controller.BackToMainMenu);

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/SettingsMenu.unity");
    }

    static void SetupBlackCamera()
    {
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.orthographic = true;
        camGO.AddComponent<AudioListener>();
    }

    static void SetupEventSystem()
    {
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();
        esGO.AddComponent<StandaloneInputModule>();
    }

    static GameObject SetupCanvas()
    {
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();
        return canvasGO;
    }

    static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(320, 70);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
        var img = go.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.1f);
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
        text.fontSize = 28;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = label;

        return btn;
    }

    static Text CreateLabel(Transform parent, string name, string label, Vector2 anchoredPos, int fontSize, FontStyle style)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(800, 60);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
        var text = go.AddComponent<Text>();
        text.font = UiFont;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = label;
        return text;
    }

    static Slider CreateSlider(Transform parent, string name, Vector2 anchoredPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(320, 20);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;

        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(go.transform, false);
        var bgRect = bgGO.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0.25f);
        bgRect.anchorMax = new Vector2(1, 0.75f);
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        bgGO.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.15f);

        var fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(go.transform, false);
        var fillAreaRect = fillAreaGO.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1, 0.75f);
        fillAreaRect.offsetMin = new Vector2(5, 0);
        fillAreaRect.offsetMax = new Vector2(-5, 0);

        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        var fillRect = fillGO.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color = new Color(1f, 1f, 1f, 0.85f);

        var handleAreaGO = new GameObject("Handle Slide Area");
        handleAreaGO.transform.SetParent(go.transform, false);
        var handleAreaRect = handleAreaGO.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(10, 0);
        handleAreaRect.offsetMax = new Vector2(-10, 0);

        var handleGO = new GameObject("Handle");
        handleGO.transform.SetParent(handleAreaGO.transform, false);
        var handleRect = handleGO.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 20);
        var handleImg = handleGO.AddComponent<Image>();
        handleImg.color = Color.white;

        var slider = go.AddComponent<Slider>();
        slider.targetGraphic = handleImg;
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;

        return slider;
    }

    static void RegisterScenesInBuildSettings()
    {
        string[] orderedNewScenes =
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/SettingsMenu.unity",
            "Assets/Scenes/Mockup_Gray.unity",
        };

        var existing = EditorBuildSettings.scenes
            .Where(s => !orderedNewScenes.Contains(s.path))
            .ToList();

        var newList = new List<EditorBuildSettingsScene>();
        foreach (var path in orderedNewScenes)
            newList.Add(new EditorBuildSettingsScene(path, true));
        newList.AddRange(existing);

        EditorBuildSettings.scenes = newList.ToArray();
        AssetDatabase.SaveAssets();
    }
}
