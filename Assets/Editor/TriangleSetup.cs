#if UNITY_EDITOR
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// One-time setup for the wall Triangle Symbol.
// Run this AFTER Tools > Setup Calendar Puzzle, with Assets/Scenes/level 0.unity open.
public static class TriangleSetup
{
    private const string SceneName = "level 0";
    private const string SlotPrefabPath = "Assets/prefab/Item Slot.prefab";

    [MenuItem("Tools/Setup Triangle Pickup")]
    public static void Setup()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name != SceneName)
        {
            EditorUtility.DisplayDialog("Triangle setup",
                "Open Assets/Scenes/level 0.unity first. Nothing was changed.", "OK");
            return;
        }

        // The wall triangle is the world-space one (it has a SpriteRenderer, not a UI Image).
        SpriteRenderer[] triangles = Object.FindObjectsOfType<SpriteRenderer>(true)
            .Where(s => s.name == "Triangle Symbol" && s.gameObject.scene == scene)
            .ToArray();

        CalendarPuzzle puzzle = Find<CalendarPuzzle>(scene);
        InventoryManager inventory = Find<InventoryManager>(scene);
        Transform container = FindTransform(scene, "Items Container");
        GameObject slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SlotPrefabPath);

        if (triangles.Length != 1 || puzzle == null || inventory == null || container == null || slotPrefab == null)
        {
            EditorUtility.DisplayDialog("Triangle setup",
                "Could not find everything needed (one wall 'Triangle Symbol', the calendar puzzle, " +
                "the InventoryManager, 'Items Container', and prefab/Item Slot). " +
                "Run Tools > Setup Calendar Puzzle first. Nothing was changed.", "OK");
            return;
        }

        GameObject wall = triangles[0].gameObject;

        // Trigger zone so Alex can stand near the triangle.
        BoxCollider2D col = wall.GetComponent<BoxCollider2D>();
        if (col == null) col = wall.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        if (triangles[0].sprite != null)
        {
            col.size = triangles[0].sprite.bounds.size * 1.5f;
        }

        WallPickup pickup = wall.GetComponent<WallPickup>();
        if (pickup == null) pickup = wall.AddComponent<WallPickup>();
        pickup.itemName = "Triangle Symbol";
        pickup.requiredPuzzle = puzzle;

        // Wire the inventory to its panel's items container and slot prefab.
        inventory.itemsContainer = container;
        inventory.itemSlotPrefab = slotPrefab;

        EditorUtility.SetDirty(inventory);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorUtility.DisplayDialog("Triangle setup",
            "Done. Alex can collect the wall triangle with E after the calendar is solved. " +
            "The scene is saved. Press Play to test.", "OK");
    }

    private static T Find<T>(Scene scene) where T : Object
    {
        return Object.FindObjectsOfType<T>(true)
            .FirstOrDefault(o => (o as Component) != null && ((Component)(object)o).gameObject.scene == scene);
    }

    private static Transform FindTransform(Scene scene, string name)
    {
        return Object.FindObjectsOfType<Transform>(true)
            .FirstOrDefault(t => t.name == name && t.gameObject.scene == scene);
    }
}
#endif
