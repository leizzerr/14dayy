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

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
    }

    static void BuildSettingsMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        SetupBlackCamera();
        SetupEventSystem();
        var canvasGO = SetupCanvas();

        var controller = new GameObject("SettingsMenuController").AddComponent<SettingsMenuController>();

        var title = CreateLabel(canvasGO.transform, "Title", "Настройки", new Vector2(0, 150), 36, FontStyle.Bold);
        CreateLabel(canvasGO.transform, "Placeholder", "Скоро здесь будут настройки", new Vector2(0, 60), 22, FontStyle.Normal);

        var backBtn = CreateButton(canvasGO.transform, "BackButton", "Назад", new Vector2(0, -90));
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
