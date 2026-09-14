using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Cave2SfxSetup
{
    static readonly string[] SfxObjectNames = { "SFX_Breath", "SFX_Scream", "SFX_Steps" };

    [MenuItem("Tools/Mockups/Wire Cave2 SFX Volume")]
    public static void Setup()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/cave2.unity", OpenSceneMode.Single);

        int wired = 0;
        foreach (var name in SfxObjectNames)
        {
            var go = GameObject.Find(name);
            if (go == null)
            {
                Debug.LogWarning($"Cave2SfxSetup: '{name}' not found.");
                continue;
            }
            if (go.GetComponent<AudioSource>() == null)
            {
                Debug.LogWarning($"Cave2SfxSetup: '{name}' has no AudioSource.");
                continue;
            }
            if (go.GetComponent<SfxSource>() == null)
                go.AddComponent<SfxSource>();
            wired++;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"Cave2SfxSetup: wired {wired} SFX objects to the SFX volume slider.");
    }
}
