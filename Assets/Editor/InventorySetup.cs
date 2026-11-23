using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

// Tools > Setup Inventory Items
// Links the designed inventory icons (already in the Canvas) to their item names and hides them,
// so the inventory starts empty and fills only when items are collected.
public static class InventorySetup
{
    [MenuItem("Tools/Setup Inventory Items")]
    public static void Setup()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name != "level 0")
        {
            EditorUtility.DisplayDialog("Inventory setup",
                "Open Assets/Scenes/level 0.unity first. Nothing was changed.", "OK");
            return;
        }

        InventoryManager inventory = Object.FindObjectOfType<InventoryManager>();
        if (inventory == null || inventory.itemsContainer == null)
        {
            EditorUtility.DisplayDialog("Inventory setup",
                "InventoryManager or Items Container is missing. Nothing was changed.", "OK");
            return;
        }

        // Designed icon object name in the scene -> item name used by the pickups.
        string[,] map =
        {
            { "Key", "Key" },
            { "Sabrina Note", "Sabrina Notes" },
            { "Triangle Symbol", "Triangle Symbol" },
            { "Library permit", "Library Permit" }
        };

        List<InventoryManager.ItemIcon> list = new List<InventoryManager.ItemIcon>();
        List<string> missing = new List<string>();

        for (int i = 0; i < map.GetLength(0); i++)
        {
            GameObject icon = FindIcon(scene, inventory.itemsContainer, map[i, 0]);
            if (icon == null)
            {
                missing.Add(map[i, 0]);
                continue;
            }

            list.Add(new InventoryManager.ItemIcon { itemName = map[i, 1], icon = icon });
            icon.SetActive(false);
        }

        inventory.itemIcons = list.ToArray();

        EditorUtility.SetDirty(inventory);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        string note = missing.Count > 0 ? " Not found: " + string.Join(", ", missing) + "." : "";
        EditorUtility.DisplayDialog("Inventory setup",
            "Done. " + list.Count + " designed icons are linked and hidden. The inventory starts empty." + note +
            " The scene is saved.", "OK");
    }

    static GameObject FindIcon(Scene scene, Transform container, string name)
    {
        foreach (RectTransform rt in Object.FindObjectsOfType<RectTransform>(true))
        {
            if (rt.name != name || rt.gameObject.scene != scene) continue;
            if (!rt.IsChildOf(container)) continue;
            return rt.gameObject;
        }
        return null;
    }
}
