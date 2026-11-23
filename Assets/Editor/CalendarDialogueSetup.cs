using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;

// Tools > Setup Calendar Dialogue
// Builds the instruction panel above the calendar (with an X button) and removes the old guide box.
public static class CalendarDialogueSetup
{
    [MenuItem("Tools/Setup Calendar Dialogue")]
    public static void Setup()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name != "level 0")
        {
            EditorUtility.DisplayDialog("Calendar dialogue",
                "Open Assets/Scenes/level 0.unity first. Nothing was changed.", "OK");
            return;
        }

        Canvas canvas = FindComponent<Canvas>(scene, "Canvas");
        CalendarPuzzle puzzle = Object.FindObjectOfType<CalendarPuzzle>();
        Transform oldGuide = FindObject(scene, "Calendar Guide");
        if (canvas == null || puzzle == null)
        {
            EditorUtility.DisplayDialog("Calendar dialogue",
                "Canvas or CalendarZone is missing. Nothing was changed.", "OK");
            return;
        }

        // Remove any previous version of the panel.
        Transform old = canvas.transform.Find("Calendar Dialogue");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        // Panel above the calendar.
        GameObject panel = CreateUI("Calendar Dialogue", canvas.transform);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -30f);
        rect.sizeDelta = new Vector2(760f, 150f);

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.06f, 0.03f, 0.04f, 0.88f);
        bg.raycastTarget = false;

        Outline border = panel.AddComponent<Outline>();
        border.effectColor = new Color(1f, 0.55f, 0.2f, 0.9f);
        border.effectDistance = new Vector2(3f, -3f);

        CanvasGroup group = panel.AddComponent<CanvasGroup>();

        // Message text.
        AddText(panel.transform, "Message",
            "REVEAL THE FOUR DATES & SOLVE THE PUZZLE",
            new Vector2(0f, -45f), new Vector2(700f, 60f), 32, new Color(1f, 0.85f, 0.55f));

        // Controls line.
        AddText(panel.transform, "Controls",
            "E: break the selected block     Left/Right: choose     Esc: close calendar",
            new Vector2(0f, -100f), new Vector2(700f, 36f), 22, new Color(0.8f, 0.75f, 0.7f));

        // X button in the upper-right corner.
        GameObject closeGo = CreateUI("Close Button", panel.transform);
        RectTransform closeRect = closeGo.GetComponent<RectTransform>();
        closeRect.anchorMin = closeRect.anchorMax = closeRect.pivot = new Vector2(1f, 1f);
        closeRect.anchoredPosition = new Vector2(-10f, -10f);
        closeRect.sizeDelta = new Vector2(44f, 44f);
        Image closeImg = closeGo.AddComponent<Image>();
        closeImg.color = new Color(0.45f, 0.12f, 0.1f);
        Button closeButton = closeGo.AddComponent<Button>();
        closeButton.targetGraphic = closeImg;
        AddText(closeGo.transform, "Label", "X", Vector2.zero, new Vector2(44f, 44f), 28, Color.white);

        // Small pointer towards the calendar.
        GameObject pointer = CreateUI("Pointer", panel.transform);
        RectTransform pointerRect = pointer.GetComponent<RectTransform>();
        pointerRect.anchorMin = pointerRect.anchorMax = pointerRect.pivot = new Vector2(0.5f, 1f);
        pointerRect.anchoredPosition = new Vector2(0f, 14f);
        pointerRect.sizeDelta = new Vector2(26f, 26f);
        pointerRect.localRotation = Quaternion.Euler(0f, 0f, 45f);
        Image pointerImg = pointer.AddComponent<Image>();
        pointerImg.color = new Color(1f, 0.55f, 0.2f, 0.9f);
        pointerImg.raycastTarget = false;

        // Wire the dialogue.
        CalendarDialogue dialogue = panel.AddComponent<CalendarDialogue>();
        dialogue.closeButton = closeButton;
        panel.SetActive(false);

        puzzle.dialogue = dialogue;
        puzzle.viewPadding = 1.6f;
        puzzle.guidePanel = null;
        if (oldGuide != null) oldGuide.gameObject.SetActive(false);

        EditorUtility.SetDirty(dialogue);
        EditorUtility.SetDirty(puzzle);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorUtility.DisplayDialog("Calendar dialogue",
            "Done. The dialogue appears above the calendar when Alex approaches it. " +
            "The X closes it without resetting the puzzle. The scene is saved.", "OK");
    }

    static void AddText(Transform parent, string name, string text, Vector2 position, Vector2 size, int fontSize, Color color)
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
        tmp.color = color;
        tmp.raycastTarget = false;
    }

    static GameObject CreateUI(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
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
