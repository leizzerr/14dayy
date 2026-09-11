using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class FlashlightSceneSetup
{
    [MenuItem("Tools/Mockups/Setup Flashlight (Gray Scene)")]
    public static void Setup()
    {
        string scenePath = "Assets/Scenes/Mockup_Gray.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        var playerMove = Object.FindFirstObjectByType<PlayerMove>();
        if (playerMove == null)
        {
            Debug.LogError("FlashlightSceneSetup: no Player found. Run 'Setup Interaction System (Gray Scene)' first.");
            return;
        }
        var playerGO = playerMove.gameObject;

        // --- Darkness covering the whole level ---
        var darknessGO = GameObject.Find("GlobalDarkness");
        if (darknessGO == null) darknessGO = new GameObject("GlobalDarkness");
        var globalLight = darknessGO.GetComponent<Light2D>();
        if (globalLight == null) globalLight = darknessGO.AddComponent<Light2D>();
        globalLight.lightType = Light2D.LightType.Global;
        globalLight.color = Color.white;
        globalLight.intensity = 0.12f;

        // --- Flashlight following the player ---
        var lightGO = GameObject.Find("PlayerLight");
        if (lightGO == null)
        {
            lightGO = new GameObject("PlayerLight");
        }
        lightGO.transform.SetParent(playerGO.transform, false);
        lightGO.transform.localPosition = Vector3.zero;

        var playerLight = lightGO.GetComponent<Light2D>();
        if (playerLight == null) playerLight = lightGO.AddComponent<Light2D>();
        playerLight.lightType = Light2D.LightType.Point;
        playerLight.color = new Color(1f, 0.87f, 0.68f);
        playerLight.intensity = 1.4f;
        playerLight.pointLightInnerRadius = 1f;
        playerLight.pointLightOuterRadius = 4.5f;
        playerLight.falloffIntensity = 0.6f;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("FlashlightSceneSetup: global darkness + player flashlight added to " + scenePath);
    }
}
