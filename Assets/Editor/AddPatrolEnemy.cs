using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class AddPatrolEnemy
{
    private const string PrefKey = "AddPatrolEnemy_SpikyRed_v1_done";
    private const string EnemyName = "Enemy_SpikyRed";
    private const string ScenePath = "Assets/Scenes/1room.unity";

    private static readonly string[] FramePaths =
    {
        "Assets/Art/Enemies/SpikyRed/spiky_red_walk_0.png",
        "Assets/Art/Enemies/SpikyRed/spiky_red_walk_1.png",
        "Assets/Art/Enemies/SpikyRed/spiky_red_walk_2.png",
        "Assets/Art/Enemies/SpikyRed/spiky_red_walk_3.png",
    };

    static AddPatrolEnemy()
    {
        EditorApplication.delayCall += Run;
    }

    [MenuItem("Tools/Add Patrol Enemy To Scene")]
    private static void RunFromMenu()
    {
        EditorPrefs.DeleteKey(PrefKey);
        Run();
    }

    private static void Run()
    {
        if (EditorPrefs.GetBool(PrefKey, false)) return;

        AssetDatabase.Refresh();

        var scene = FindOrOpenScene();
        if (!scene.IsValid())
        {
            Debug.LogWarning("AddPatrolEnemy: сцену " + ScenePath + " не найти/не открыть, повтор позже.");
            EditorApplication.delayCall += Run;
            return;
        }

        if (GameObject.Find(EnemyName) != null)
        {
            EditorPrefs.SetBool(PrefKey, true);
            return;
        }

        var sprites = new Sprite[FramePaths.Length];
        for (int i = 0; i < FramePaths.Length; i++)
        {
            if (!ConfigurePixelArtImport(FramePaths[i]))
            {
                EditorApplication.delayCall += Run;
                return;
            }
            sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(FramePaths[i]);
        }

        if (sprites.Any(s => s == null))
        {
            EditorApplication.delayCall += Run;
            return;
        }

        var go = new GameObject(EnemyName);
        SceneManager.MoveGameObjectToScene(go, scene);
        go.transform.position = new Vector3(10f, 5f, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprites[0];
        sr.sortingOrder = 1;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.6f, 0.55f);

        var patrol = go.AddComponent<EnemyPatrol>();
        patrol.speed = 1.2f;
        patrol.patrolDistance = 2.5f;
        patrol.frameTime = 0.18f;
        patrol.walkFrames = sprites;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorPrefs.SetBool(PrefKey, true);
        Debug.Log("AddPatrolEnemy: враг '" + EnemyName + "' добавлен в сцену '" + scene.name + "' в позиции " + go.transform.position + " и сцена сохранена.");
    }

    private static Scene FindOrOpenScene()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (s.path == ScenePath) return s;
        }

        if (System.IO.File.Exists(ScenePath))
        {
            return EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
        }

        return default;
    }

    private static bool ConfigurePixelArtImport(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return false;

        bool changed = false;
        if (importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; changed = true; }
        if (importer.spriteImportMode != SpriteImportMode.Single) { importer.spriteImportMode = SpriteImportMode.Single; changed = true; }
        if (importer.filterMode != FilterMode.Point) { importer.filterMode = FilterMode.Point; changed = true; }
        if (importer.spritePixelsPerUnit != 32) { importer.spritePixelsPerUnit = 32; changed = true; }
        if (importer.textureCompression != TextureImporterCompression.Uncompressed) { importer.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }
        if (!importer.alphaIsTransparency) { importer.alphaIsTransparency = true; changed = true; }
        if (importer.mipmapEnabled) { importer.mipmapEnabled = false; changed = true; }

        if (changed)
        {
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }

        return true;
    }
}
