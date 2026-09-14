using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class MockupGrayColliders
{
    // Tile IDs (baseR, baseC) identified as solid walls — confirmed both by their
    // brightness signature (bright "cap" band on the edge that faces outward/up)
    // and by how consistently they're used along room perimeters in the tile grid.
    static readonly HashSet<(int, int)> WallTileIds = new HashSet<(int, int)>
    {
        (0, 0), (0, 1), (0, 3), (0, 10), (0, 11), (0, 12),
        (1, 10), (1, 12),
        (2, 10), (2, 11), (2, 12),
        (3, 4),
        (4, 0), (4, 2), (4, 3), (4, 4), (4, 5), (4, 6),
    };

    static readonly (int, int) WaterTileId = (4, 12);

    // The small walled platform in the water room (rows 17-19, cols 26-30) — the
    // player spawns here. (20,29) blocks the east side of the stairs at (20,28) so
    // they can only be climbed from the west.
    static readonly HashSet<(int, int)> ForceWall = new HashSet<(int, int)>
    {
        (20, 29),
    };

    // (19,28): the platform's south wall gap, aligned with the stair prop at (20,28).
    // (18,25): the water tile under the wooden bridge prop — the player spawns on the
    // platform, so this crossing needs to stay usable to get back out.
    static readonly HashSet<(int, int)> ForceOpen = new HashSet<(int, int)>
    {
        (19, 28),
        (18, 25),
    };

    // Thin collision skin instead of a full 1x1 block, so the player can walk right
    // up against the wall/water sprite instead of stopping half a tile early.
    const float EdgeThickness = 0.12f;

    [MenuItem("Tools/Mockups/Add Gray Level Colliders")]
    public static void AddColliders()
    {
        string dataPath = Application.dataPath + "/Editor/MockupBuilderData/flat_gray.json";
        var json = File.ReadAllText(dataPath);
        var data = JsonUtility.FromJson<MockupTilemapBuilder.GridData>(json);

        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Mockup_Gray.unity", OpenSceneMode.Single);

        // Clean slate every run so classification/thickness changes always take
        // effect, instead of leaving stale colliders from a previous pass.
        var groundParent = GameObject.Find("Level/Ground");
        if (groundParent != null)
        {
            foreach (Transform child in groundParent.transform)
            {
                foreach (var existing in child.GetComponents<BoxCollider2D>())
                    Object.DestroyImmediate(existing);
            }
        }

        var blocked = new HashSet<(int, int)>();
        var walkable = new HashSet<(int, int)>();
        foreach (var cell in data.cells)
        {
            if (!cell.hasBase) continue;
            var pos = (cell.r, cell.c);

            bool isWater = cell.baseR == WaterTileId.Item1 && cell.baseC == WaterTileId.Item2;
            bool isWall = WallTileIds.Contains((cell.baseR, cell.baseC));
            if (ForceOpen.Contains(pos)) { isWall = false; isWater = false; }
            if (ForceWall.Contains(pos)) isWall = true;

            if (isWall || isWater) blocked.Add(pos);
            else walkable.Add(pos);
        }

        int tileCount = 0, skipped = 0;

        foreach (var pos in blocked)
        {
            var go = GameObject.Find($"Ground_{pos.Item1}_{pos.Item2}");
            if (go == null) { skipped++; continue; }

            bool any = false;
            any |= TryAddEdge(go, walkable, (pos.Item1 - 1, pos.Item2), 0, 1);   // north neighbour walkable -> thin edge on north side
            any |= TryAddEdge(go, walkable, (pos.Item1 + 1, pos.Item2), 0, -1);  // south
            any |= TryAddEdge(go, walkable, (pos.Item1, pos.Item2 - 1), -1, 0);  // west
            any |= TryAddEdge(go, walkable, (pos.Item1, pos.Item2 + 1), 1, 0);   // east

            if (any) tileCount++;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"MockupGrayColliders: added thin edge colliders to {tileCount} tiles ({skipped} cells had no matching GameObject).");
    }

    static bool TryAddEdge(GameObject go, HashSet<(int, int)> walkable, (int, int) neighbor, int dx, int dy)
    {
        if (!walkable.Contains(neighbor)) return false;

        var col = go.AddComponent<BoxCollider2D>();
        if (dy != 0)
        {
            col.size = new Vector2(1f, EdgeThickness);
            col.offset = new Vector2(0f, dy * (0.5f - EdgeThickness / 2f));
        }
        else
        {
            col.size = new Vector2(EdgeThickness, 1f);
            col.offset = new Vector2(dx * (0.5f - EdgeThickness / 2f), 0f);
        }
        return true;
    }
}
