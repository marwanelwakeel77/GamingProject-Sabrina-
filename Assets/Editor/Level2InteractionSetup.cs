using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;

// Tools > Setup Level 2 Interactions
// Adds the hint line, the safe keypad (with its panel), and the exit door trigger to "level 0".
// Run it once, after "Setup Calendar Puzzle" and "Setup Triangle Pickup".
public static class Level2InteractionSetup
{
    [MenuItem("Tools/Setup Level 2 Interactions")]
    public static void Setup()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name != "level 0")
        {
            EditorUtility.DisplayDialog("Level 2 setup",
                "Open Assets/Scenes/level 0.unity first. Nothing was changed.", "OK");
            return;
        }

        Canvas canvas = FindComponent<Canvas>(scene, "Canvas");
        Transform safeT = FindObject(scene, "safe_0");
        Transform doorT = FindObject(scene, "exit door");
        Transform permitT = FindObject(scene, "Library permit");
        CalendarPuzzle calendar = Object.FindObjectOfType<CalendarPuzzle>();

        List<string> missing = new List<string>();
        if (canvas == null) missing.Add("Canvas");
        if (safeT == null) missing.Add("safe_0");
        if (doorT == null) missing.Add("exit door");
        if (calendar == null) missing.Add("CalendarZone (run Setup Calendar Puzzle first)");
        if (missing.Count > 0)
        {
            EditorUtility.DisplayDialog("Level 2 setup",
                "Missing in the scene: " + string.Join(", ", missing) + ". Nothing was changed.", "OK");
            return;
        }

        // 1. Hint line at the top of the screen.
        GameObject hintGo = CreateUI("Hint Text", canvas.transform);
        RectTransform hintRect = hintGo.GetComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0.5f, 1f);
        hintRect.anchorMax = new Vector2(0.5f, 1f);
        hintRect.pivot = new Vector2(0.5f, 1f);
        hintRect.anchoredPosition = new Vector2(0f, -90f);
        hintRect.sizeDelta = new Vector2(1000f, 60f);
        TextMeshProUGUI hintTmp = hintGo.AddComponent<TextMeshProUGUI>();
        hintTmp.text = "";
        hintTmp.fontSize = 34;
        hintTmp.alignment = TextAlignmentOptions.Center;
        hintTmp.color = Color.white;
        hintGo.AddComponent<HintText>();

        // 2. Safe panel with the keypad (hidden until Alex opens the safe).
        GameObject panel = CreateUI("Safe Panel", canvas.transform);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = panelRect.anchorMax = panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(520f, 560f);
        Image panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0.1f, 0.08f, 0.06f, 0.95f);

        TextMeshProUGUI title = AddLabel(panel.transform, "Title", "SAFE", new Vector2(0f, 240f), new Vector2(480f, 60f), 44);
        TextMeshProUGUI entryText = AddLabel(panel.transform, "Entry Text", "Number: _", new Vector2(0f, 175f), new Vector2(480f, 50f), 32);
        TextMeshProUGUI slotsText = AddLabel(panel.transform, "Slots Text", "? ? ? ?", new Vector2(0f, 120f), new Vector2(480f, 50f), 38);

        SafeKeypad keypad = safeT.gameObject.GetComponent<SafeKeypad>();
        if (keypad == null) keypad = safeT.gameObject.AddComponent<SafeKeypad>();
        keypad.keypadPanel = panel;
        keypad.entryText = entryText;
        keypad.slotsText = slotsText;
        keypad.calendar = calendar;
        keypad.permitSprite = permitT != null ? permitT.GetComponent<Image>()?.sprite : null;
        keypad.code = new int[] { 5, 15, 25, 29 };

        // Keypad layout: 1 2 3 / 4 5 6 / 7 8 9 / < 0 OK
        float[] columns = { -130f, 0f, 130f };
        float[] rows = { 30f, -55f, -140f, -225f };
        int[,] layout = { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 }, { -1, 0, -2 } };
        for (int r = 0; r < 4; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                int value = layout[r, c];
                string label = value == -1 ? "<" : value == -2 ? "OK" : value.ToString();
                CreateKeypadButton(panel.transform, "Key " + label, label,
                    new Vector2(columns[c], rows[r]), keypad, value);
            }
        }

        CreateKeypadButton(panel.transform, "Key Close", "X",
            new Vector2(230f, 245f), keypad, -3, new Vector2(60f, 60f));

        panel.SetActive(false);

        // 3. Safe trigger (so E works when Alex is next to the safe).
        BoxCollider2D safeCol = EnsureTrigger(safeT.gameObject, 1.5f);

        // 4. Exit door trigger and locking script.
        EnsureTrigger(doorT.gameObject, 1.5f);
        ExitDoor door = doorT.gameObject.GetComponent<ExitDoor>();
        if (door == null) door = doorT.gameObject.AddComponent<ExitDoor>();
        door.requiredItem = "Library Permit";
        door.nextScene = "level 3";

        EditorUtility.SetDirty(keypad);
        EditorUtility.SetDirty(door);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorUtility.DisplayDialog("Level 2 setup",
            "Done. The safe keypad (code 5, 15, 25, 29), the hint line and the exit door lock are set up. " +
            "The scene is saved. Press Play to test.", "OK");
    }

    static void CreateKeypadButton(Transform parent, string name, string label, Vector2 position,
        SafeKeypad keypad, int value, Vector2? size = null)
    {
        GameObject go = CreateUI(name, parent);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size ?? new Vector2(110f, 70f);

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.55f, 0.35f, 0.18f);
        Button button = go.AddComponent<Button>();
        button.targetGraphic = img;

        KeypadButton kb = go.AddComponent<KeypadButton>();
        kb.keypad = keypad;
        kb.digit = value;

        AddLabel(go.transform, "Label", label, Vector2.zero, rect.sizeDelta, 36);
    }

    static TextMeshProUGUI AddLabel(Transform parent, string name, string text, Vector2 position, Vector2 size, int fontSize)
    {
        GameObject go = CreateUI(name, parent);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        return tmp;
    }

    static GameObject CreateUI(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static BoxCollider2D EnsureTrigger(GameObject go, float scale)
    {
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        BoxCollider2D col = go.GetComponent<BoxCollider2D>();
        if (col == null) col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        if (sr != null && sr.sprite != null) col.size = sr.sprite.bounds.size * scale;
        return col;
    }

    static Transform FindObject(Scene scene, string name)
    {
        foreach (Transform t in Object.FindObjectsOfType<Transform>(true))
        {
            if (t.name == name && t.gameObject.scene == scene) return t;
        }
        return null;
    }

    static T FindComponent<T>(Scene scene, string name) where T : Component
    {
        Transform t = FindObject(scene, name);
        return t != null ? t.GetComponent<T>() : null;
    }
}
