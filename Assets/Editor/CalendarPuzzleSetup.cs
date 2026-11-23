#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// One-time setup for the Level 2 calendar puzzle.
// Open Assets/Scenes/level 0.unity, then use the menu: Tools > Setup Calendar Puzzle.
// It converts the four old "Reveal Date" blocks, adds the CalendarZone, guide text,
// and 30-second timer, removes the old RevealDate script and prefab, and saves the scene.
public static class CalendarPuzzleSetup
{
    private const string SceneName = "level 0";
    private const string OldBlockName = "Reveal Date";
    private const string OldPrefabPath = "Assets/prefab/Reveal Date.prefab";
    private const string OldScriptPath = "Assets/RevealDate.cs";
    private const string GuideMessage = "Reveal the 4 hidden dates";

    // "Reveal Date(15)" -> "15"
    private static string ParseDateNumber(string objectName)
    {
        int open = objectName.IndexOf('(');
        int close = objectName.IndexOf(')');
        if (open >= 0 && close > open)
            return objectName.Substring(open + 1, close - open - 1).Trim();
        return objectName;
    }

    [MenuItem("Tools/Setup Calendar Puzzle")]
    public static void Setup()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name != SceneName)
        {
            EditorUtility.DisplayDialog("Calendar setup",
                "Open Assets/Scenes/level 0.unity first. Nothing was changed.", "OK");
            return;
        }

        // 1. Find the four date blocks, top row first, then left to right.
        List<Transform> blocks = Object.FindObjectsOfType<Transform>(true)
            .Where(t => t.name.StartsWith(OldBlockName) && t.gameObject.scene == scene)
            .OrderByDescending(t => Mathf.Round(t.position.y * 2f))
            .ThenBy(t => t.position.x)
            .ToList();

        if (blocks.Count != 4)
        {
            EditorUtility.DisplayDialog("Calendar setup",
                "Expected 4 '" + OldBlockName + "' objects but found " + blocks.Count +
                ". Nothing was changed.", "OK");
            return;
        }

        Transform calendar = FindRootNamed(scene, "Calendar_1");
        Transform canvas = FindRootNamed(scene, "Canvas");
        if (calendar == null || canvas == null)
        {
            EditorUtility.DisplayDialog("Calendar setup",
                "Could not find 'Calendar_1' or 'Canvas' in this scene. Nothing was changed.", "OK");
            return;
        }

        List<CalendarDateBlock> blockScripts = new List<CalendarDateBlock>();

        // 2. Convert each block into a CalendarDateBlock.
        for (int i = 0; i < 4; i++)
        {
            GameObject go = blocks[i].gameObject;

            // Detach from the old prefab so deleting the prefab later does not break the scene.
            if (PrefabUtility.IsPartOfPrefabInstance(go))
            {
                PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely,
                    InteractionMode.AutomatedAction);
            }

            // Remove the old RevealDate script (must happen before the script file is deleted).
            foreach (MonoBehaviour mb in go.GetComponents<MonoBehaviour>())
            {
                if (mb != null && mb.GetType().Name == "RevealDate")
                {
                    Object.DestroyImmediate(mb);
                }
            }
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);

            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();

            BoxCollider2D col = go.GetComponent<BoxCollider2D>();
            if (col == null) col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;

            string num = ParseDateNumber(blocks[i].name);
            CalendarDateBlock block = go.GetComponent<CalendarDateBlock>();
            if (block == null) block = go.AddComponent<CalendarDateBlock>();

            block.hiddenSprite = sr.sprite;
            block.dateNumber = num;
            block.numberLabel = CreateNumberLabel(go.transform, sr, num);

            go.name = "Date Block " + num;
            blockScripts.Add(block);
        }

        // 3. Calendar zone: a trigger covering the calendar, where the guide appears.
        GameObject zone = new GameObject("CalendarZone");
        zone.transform.SetParent(calendar, false);
        zone.transform.localPosition = Vector3.zero;

        SpriteRenderer calSr = calendar.GetComponent<SpriteRenderer>();
        Vector2 calSize = (calSr != null && calSr.sprite != null)
            ? (Vector2)calSr.sprite.bounds.size
            : new Vector2(4f, 4f);

        BoxCollider2D zoneCol = zone.AddComponent<BoxCollider2D>();
        zoneCol.isTrigger = true;
        zoneCol.size = calSize * 1.1f;

        CalendarPuzzle puzzle = zone.AddComponent<CalendarPuzzle>();
        puzzle.dateBlocks = blockScripts.ToArray();
        foreach (CalendarDateBlock b in blockScripts)
        {
            b.puzzle = puzzle;
        }

        // 4. Guide dialogue (bottom centre of the screen).
        GameObject guide = CreateUIObject("Calendar Guide", canvas,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(640f, 90f));
        Image guideBg = guide.AddComponent<Image>();
        guideBg.color = new Color(0f, 0f, 0f, 0.8f);

        GameObject guideTextObj = CreateStretchedObject("Guide Text", guide.transform);
        TextMeshProUGUI guideText = guideTextObj.AddComponent<TextMeshProUGUI>();
        guideText.text = GuideMessage;
        guideText.fontSize = 36;
        guideText.alignment = TextAlignmentOptions.Center;
        guideText.color = Color.white;

        puzzle.guidePanel = guide;
        puzzle.guideText = guideText;

        // 5. Timer (top right of the screen).
        GameObject timerObj = CreateUIObject("Calendar Timer", canvas,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-60f, -60f), new Vector2(150f, 80f));
        TextMeshProUGUI timerText = timerObj.AddComponent<TextMeshProUGUI>();
        timerText.text = "30";
        timerText.fontSize = 48;
        timerText.alignment = TextAlignmentOptions.Center;
        timerText.color = Color.yellow;

        puzzle.timerText = timerText;
        puzzle.timeLimit = 30f;

        // Both start hidden; the puzzle shows them while Alex plays.
        guide.SetActive(false);
        timerObj.SetActive(false);

        // 6. Remove the old experiment: the prefab and the script.
        AssetDatabase.DeleteAsset(OldPrefabPath);
        AssetDatabase.DeleteAsset(OldScriptPath);

        // 7. Save the scene.
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorUtility.DisplayDialog("Calendar setup",
            "Done. The four date blocks, CalendarZone, guide text, and 30-second timer are set up " +
            "and the scene is saved. Press Play to test. Check the date blocks' positions " +
            "and the number size if they do not line up with the calendar.", "OK");
    }

    // The revealed date number, shown on top of the block.
    private static TextMeshPro CreateNumberLabel(Transform parent, SpriteRenderer sr, string text)
    {
        GameObject go = new GameObject("Date Number");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(0f, 0f, -0.01f);

        TextMeshPro tmp = go.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = 6;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.black;

        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        mr.sortingLayerID = sr.sortingLayerID;
        mr.sortingOrder = sr.sortingOrder + 1;

        go.SetActive(false);
        return tmp;
    }

    private static Transform FindRootNamed(Scene scene, string name)
    {
        return Object.FindObjectsOfType<Transform>(true)
            .FirstOrDefault(t => t.name == name && t.gameObject.scene == scene);
    }

    private static GameObject CreateUIObject(string name, Transform parent,
        Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        return go;
    }

    private static GameObject CreateStretchedObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(10f, 5f);
        rt.offsetMax = new Vector2(-10f, -5f);
        return go;
    }
}
#endif
