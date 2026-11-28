#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

//Builds the whole Level 5 scene from the uploaded art: Tools > The Curse of Sabrina > Build Level 5 Scene
//It sets the sprite import settings, slices the sheets, makes the animations, prefabs, canvases,
//connects every Inspector reference, saves Assets/Scenes/level 5.unity and adds it to the Build Settings.
public static class Level5Builder
{
    const string ROOT = "Assets/Level 5";
    const string ART = ROOT + "/Art";
    const string UIART = ROOT + "/Art/UI";
    const string AUDIO = ROOT + "/Audio";
    const string ANIM = ROOT + "/Animations";
    const string PREFABS = ROOT + "/Prefabs";
    const string SCENE = "Assets/Scenes/level 5.unity";
    const string L2 = "Assets/Level 2 Assets";

    static readonly Color Gold = new Color(1f, 0.82f, 0.48f, 1f);
    static readonly Color Ivory = new Color(0.98f, 0.92f, 0.82f, 1f);
    static readonly Color Blood = new Color(0.91f, 0.22f, 0.16f, 1f);

    static TMP_FontAsset font;
    static Sprite white, glow;

    // ------------------------------------------------------------------ menu

    [MenuItem("Tools/The Curse of Sabrina/Build Level 5 Scene")]
    public static void BuildMenu()
    {
        if (File.Exists(SCENE) &&
            !EditorUtility.DisplayDialog("Build Level 5", "'" + SCENE + "' already exists. Build it again from the Level 5 assets? (Changes made by hand in that scene will be replaced.)", "Build", "Cancel"))
            return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Build();
    }

    [MenuItem("Tools/The Curse of Sabrina/Set Up Level 5 Sprites Only")]
    public static void SpritesMenu()
    {
        ImportAll();
        Debug.Log("Level 5: sprite and audio import settings applied.");
    }

    public static void Build()
    {
        ImportAll();
        font = TMP_Settings.defaultFontAsset;
        if (font == null) font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        white = LoadSprite(ART + "/White.png");
        glow = LoadSprite(ART + "/SoftGlow.png");
        EnsureFolder(ANIM); EnsureFolder(PREFABS);

        AddTag("Enemy");
        AddTag("Wall");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        BuildScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, SCENE);
        AddToBuildSettings(SCENE);
        AssetDatabase.SaveAssets();
        Debug.Log("Level 5 built and saved to " + SCENE + ". Press Play to test it.");
        EditorUtility.DisplayDialog("Level 5", "Level 5 was built and saved to\n" + SCENE + "\n\nPress Play to test it.", "OK");
    }

    // ------------------------------------------------------------------ import

    static void ImportAll()
    {
        Single(ART + "/Final_Room_Background.png", 25.6f, new Vector2(0.5f, 0.5f), false);
        Single(ART + "/Final_Room_Platform.png", 100f, new Vector2(0.5f, 1f), true);
        Single(ART + "/Final_Room_Candles.png", 250f, new Vector2(0.5f, 0f), false);
        Single(ART + "/Sabrina_Device.png", 194f, new Vector2(0.5f, 0f), false);
        Single(ART + "/Bullet.png", 1500f, new Vector2(0.5f, 0.5f), false);
        Single(ART + "/White.png", 8f, new Vector2(0.5f, 0.5f), true);
        Single(ART + "/SoftGlow.png", 256f, new Vector2(0.5f, 0.5f), false);
        Grid(ART + "/Switch_Sheet.png", "Switch", 540, 815, new[] { 2 }, 300f, new Vector2(0.5f, 0f));
        Grid(ART + "/Door_Sheet.png", "Door", 640, 645, new[] { 3 }, 160f, new Vector2(0.5f, 0f));
        Grid(ART + "/Spike_Trap_Sheet.png", "Spikes", 776, 340, new[] { 2 }, 300f, new Vector2(0.5f, 0f));
        Grid(ART + "/Collapsing_Floor_Sheet.png", "Collapse", 262, 222, new[] { 8 }, 112f, new Vector2(0.5f, 1f));
        GhostSheet(ART + "/Ghost_Sheet.png");

        foreach (var n in new[] { "Symbol_Sun", "Symbol_Moon", "Symbol_Triangle" })
            Single(UIART + "/" + n + ".png", 194f, new Vector2(0.5f, 0.5f), false);
        Single(UIART + "/Hint_Frame.png", 100f, new Vector2(0.5f, 0.5f), false);
        Single(UIART + "/Close_Button.png", 100f, new Vector2(0.5f, 0.5f), false);
        foreach (var n in new[] { "Level5_Start_Screen", "Sabrina_Message_Screen", "Level5_End_Screen", "Device_Puzzle_Frame", "Device_Timer_Frame", "Gothic_Button_Frame", "End_Button_Frame" })
            Single(UIART + "/" + n + ".png", 100f, new Vector2(0.5f, 0.5f), false);

        //long music is streamed, short sounds are loaded in memory
        foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new[] { AUDIO }))
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            var ai = AssetImporter.GetAtPath(p) as AudioImporter;
            if (ai == null) continue;
            var s = ai.defaultSampleSettings;
            s.loadType = (p.Contains("Music") || p.Contains("Ghost.mp3")) ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
            ai.defaultSampleSettings = s;
            ai.SaveAndReimport();
        }
    }

    static TextureImporter Base(string path, float ppu)
    {
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) { Debug.LogError("Level 5: missing art file " + path); return null; }
        ti.textureType = TextureImporterType.Sprite;
        ti.spritePixelsPerUnit = ppu;
        ti.mipmapEnabled = false;
        ti.alphaIsTransparency = true;
        ti.filterMode = FilterMode.Bilinear;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.maxTextureSize = 4096;
        ti.wrapMode = TextureWrapMode.Clamp;
        return ti;
    }

    static void Single(string path, float ppu, Vector2 pivot, bool fullRect)
    {
        var ti = Base(path, ppu); if (ti == null) return;
        ti.spriteImportMode = SpriteImportMode.Single;
        var st = new TextureImporterSettings();
        ti.ReadTextureSettings(st);
        st.spriteAlignment = (int)SpriteAlignment.Custom;
        st.spritePivot = pivot;
        st.spriteMeshType = fullRect ? SpriteMeshType.FullRect : SpriteMeshType.Tight;
        ti.SetTextureSettings(st);
        if (fullRect) ti.wrapMode = TextureWrapMode.Repeat;
        ti.SaveAndReimport();
    }

    //equal cells, one row per entry of counts (top row first)
    static void Grid(string path, string prefix, int cw, int ch, int[] counts, float ppu, Vector2 pivot)
    {
        var ti = Base(path, ppu); if (ti == null) return;
        ti.spriteImportMode = SpriteImportMode.Multiple;
        var list = new List<SpriteMetaData>();
        int rows = counts.Length;
        for (int r = 0; r < rows; r++)
            for (int k = 0; k < counts[r]; k++)
                list.Add(new SpriteMetaData
                {
                    name = prefix + "_" + (rows > 1 ? r + "_" : "") + k,
                    rect = new Rect(k * cw, (rows - 1 - r) * ch, cw, ch),
                    alignment = (int)SpriteAlignment.Custom,
                    pivot = pivot
                });
        ti.spritesheet = list.ToArray();
        ti.SaveAndReimport();
    }

    static void GhostSheet(string path)
    {
        var ti = Base(path, 60f); if (ti == null) return;
        ti.spriteImportMode = SpriteImportMode.Multiple;
        string[] names = { "Float", "Drift", "Attack", "Death" };
        int[] counts = { 7, 8, 8, 7 };
        int cw = 272, ch = 111;
        var list = new List<SpriteMetaData>();
        for (int r = 0; r < 4; r++)
            for (int k = 0; k < counts[r]; k++)
                list.Add(new SpriteMetaData
                {
                    name = "Ghost_" + names[r] + "_" + k,
                    rect = new Rect(k * cw, (3 - r) * ch, cw, ch),
                    alignment = (int)SpriteAlignment.Custom,
                    pivot = new Vector2(0.32f, 0.5f) //the body of the ghost, so flipping turns around her body
                });
        ti.spritesheet = list.ToArray();
        ti.SaveAndReimport();
    }

    static Sprite LoadSprite(string path)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s == null) Debug.LogError("Level 5: sprite not found " + path);
        return s;
    }

    static Sprite[] LoadSprites(string path, string prefix)
    {
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
            .Where(s => s.name.StartsWith(prefix))
            .OrderBy(s => int.Parse(s.name.Substring(s.name.LastIndexOf('_') + 1)))
            .ToArray();
    }

    static AudioClip Clip(string path)
    {
        var c = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        if (c == null) Debug.LogWarning("Level 5: sound not found " + path);
        return c;
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }

    static void AddToBuildSettings(string path)
    {
        var scenes = EditorBuildSettings.scenes.ToList();
        if (scenes.Any(s => s.path == path)) return;
        scenes.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    // ------------------------------------------------------------------ animations

    static AnimationClip MakeClip(string name, Sprite[] frames, float fps, bool loop)
    {
        string path = ANIM + "/" + name + ".anim";
        AssetDatabase.DeleteAsset(path);
        var clip = new AnimationClip { frameRate = fps };
        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        var keys = new ObjectReferenceKeyframe[frames.Length + 1];
        for (int i = 0; i < frames.Length; i++)
            keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = frames[i] };
        //the last picture is held for a full frame
        keys[frames.Length] = new ObjectReferenceKeyframe { time = frames.Length / fps, value = frames[frames.Length - 1] };
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    static AnimatorController GhostAnimator()
    {
        string sheet = ART + "/Ghost_Sheet.png";
        var floatC = MakeClip("Ghost Float", LoadSprites(sheet, "Ghost_Float_"), 9f, true);
        var drift = MakeClip("Ghost Drift", LoadSprites(sheet, "Ghost_Drift_"), 12f, true);
        var attack = MakeClip("Ghost Attack", LoadSprites(sheet, "Ghost_Attack_"), 14f, false);
        var hit = MakeClip("Ghost Hit", LoadSprites(sheet, "Ghost_Drift_").Skip(4).ToArray(), 20f, false);
        var death = MakeClip("Ghost Death", LoadSprites(sheet, "Ghost_Death_"), 6f, false);

        string path = ANIM + "/Ghost.controller";
        AssetDatabase.DeleteAsset(path);
        var ac = AnimatorController.CreateAnimatorControllerAtPath(path);
        //bools like the animations of lab 3: Moving, Attacking, Hurt, Dead
        ac.AddParameter("Moving", AnimatorControllerParameterType.Bool);
        ac.AddParameter("Attacking", AnimatorControllerParameterType.Bool);
        ac.AddParameter("Hurt", AnimatorControllerParameterType.Bool);
        ac.AddParameter("Dead", AnimatorControllerParameterType.Bool);
        var sm = ac.layers[0].stateMachine;
        var sFloat = sm.AddState("Float"); sFloat.motion = floatC; sm.defaultState = sFloat;
        var sDrift = sm.AddState("Drift"); sDrift.motion = drift;
        var sAttack = sm.AddState("Attack"); sAttack.motion = attack;
        var sHit = sm.AddState("Hit"); sHit.motion = hit;
        var sDeath = sm.AddState("Death"); sDeath.motion = death;

        var t = sFloat.AddTransition(sDrift); t.AddCondition(AnimatorConditionMode.If, 0, "Moving"); t.hasExitTime = false; t.duration = 0.1f;
        t = sDrift.AddTransition(sFloat); t.AddCondition(AnimatorConditionMode.IfNot, 0, "Moving"); t.hasExitTime = false; t.duration = 0.1f;

        //from Any State, like Grounded in lab 3 (Has Exit Time unchecked)
        var a = sm.AddAnyStateTransition(sDeath); a.AddCondition(AnimatorConditionMode.If, 0, "Dead"); a.hasExitTime = false; a.duration = 0f; a.canTransitionToSelf = false;
        a = sm.AddAnyStateTransition(sAttack); a.AddCondition(AnimatorConditionMode.If, 0, "Attacking"); a.AddCondition(AnimatorConditionMode.IfNot, 0, "Dead"); a.hasExitTime = false; a.duration = 0f; a.canTransitionToSelf = false;
        a = sm.AddAnyStateTransition(sHit); a.AddCondition(AnimatorConditionMode.If, 0, "Hurt"); a.AddCondition(AnimatorConditionMode.IfNot, 0, "Dead"); a.hasExitTime = false; a.duration = 0f; a.canTransitionToSelf = false;
        t = sAttack.AddTransition(sFloat); t.AddCondition(AnimatorConditionMode.IfNot, 0, "Attacking"); t.hasExitTime = false; t.duration = 0.05f;
        t = sHit.AddTransition(sFloat); t.AddCondition(AnimatorConditionMode.IfNot, 0, "Hurt"); t.hasExitTime = false; t.duration = 0.05f;
        return ac;
    }

    static AnimatorController DoorAnimator(Sprite[] frames)
    {
        var closed = MakeClip("Exit Door Closed", new[] { frames[0] }, 1f, false);
        var open = MakeClip("Exit Door Open", frames, 2.5f, false);
        string path = ANIM + "/Exit Door.controller";
        AssetDatabase.DeleteAsset(path);
        var ac = AnimatorController.CreateAnimatorControllerAtPath(path);
        ac.AddParameter("isOpen", AnimatorControllerParameterType.Bool);
        var sm = ac.layers[0].stateMachine;
        var sClosed = sm.AddState("Closed"); sClosed.motion = closed; sm.defaultState = sClosed;
        var sOpen = sm.AddState("Open"); sOpen.motion = open;
        var t = sClosed.AddTransition(sOpen); t.AddCondition(AnimatorConditionMode.If, 0, "isOpen"); t.hasExitTime = false; t.duration = 0f;
        return ac;
    }

    // ------------------------------------------------------------------ small helpers

    static GameObject GO(string name, Transform parent, Vector3 localPos)
    {
        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        return go;
    }

    static SpriteRenderer SR(GameObject go, Sprite s, string layer, int order)
    {
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = s;
        sr.sortingLayerName = layer;
        sr.sortingOrder = order;
        return sr;
    }

    static BoxCollider2D Box(GameObject go, Vector2 size, Vector2 offset, bool trigger)
    {
        var b = go.AddComponent<BoxCollider2D>();
        b.size = size; b.offset = offset; b.isTrigger = trigger;
        return b;
    }

    static ParticleSystem Particles(string name, Transform parent, Vector3 pos, Color c1, Color c2, float rate, float life,
        float speed, float size, ParticleSystemShapeType shape, Vector3 shapeScale, bool loop, bool playOnAwake, int burst,
        string layer, int order, float gravity = 0f)
    {
        var go = GO(name, parent, pos);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.duration = loop ? 5f : 1.5f;
        main.loop = loop;
        main.playOnAwake = playOnAwake;
        main.startLifetime = new ParticleSystem.MinMaxCurve(life * 0.6f, life);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.4f, speed);
        main.startSize = new ParticleSystem.MinMaxCurve(size * 0.5f, size);
        main.startColor = new ParticleSystem.MinMaxGradient(c1, c2);
        main.gravityModifier = gravity;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 400;
        var em = ps.emission;
        em.rateOverTime = rate;
        if (burst > 0) em.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)burst) });
        var sh = ps.shape;
        sh.shapeType = shape;
        sh.scale = shapeScale;
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                  new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.15f), new GradientAlphaKey(0f, 1f) });
        col.color = g;
        var r = go.GetComponent<ParticleSystemRenderer>();
        r.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-ParticleSystem.mat");
        r.sortingLayerName = layer;
        r.sortingOrder = order;
        return ps;
    }

    static Transform Find(Transform root, string name)
    {
        if (root.name == name) return root;
        foreach (Transform c in root)
        {
            var f = Find(c, name);
            if (f != null) return f;
        }
        return null;
    }

    // UI

    static Transform mainCanvas; //Level 5 uses one Canvas for all its UI

    //a part of the one Canvas (an empty object that fills the screen), so the Hierarchy stays simple
    static Transform Section(string name)
    {
        return UI(name, mainCanvas);
    }

    static Canvas MakeCanvas(string name, int order, bool scaleWithScreen)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = order;
        var cs = go.AddComponent<CanvasScaler>();
        if (scaleWithScreen)
        {
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1920, 1080);
            cs.matchWidthOrHeight = 0.5f;
        }
        else
        {
            //the same settings as the Canvas of level 2, so its Health Bar and Inventory prefabs look the same
            cs.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            cs.referenceResolution = new Vector2(800, 600);
        }
        go.AddComponent<GraphicRaycaster>();
        return c;
    }

    static RectTransform UI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = 5;
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        Anchor(rt, 0, 0, 1, 1);
        return rt;
    }

    static RectTransform Anchor(RectTransform rt, float x0, float y0, float x1, float y1)
    {
        rt.anchorMin = new Vector2(x0, y0);
        rt.anchorMax = new Vector2(x1, y1);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return rt;
    }

    static Image Img(RectTransform rt, Sprite s, Color c, bool preserve = false)
    {
        var i = rt.gameObject.AddComponent<Image>();
        i.sprite = s; i.color = c; i.preserveAspect = preserve; i.raycastTarget = false;
        return i;
    }

    static Image Filled(RectTransform rt, Sprite s, Color c)
    {
        var i = Img(rt, s, c);
        i.type = Image.Type.Filled;
        i.fillMethod = Image.FillMethod.Horizontal;
        i.fillOrigin = 0;
        i.fillAmount = 1f;
        return i;
    }

    static TextMeshProUGUI Txt(RectTransform rt, string text, float size, Color c, TextAlignmentOptions al, bool autoSize = false, float min = 14f)
    {
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.font = font;
        t.text = text;
        t.fontSize = size;
        t.color = c;
        t.alignment = al;
        t.enableWordWrapping = true;
        t.richText = true;
        t.raycastTarget = false;
        if (autoSize) { t.enableAutoSizing = true; t.fontSizeMin = min; t.fontSizeMax = size; }
        return t;
    }

    //the tags of lab 9 ("Enemy" for the ghost, "Wall" for the walls) are added to the project if they are missing
    static void AddTag(string tag)
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tags = tagManager.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }

    //an image that keeps the proportions of the artwork inside its parent. Children placed with 0..1 anchors
    //stay on the same spot of the artwork at every screen size
    static RectTransform Fitted(RectTransform parent, string name, Sprite s)
    {
        var rt = UI(name, parent);
        Img(rt, s, Color.white);
        var f = rt.gameObject.AddComponent<AspectRatioFitter>();
        f.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        f.aspectRatio = s.rect.width / s.rect.height;
        return rt;
    }

    //a normal UI Button: the Color Tint of the Button makes the hover and pressed effects
    static Button MakeButton(RectTransform parent, string name, Sprite frame, string label, float size, UnityAction onClick)
    {
        var img = Img(parent, frame, Color.white, true);
        img.raycastTarget = true;
        var b = parent.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        ButtonColors(b);
        var t = Txt(Anchor(UI("Label", parent), 0.1f, 0.12f, 0.9f, 0.88f), label, size, Gold, TextAlignmentOptions.Center, true, 12f);
        t.fontStyle = FontStyles.Bold;
        t.characterSpacing = 6f;
        parent.name = name;
        UnityEventTools.AddPersistentListener(b.onClick, onClick);
        return b;
    }

    static void ButtonColors(Button b)
    {
        var colors = b.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.85f, 0.55f, 1f); //gold when the mouse is on it
        colors.pressedColor = new Color(0.65f, 0.45f, 0.35f, 1f); //darker when it is pressed
        colors.selectedColor = Color.white;
        colors.fadeDuration = 0.1f;
        b.colors = colors;
        b.transition = Selectable.Transition.ColorTint;
    }

    // ------------------------------------------------------------------ the scene

    const float FLOOR = -4f;        //top of the floor
    const float COLLAPSE_START = 15.8f;
    const float COLLAPSE_END = 22.8f;

    static void BuildScene()
    {
        int ground = LayerMask.NameToLayer("Ground");
        if (ground < 0) ground = 0;

        // ---------- sounds
        var sMusic = Clip(AUDIO + "/Final Room Music.mp3");
        var sSwitch = Clip(AUDIO + "/Electric Switch.mp3");
        var sGhost = Clip(AUDIO + "/Ghost.mp3");
        var sWhisper = Clip(AUDIO + "/Ghost Whisper.mp3");
        var sClick = Clip(AUDIO + "/Button Click.mp3");
        var sDamage = Clip(AUDIO + "/Damage.mp3");
        var sAlex = Clip(AUDIO + "/Alex.mp3");
        var sTimer = Clip(AUDIO + "/Timer.mp3");
        var sBullet = Clip(AUDIO + "/Bullet.mp3");
        var sCheckpoint = Clip(AUDIO + "/Checkpoint.mp3");
        var sDoor = Clip(AUDIO + "/Door Open.mp3");
        var sDevice = Clip(AUDIO + "/Device Activation.ogg");
        //sounds of level 2 that are reused
        var sBreak = Clip(L2 + "/Sounds/break.mp3");
        var sCorrect = Clip(L2 + "/Sounds/correct.mp3");
        var sWrong = Clip(L2 + "/Sounds/wrong_pass.mp3");
        var sDeath = Clip(L2 + "/Sounds/death.mp3");
        var sKeypad = Clip(L2 + "/Sounds/keypad.mp3");
        var sPaper = Clip(L2 + "/Sounds/paper.mp3");

        // ---------- sprites
        var bgSprite = LoadSprite(ART + "/Final_Room_Background.png");
        var platform = LoadSprite(ART + "/Final_Room_Platform.png");
        var candles = LoadSprite(ART + "/Final_Room_Candles.png");
        var deviceSprite = LoadSprite(ART + "/Sabrina_Device.png");
        var bulletSprite = LoadSprite(ART + "/Bullet.png");
        var switchFrames = LoadSprites(ART + "/Switch_Sheet.png", "Switch_");
        var doorFrames = LoadSprites(ART + "/Door_Sheet.png", "Door_");
        var spikeFrames = LoadSprites(ART + "/Spike_Trap_Sheet.png", "Spikes_");
        var collapseFrames = LoadSprites(ART + "/Collapsing_Floor_Sheet.png", "Collapse_");
        var ghostFrames = LoadSprites(ART + "/Ghost_Sheet.png", "Ghost_Float_");
        var symSun = LoadSprite(UIART + "/Symbol_Sun.png");
        var symMoon = LoadSprite(UIART + "/Symbol_Moon.png");
        var symTriangle = LoadSprite(UIART + "/Symbol_Triangle.png");
        var startArt = LoadSprite(UIART + "/Level5_Start_Screen.png");
        var messageArt = LoadSprite(UIART + "/Sabrina_Message_Screen.png");
        var endArt = LoadSprite(UIART + "/Level5_End_Screen.png");
        var deviceFrame = LoadSprite(UIART + "/Device_Puzzle_Frame.png");
        var timerFrame = LoadSprite(UIART + "/Device_Timer_Frame.png");
        var buttonFrame = LoadSprite(UIART + "/Gothic_Button_Frame.png");
        var hintFrame = LoadSprite(UIART + "/Hint_Frame.png"); //the gothic frame for the hints and "Press E"
        var promptFrame = hintFrame;
        var redBar = LoadSprite(L2 + "/Red Bar.png");
        var blackBar = LoadSprite(L2 + "/black bar.png");
        var respawnMark = LoadSprite(L2 + "/Respawnpoint.png");

        // ---------- managers and audio
        var managerGO = GO("Level 5 Manager", null, Vector3.zero);
        var manager = managerGO.AddComponent<Level5Manager>();
        manager.levelMusic = sMusic;
        manager.deathSound = sDeath;
        manager.checkpointSound = sCheckpoint;

        //an Audio Manager like level 2 (if alex comes from an earlier level, that one is kept and this one removes itself)
        var amGO = GO("Audio Manager", null, Vector3.zero);
        var am = amGO.AddComponent<AudioManager>();
        am.musicSource = amGO.AddComponent<AudioSource>();
        am.musicSource.loop = true; am.musicSource.playOnAwake = false; am.musicSource.volume = 0.55f;
        am.sfxSource = amGO.AddComponent<AudioSource>();
        am.sfxSource.playOnAwake = false;
        am.overworldMusic = sMusic;

        // ---------- room
        var room = GO("Final Room", null, Vector3.zero).transform;
        var bg = GO("Background", room, new Vector3(2.25f, 1.9f, 0));
        SR(bg, bgSprite, "Default", -100);

        for (int i = 0; i < 2; i++)
        {
            float x0 = i == 0 ? -23f : COLLAPSE_END;
            float x1 = i == 0 ? COLLAPSE_START : 29f;
            var fl = GO(i == 0 ? "Floor" : "Floor (Exit Side)", room, new Vector3((x0 + x1) / 2f, FLOOR + 0.12f, 0));
            var fsr = SR(fl, platform, "Default", -40);
            fsr.drawMode = SpriteDrawMode.Tiled;
            fsr.size = new Vector2(x1 - x0, platform.bounds.size.y);
            var col = GO(i == 0 ? "Ground" : "Ground (Exit Side)", room, new Vector3((x0 + x1) / 2f, FLOOR - 0.5f, 0));
            col.layer = ground;
            Box(col, new Vector2(x1 - x0, 1f), Vector2.zero, false);
        }
        var wallL = GO("Wall Left", room, new Vector3(-23.3f, 2f, 0)); Box(wallL, new Vector2(1, 20), Vector2.zero, false); wallL.tag = "Wall";
        var wallR = GO("Wall Right", room, new Vector3(28.3f, 2f, 0)); Box(wallR, new Vector2(1, 20), Vector2.zero, false); wallR.tag = "Wall";

        var c1 = GO("Candles", room, new Vector3(-11.5f, FLOOR + 0.05f, 0)); c1.transform.localScale = Vector3.one * 0.6f;
        var c1sr = SR(c1, candles, "Default", -30);
        var c2 = GO("Candles (1)", room, new Vector3(-4.2f, FLOOR + 0.05f, 0)); c2.transform.localScale = new Vector3(-0.55f, 0.55f, 1f);
        var c2sr = SR(c2, candles, "Default", -30);
        var embers1 = Particles("Candle Embers", c1.transform, new Vector3(0, 1.2f, 0), new Color(1f, 0.7f, 0.3f), new Color(1f, 0.4f, 0.1f), 6, 2.5f, 0.6f, 0.09f, ParticleSystemShapeType.Box, new Vector3(3, 0.5f, 0.1f), true, false, 0, "Default", -29, -0.05f);
        var embers2 = Particles("Candle Embers", c2.transform, new Vector3(0, 1.2f, 0), new Color(1f, 0.7f, 0.3f), new Color(1f, 0.4f, 0.1f), 6, 2.5f, 0.6f, 0.09f, ParticleSystemShapeType.Box, new Vector3(3, 0.5f, 0.1f), true, false, 0, "Default", -29, -0.05f);

        // darkness over the room (alex is on the foreground layer, so he stays visible)
        var dark = GO("Darkness", room, new Vector3(2.25f, 1.9f, 0));
        dark.transform.localScale = new Vector3(60f, 30f, 1f);
        var darkSR = SR(dark, white, "background", 0);
        darkSR.color = new Color(0, 0, 0, 0.78f);
        var flashGO = GO("Light Flash", room, new Vector3(2.25f, 1.9f, 0));
        flashGO.transform.localScale = new Vector3(60f, 30f, 1f);
        var flashSR = SR(flashGO, white, "foreground", 50);
        flashSR.color = new Color(1, 1, 1, 0);

        // ---------- level manager and checkpoint (same scripts as level 2)
        var lmGO = GO("Level Manager", null, Vector3.zero);
        var lm = lmGO.AddComponent<LevelManager>();
        lm.restartLevel = true;
        lm.levelName = "level 5";
        lm.clearLevel2Items = false;
        var cp = GO("Level 5 Checkpoint", room, new Vector3(-20.6f, FLOOR, 0));
        var cpMark = GO("Respawn Point", cp.transform, new Vector3(0, 0.75f, 0));
        if (respawnMark != null)
        {
            var mk = SR(cpMark, respawnMark, "Default", -25);
            float h = respawnMark.bounds.size.y;
            cpMark.transform.localScale = Vector3.one * (1.5f / Mathf.Max(0.01f, h));
            cpMark.transform.localPosition = new Vector3(-1.3f, 0.75f, 0);
        }
        var cpTrig = cp.AddComponent<CheckPoint>();
        Box(cp, new Vector2(2, 3), new Vector2(0, 1.5f), true);
        lm.CurrentCheckpoint = cp;
        var startPoint = GO("Start Point", cp.transform, new Vector3(0, 1.4f, 0));
        manager.startPoint = startPoint.transform;

        // ---------- camera
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        camGO.transform.position = new Vector3(-8.8f, 1.5f, -10f);
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 8f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        camGO.AddComponent<AudioListener>();
        var follow = camGO.AddComponent<CameraFollow>();
        follow.Cameraspeed = 6f;
        follow.minX = -8.6f; follow.maxX = 13.1f;
        follow.minY = 1.5f; follow.maxY = 1.5f;

        // ---------- UI canvases first (alex needs his health bar)
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();

        //the one Canvas of Level 5: the Health Bar and Inventory prefabs of level 2 and all the Level 5 screens
        var canvas = MakeCanvas("Canvas", 10, false); //Constant Pixel Size like the Canvas of level 2, so its prefabs look the same
        mainCanvas = canvas.transform;
        var healthPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/prefab/Health Bar.prefab");
        var invPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/prefab/Inventory UI.prefab");
        var health = (GameObject)PrefabUtility.InstantiatePrefab(healthPrefab, canvas.transform);
        var invGO = (GameObject)PrefabUtility.InstantiatePrefab(invPrefab, canvas.transform);
        var inventory = invGO.GetComponentInChildren<InventoryManager>(true);
        var redBarT = Find(health.transform, "red bar");

        // ---------- alex (the prefab of level 2)
        var alexPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/prefab/alex.prefab");
        var alex = (GameObject)PrefabUtility.InstantiatePrefab(alexPrefab);
        alex.name = "alex";
        alex.transform.position = startPoint.transform.position;
        alex.transform.localScale = new Vector3(0.91563797f, 0.6929629f, 1f); //the same size as in level 2
        var pc = alex.GetComponent<PlayerController>();
        pc.jumpHeight = 20f; //the same as in level 2
        var stats = alex.AddComponent<PlayerStats>();
        stats.healthBar = redBarT != null ? redBarT.GetComponent<Image>() : null;
        var steps = alex.AddComponent<AudioSource>();
        steps.clip = sAlex; steps.loop = true; steps.playOnAwake = false; steps.enabled = false; steps.volume = 0.6f;
        alex.AddComponent<PlayerFootsteps>();
        var shootPoint = GO("Shooting Point", alex.transform, new Vector3(0.75f, 0.35f, 0));
        var shooting = alex.AddComponent<PlayerShooting>();
        shooting.shootingPoint = shootPoint.transform;
        shooting.sr = alex.GetComponent<SpriteRenderer>();
        shooting.Shoot = KeyCode.F;
        shooting.shootSound = sBullet;
        shooting.reloadSound = sKeypad;
        follow.Target = alex.transform;
        manager.player = pc;
        manager.playerShooting = shooting;

        // ---------- bullet prefab
        var impact = Particles("Bullet Impact", null, Vector3.zero, new Color(0.6f, 0.9f, 1f), new Color(0.2f, 0.6f, 1f), 0, 0.35f, 4f, 0.18f, ParticleSystemShapeType.Circle, Vector3.one * 0.2f, false, true, 14, "foreground", 20);
        var impactPrefab = PrefabUtility.SaveAsPrefabAsset(impact.gameObject, PREFABS + "/Bullet Impact.prefab");
        Object.DestroyImmediate(impact.gameObject);

        var bulletGO = GO("Bullet", null, Vector3.zero);
        SR(bulletGO, bulletSprite, "foreground", 8);
        var brb = bulletGO.AddComponent<Rigidbody2D>();
        brb.gravityScale = 0; brb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        brb.interpolation = RigidbodyInterpolation2D.Interpolate;
        var bcol = bulletGO.AddComponent<CapsuleCollider2D>();
        bcol.direction = CapsuleDirection2D.Horizontal;
        bcol.size = new Vector2(bulletSprite.bounds.size.x * 0.9f, bulletSprite.bounds.size.y * 0.6f);
        bcol.isTrigger = true;
        var bullet = bulletGO.AddComponent<PlayerBullet>();
        bullet.impactEffect = impactPrefab;
        bullet.impactSound = sDamage;
        var bulletPrefab = PrefabUtility.SaveAsPrefabAsset(bulletGO, PREFABS + "/Bullet.prefab");
        Object.DestroyImmediate(bulletGO);
        shooting.bullet = bulletPrefab;

        // ---------- electrical switch
        var power = GO("Room Power", room, Vector3.zero).AddComponent<RoomPower>();
        power.darkness = darkSR;
        power.flash = flashSR;
        power.lightEffects = new[] { embers1, embers2 };

        var sw = GO("Electrical Switch", room, new Vector3(-15.5f, FLOOR + 0.9f, 0));
        var swSR = SR(sw, switchFrames[0], "background", 10); //above the darkness, so the player can find it
        Box(sw, new Vector2(3.2f, 3.4f), new Vector2(0, 0.6f), true);
        var swScript = sw.AddComponent<ElectricalSwitch>();
        swScript.sr = swSR;
        swScript.offSprite = switchFrames[0];
        swScript.onSprite = switchFrames[1];
        swScript.switchSound = sSwitch;
        swScript.room = power;
        swScript.sparks = Particles("Sparks", sw.transform, new Vector3(0, 2.3f, 0), new Color(1f, 0.95f, 0.6f), new Color(1f, 0.6f, 0.1f), 0, 0.5f, 5f, 0.08f, ParticleSystemShapeType.Circle, Vector3.one * 0.3f, false, false, 40, "foreground", 30, 1f);
        var swGlow = GO("Switch Glow", sw.transform, new Vector3(0, 1.3f, 0));
        swGlow.transform.localScale = Vector3.one * 3.5f;
        SR(swGlow, glow, "background", 9).color = new Color(1f, 0.6f, 0.3f, 0.35f);

        // ---------- Sabrina's ghost
        var ghostGO = GO("Sabrina's Ghost", null, new Vector3(-7f, -2.3f, 0));
        var gsr = SR(ghostGO, ghostFrames.Length > 0 ? ghostFrames[0] : null, "foreground", 12);
        var gAnim = ghostGO.AddComponent<Animator>();
        gAnim.runtimeAnimatorController = GhostAnimator();
        var gcol = ghostGO.AddComponent<CircleCollider2D>();
        gcol.radius = 1.05f; gcol.isTrigger = true;
        gcol.offset = Vector2.zero; //on her body (Unity moves a new collider to the middle of the picture)
        var ghost = ghostGO.AddComponent<GhostController>();
        ghost.anim = gAnim; ghost.sr = gsr; ghost.player = alex.transform; ghost.aimPoint = shootPoint.transform;
        ghost.maxSpeed = 2.4f; ghost.damage = 1;
        ghost.health = 10; ghost.maxHealth = 10; //one full clip of 10 bullets defeats her
        ghost.appearSound = sGhost; ghost.whisperSound = sWhisper;
        ghost.hitSound = sDamage; ghost.deathSound = sWhisper; ghost.hurtSound = sDamage;
        ghost.minX = -21.5f; ghost.maxX = -2f;
        ghost.appearEffect = Particles("Appear Mist", ghostGO.transform, Vector3.zero, new Color(1f, 0.2f, 0.35f), new Color(0.6f, 0.1f, 0.3f), 0, 1.2f, 2f, 0.6f, ParticleSystemShapeType.Circle, Vector3.one, false, false, 60, "foreground", 11);
        ghost.deathEffect = Particles("Fade Mist", ghostGO.transform, Vector3.zero, new Color(1f, 0.85f, 0.9f), new Color(1f, 0.3f, 0.45f), 0, 1.8f, 1.5f, 0.5f, ParticleSystemShapeType.Circle, Vector3.one * 1.5f, false, false, 90, "foreground", 13, -0.1f);
        manager.ghost = ghost;

        var barrier = GO("Spirit Barrier", room, new Vector3(-1f, 2f, 0));
        Box(barrier, new Vector2(0.6f, 20f), Vector2.zero, false);
        barrier.tag = "Wall";
        Particles("Barrier Mist", barrier.transform, new Vector3(0, -4.5f, 0), new Color(1f, 0.25f, 0.4f, 0.6f), new Color(0.5f, 0.1f, 0.3f, 0.5f), 18, 2.5f, 1.2f, 0.5f, ParticleSystemShapeType.Box, new Vector3(0.4f, 1f, 0.1f), true, true, 0, "foreground", 2);
        manager.ghostBarrier = barrier;

        // ---------- spike traps
        var traps = new List<SpikeTrap>();
        GameObject trapPrefab = null;
        float[] trapX = { 1.8f, 5.1f };
        for (int i = 0; i < 2; i++)
        {
            GameObject t;
            if (trapPrefab == null)
            {
                t = GO("Spike Trap", null, Vector3.zero);
                var tsr = SR(t, spikeFrames[1], "Default", -10);
                var dmg = Box(t, new Vector2(2.3f, 0.55f), new Vector2(0f, 0.78f), true);
                var trap = t.AddComponent<SpikeTrap>();
                trap.sr = tsr;
                trap.hiddenSprite = spikeFrames[1]; //the sheet shows the sharp spikes first
                trap.sharpSprite = spikeFrames[0];
                trap.spikeCollider = dmg;
                trap.spikeSound = sBreak;
                trapPrefab = PrefabUtility.SaveAsPrefabAssetAndConnect(t, PREFABS + "/Spike Trap.prefab", InteractionMode.AutomatedAction);
            }
            else
            {
                t = (GameObject)PrefabUtility.InstantiatePrefab(trapPrefab);
            }
            t.name = "Spike Trap " + (i + 1);
            t.transform.SetParent(room, false);
            t.transform.position = new Vector3(trapX[i], FLOOR - 0.05f, 0);
            var st = t.GetComponent<SpikeTrap>();
            st.startDelay = i * 1.1f;
            traps.Add(st);
        }
        manager.spikeTraps = traps.ToArray();

        // ---------- Sabrina's device
        var dev = GO("Sabrina's Device", room, new Vector3(10.6f, FLOOR - 0.05f, 0));
        var devSR = SR(dev, deviceSprite, "Default", -20);
        Box(dev, new Vector2(4f, 3.5f), new Vector2(0, 1.75f), true);
        var device = dev.AddComponent<SabrinaDevice>();
        device.activationSound = sDevice;
        var tick = dev.AddComponent<AudioSource>(); //the ticking of the 20 seconds, Loop checked
        tick.clip = sTimer; tick.loop = true; tick.playOnAwake = false;
        device.timerSound = tick;
        device.flash = flashSR;
        Vector3[] tilePos = { new Vector3(-0.976f, 1.461f, 0), new Vector3(0.003f, 1.461f, 0), new Vector3(0.995f, 1.461f, 0) };
        Color[] tileCol = { new Color(1f, 0.6f, 0.15f, 1f), new Color(0.3f, 0.55f, 1f, 1f), new Color(1f, 0.9f, 0.25f, 1f) };
        string[] tileName = { "Sun Light", "Moon Light", "Triangle Light" };
        var lights = new SpriteRenderer[3];
        for (int i = 0; i < 3; i++)
        {
            var l = GO(tileName[i], dev.transform, tilePos[i]);
            l.transform.localScale = Vector3.one * 1.9f;
            lights[i] = SR(l, glow, "Default", -19);
            lights[i].color = tileCol[i];
        }
        //the light tint stays in the colour, the alpha is changed by the script
        device.symbolLights = lights;
        var core = GO("Core Glow", dev.transform, new Vector3(-0.01f, 2.675f, 0));
        core.transform.localScale = Vector3.one * 3.2f;
        device.coreGlow = SR(core, glow, "Default", -19);
        device.coreGlow.color = new Color(1f, 0.25f, 0.3f, 1f);
        device.magicEffect = Particles("Magic", dev.transform, new Vector3(0, 2.4f, 0), new Color(1f, 0.4f, 0.5f), new Color(1f, 0.85f, 0.4f), 25, 2.5f, 1.4f, 0.14f, ParticleSystemShapeType.Circle, Vector3.one * 1.5f, true, false, 80, "Default", -18, -0.05f);

        // ---------- collapsing floor
        var cfGO = GO("Collapsing Floor", room, new Vector3((COLLAPSE_START + COLLAPSE_END) / 2f, FLOOR, 0));
        Box(cfGO, new Vector2((COLLAPSE_END - COLLAPSE_START) + 1.4f, 3.5f), new Vector2(-0.7f, 1.75f), true); //starts a little before the floor, so alex can't jump over it
        var cf = cfGO.AddComponent<CollapsingFloor>();
        float partW = (COLLAPSE_END - COLLAPSE_START) / 3f;
        var parts = new SpriteRenderer[3];
        for (int i = 0; i < 3; i++)
        {
            var p = GO("Floor Part " + (i + 1), cfGO.transform, new Vector3(-partW + i * partW, 0.12f, 0));
            float w = collapseFrames[0].bounds.size.x;
            p.transform.localScale = new Vector3(partW / w * 1.02f, partW / w * 1.02f, 1f);
            parts[i] = SR(p, collapseFrames[0], "Default", -39);
        }
        var cground = GO("Collapsing Ground", cfGO.transform, new Vector3(0, -0.5f, 0));
        cground.layer = ground;
        var cgCol = Box(cground, new Vector2(COLLAPSE_END - COLLAPSE_START, 1f), Vector2.zero, false);
        var safe = GO("Safe Point", room, new Vector3(COLLAPSE_END + 0.4f, FLOOR, 0));
        cf.parts = parts;
        cf.stages = collapseFrames;
        cf.floorCollider = cgCol;
        cf.startX = COLLAPSE_START;
        cf.safePoint = safe.transform;
        cf.player = pc;
        cf.crackSound = sBreak;
        cf.dust = Particles("Dust", cfGO.transform, new Vector3(0, -0.3f, 0), new Color(1f, 0.55f, 0.2f), new Color(0.5f, 0.3f, 0.2f), 30, 1.2f, 1f, 0.15f, ParticleSystemShapeType.Box, new Vector3(COLLAPSE_END - COLLAPSE_START, 0.2f, 0.1f), true, false, 0, "Default", -38, 0.6f);
        Particles("Fire", room, new Vector3((COLLAPSE_START + COLLAPSE_END) / 2f, -6.7f, 0), new Color(1f, 0.55f, 0.1f), new Color(1f, 0.15f, 0.05f), 45, 1.4f, 2.2f, 0.45f, ParticleSystemShapeType.Box, new Vector3(COLLAPSE_END - COLLAPSE_START, 0.3f, 0.1f), true, true, 0, "Default", -41);
        var pit = GO("Fire Pit", room, new Vector3((COLLAPSE_START + COLLAPSE_END) / 2f, -8.5f, 0));
        Box(pit, new Vector2(COLLAPSE_END - COLLAPSE_START + 2f, 2f), Vector2.zero, true);
        pit.AddComponent<Level5Hazard>().message = "The floor collapsed! Alex fell into the fire.";

        // ---------- exit door
        var doorGO = GO("Exit Door", room, new Vector3(25.4f, FLOOR - 0.05f, 0));
        SR(doorGO, doorFrames[0], "Default", -15);
        var dAnim = doorGO.AddComponent<Animator>();
        dAnim.runtimeAnimatorController = DoorAnimator(doorFrames);
        Box(doorGO, new Vector2(1f, 2.6f), new Vector2(0, 1.3f), true);
        var door = doorGO.AddComponent<Level5ExitDoor>();
        door.doorAnim = dAnim;
        door.openSound = sDoor;
        var dl = GO("Door Light", doorGO.transform, new Vector3(0, 1.45f, 0));
        dl.transform.localScale = new Vector3(3.2f, 4f, 1f);
        door.doorLight = SR(dl, glow, "Default", -14);
        door.doorLight.color = new Color(1f, 0.85f, 0.4f, 0f);
        door.lightEffect = Particles("Golden Light", doorGO.transform, new Vector3(0, 1.4f, 0), new Color(1f, 0.95f, 0.6f), new Color(1f, 0.75f, 0.3f), 20, 2f, 0.5f, 0.12f, ParticleSystemShapeType.Box, new Vector3(1.3f, 2.2f, 0.1f), true, false, 0, "Default", -13, -0.05f);
        var dw = GO("Doorway", doorGO.transform, new Vector3(0, 1f, 0));
        door.doorway = dw.transform;
        var doorBlock = GO("Door Block", room, new Vector3(24.2f, 2f, 0)); Box(doorBlock, new Vector2(0.5f, 20f), Vector2.zero, false); doorBlock.tag = "Wall";
        var floorBlock = GO("Floor Block", room, new Vector3(COLLAPSE_START - 1.6f, 2f, 0)); Box(floorBlock, new Vector2(0.5f, 20f), Vector2.zero, false); floorBlock.tag = "Wall";
        door.doorBlock = doorBlock;
        door.floorBlock = floorBlock;

        power.glowingThings = new[] { devSR, c1sr, c2sr, door.GetComponent<SpriteRenderer>() };

        // ---------- canvases of Level 5
        BuildUI(manager, inventory, invGO, symSun, symMoon, symTriangle, startArt, messageArt, endArt, deviceFrame, timerFrame,
                buttonFrame, hintFrame, promptFrame, redBar, blackBar, bulletSprite, device, door, cf, shooting, ghost,
                sClick, sCorrect, sWrong, sTimer, sGhost, sMusic, sPaper);

        //the ghost is saved as a prefab too. It is hidden until the power is back
        PrefabUtility.SaveAsPrefabAssetAndConnect(ghostGO, PREFABS + "/Sabrina's Ghost.prefab", InteractionMode.AutomatedAction);
        ghostGO.SetActive(false);
    }


    // ------------------------------------------------------------------ hint box

    //the message frame of Level 5 (gothic frame with the three symbols) with a close button at the top right
    static HintText BuildHintBox(Transform hud)
    {
        var frame = LoadSprite(UIART + "/Hint_Frame.png");
        var closeSprite = LoadSprite(UIART + "/Close_Button.png");
        var xSound = Clip(L2 + "/Sounds/X.mp3"); //the same close sound as the buttons of level 2

        var hintGO = UI("Hint Manager", hud);
        var hintText = hintGO.gameObject.AddComponent<HintText>();
        var hintBox = Anchor(UI("Hint", hintGO), 0.33f, 0.56f, 0.67f, 0.86f);
        Img(hintBox, frame, Color.white, true);
        hintText.hintBox = hintBox.gameObject;
        hintText.textDisplay = Txt(Anchor(UI("Hint Text", hintBox), 0.15f, 0.26f, 0.85f, 0.70f), "", 30, Ivory, TextAlignmentOptions.Center, true, 16);
        hintText.textDisplay.lineSpacing = 4;
        hintText.showTime = 6f;

        var close = Anchor(UI("Close Button", hintBox), 0.80f, 0.60f, 0.855f, 0.71f);
        var img = Img(close, closeSprite, Color.white, true);
        img.raycastTarget = true;
        var b = close.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        ButtonColors(b);
        var bs = close.gameObject.AddComponent<ButtonSound>();
        bs.clickSound = xSound;
        UnityEventTools.AddPersistentListener(b.onClick, bs.PlaySound);
        UnityEventTools.AddPersistentListener(b.onClick, hintText.CloseHint);
        return hintText;
    }

    //puts the new hint box in the Level 5 scene that is open now, without building the whole scene again
    [MenuItem("Tools/The Curse of Sabrina/Update Level 5 Hint Box")]
    public static void UpdateHintBoxMenu()
    {
        var manager = Object.FindObjectOfType<Level5Manager>();
        var hudGO = GameObject.Find("Level 5 HUD");
        if (manager == null || hudGO == null)
        {
            EditorUtility.DisplayDialog("Level 5", "Open the 'level 5' scene first.", "OK");
            return;
        }
        ImportAll();
        font = TMP_Settings.defaultFontAsset;
        white = LoadSprite(ART + "/White.png");
        glow = LoadSprite(ART + "/SoftGlow.png");
        var old = hudGO.transform.Find("Hint Manager");
        int index = old != null ? old.GetSiblingIndex() : -1;
        if (old != null) Object.DestroyImmediate(old.gameObject);
        manager.hint = BuildHintBox(hudGO.transform);
        if (index >= 0) manager.hint.transform.SetSiblingIndex(index);
        EditorUtility.SetDirty(manager);
        EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
        EditorSceneManager.SaveScene(manager.gameObject.scene);
        Debug.Log("Level 5: the hint box now uses the gothic frame with a close button.");
    }

    // ------------------------------------------------------------------ UI

    //the sentences are typed by the scripts letter by letter (lab 9), so they have no rich-text tags
    const string INTRO_TEXT =
        "\"Alex... you've finally made it.\n\n" +
        "This is where Sabrina's story ends... or perhaps where yours begins.\n\n" +
        "The room is silent, the lights are dead, and something is waiting in the darkness.\n\n" +
        "Explore your surroundings. Find a way to restore the power.\n\n" +
        "But be careful, Alex... you're not alone in here.";
    const string INTRO_LAST_LINE = "The truth is waiting for you.\"";

    const string MESSAGE_TEXT =
        "\"Alex… if you can hear this, then you have succeeded.\n\n" +
        "The three symbols have finally broken the curse. I spent years trying to stop what was hidden beneath this school.\n\n" +
        "I am sorry I couldn't escape. The truth is… I was never missing. I was trapped here, between life and death.\n\n" +
        "The ghost you saw was me.\n\n" +
        "Thank you for setting me free.\n\n" +
        "Now leave this place. Your story ends here… but mine can finally rest.\"";

    const string END_TEXT =
        "\"After everything he discovered, Alex finally understands what happened to Sabrina.\n\n" +
        "The three symbols have destroyed the mechanism sustaining the curse.\n\n" +
        "Sabrina is finally free, and the school is no longer haunted.\n\n" +
        "As the first light of dawn appears, Alex finally escapes.\"";

    static void BuildUI(Level5Manager manager, InventoryManager inventory, GameObject invGO, Sprite symSun, Sprite symMoon, Sprite symTriangle,
        Sprite startArt, Sprite messageArt, Sprite endArt, Sprite deviceFrame, Sprite timerFrame, Sprite buttonFrame, Sprite hintFrame,
        Sprite promptFrame, Sprite redBar, Sprite blackBar, Sprite bulletSprite, SabrinaDevice device, Level5ExitDoor door,
        CollapsingFloor floor, PlayerShooting shooting, GhostController ghost,
        AudioClip sClick, AudioClip sCorrect, AudioClip sWrong, AudioClip sTimer, AudioClip sGhost, AudioClip sMusic, AudioClip sPaper)
    {
        // ---------- inventory: the Triangle Symbol of level 2, plus the Crescent Moon and Sun Symbols
        manager.inventory = inventory;
        var tri = Find(invGO.transform, "Triangle Symbol");
        if (tri == null) { Debug.LogError("Level 5: 'Triangle Symbol' was not found in the Inventory UI prefab."); }
        else
        {
            var moon = Object.Instantiate(tri.gameObject, tri.parent);
            moon.name = "Crescent Moon Symbol";
            var sun = Object.Instantiate(tri.gameObject, tri.parent);
            sun.name = "Sun Symbol";
            var mi = moon.GetComponent<Image>(); if (mi != null) { mi.sprite = symMoon; mi.preserveAspect = true; }
            var si = sun.GetComponent<Image>(); if (si != null) { si.sprite = symSun; si.preserveAspect = true; }
            //the name under the icon
            foreach (var t in moon.GetComponentsInChildren<TextMeshProUGUI>(true)) t.text = "Crescent Moon Symbol";
            foreach (var t in sun.GetComponentsInChildren<TextMeshProUGUI>(true)) t.text = "Sun Symbol";

            //the names are centred under their own slot and wrap/shrink, so they never run into each other
            foreach (var icon in new[] { tri.gameObject, moon, sun })
            {
                foreach (var t in icon.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    var r = t.rectTransform;
                    r.anchorMin = new Vector2(0.5f, 0.5f);
                    r.anchorMax = new Vector2(0.5f, 0.5f);
                    r.pivot = new Vector2(0.5f, 1f);
                    r.anchoredPosition = new Vector2(0f, -42f);
                    r.sizeDelta = new Vector2(88f, 48f);
                    t.alignment = TextAlignmentOptions.Top;
                    t.enableWordWrapping = true;
                    t.enableAutoSizing = true;
                    t.fontSizeMin = 10f;
                    t.fontSizeMax = 18f;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(r);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(t);
                }
            }
            var icons = inventory.itemIcons.ToList();
            icons.Add(moon); icons.Add(sun);
            inventory.itemIcons = icons.ToArray();
            PrefabUtility.RecordPrefabInstancePropertyModifications(inventory);
            manager.triangleIcon = tri.gameObject;
            manager.moonIcon = moon;
            manager.sunIcon = sun;
        }

        // ---------- HUD
        //the order in the Canvas is the drawing order: HUD at the back, the fade at the front
        var hud = Section("Level 5 HUD");
        hud.SetAsFirstSibling(); //the Health Bar and Inventory of level 2 are drawn over the HUD

        //"Press E to ..." in the same gothic frame as the hints
        var prompt = Anchor(UI("Prompt", hud), 0.375f, 0.02f, 0.625f, 0.27f);
        Img(prompt, promptFrame, Color.white, true);
        manager.promptText = Txt(Anchor(UI("Prompt Text", prompt), 0.15f, 0.28f, 0.85f, 0.70f), "Press E", 30, Ivory, TextAlignmentOptions.Center, true, 14);
        manager.promptBox = prompt.gameObject;

        manager.hint = BuildHintBox(hud);

        //bullets left and reload (shown by PlayerShooting)
        var ammoPanel = Anchor(UI("Ammo", hud), 0.012f, 0.025f, 0.25f, 0.15f);
        Img(ammoPanel, white, new Color(0.05f, 0f, 0f, 0.55f));
        shooting.ammoText = Txt(Anchor(UI("Ammo Text", ammoPanel), 0.05f, 0.5f, 0.95f, 0.95f), "AMMO  10 / 10", 30, Gold, TextAlignmentOptions.Left, true, 14);
        shooting.ammoText.fontStyle = FontStyles.Bold;
        var row = Anchor(UI("Bullets", ammoPanel), 0.04f, 0.08f, 0.96f, 0.46f);
        var hl = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        hl.spacing = 4; hl.childControlWidth = true; hl.childControlHeight = true; hl.childForceExpandWidth = true; hl.childForceExpandHeight = true;
        var icons2 = new Image[10];
        for (int i = 0; i < 10; i++) icons2[i] = Img(UI("Bullet " + (i + 1), row), bulletSprite, Color.white, true);
        shooting.bulletIcons = icons2;
        var reload = Anchor(UI("Reload", ammoPanel), 0.04f, 0.06f, 0.96f, 0.48f);
        Img(reload, white, new Color(0, 0, 0, 0.85f));
        shooting.reloadBar = Filled(Anchor(UI("Reload Bar", reload), 0.02f, 0.1f, 0.98f, 0.9f), white, new Color(0.85f, 0.3f, 0.15f, 0.9f));
        shooting.reloadText = Txt(Anchor(UI("Reload Text", reload), 0f, 0f, 1f, 1f), "RELOADING", 24, Ivory, TextAlignmentOptions.Center, true, 12);
        shooting.reloadBox = reload.gameObject;

        //the new health bar of Sabrina's ghost (alex keeps his Health Bar prefab)
        var gbHolder = UI("Ghost Health Bar", hud);
        var gb = gbHolder.gameObject.AddComponent<GhostHealthBar>();
        var gbRoot = Anchor(UI("Bar", gbHolder), 0.3f, 0.855f, 0.7f, 0.98f);
        gb.bar = gbRoot.gameObject;
        var gLabel = Txt(Anchor(UI("Name", gbRoot), 0f, 0.66f, 1f, 1f), "SABRINA'S GHOST", 30, Gold, TextAlignmentOptions.Center, true, 14);
        gLabel.fontStyle = FontStyles.Bold; gLabel.characterSpacing = 8;
        var gBack = Anchor(UI("Back", gbRoot), 0f, -0.25f, 1f, 0.8f);
        Img(gBack, blackBar != null ? blackBar : white, blackBar != null ? Color.white : Color.black);
        gb.lightBar = Filled(Anchor(UI("Light Bar", gBack), 0f, 0f, 1f, 1f), redBar != null ? redBar : white, new Color(1f, 0.95f, 0.9f, 0.55f));
        gb.redBar = Filled(Anchor(UI("Red Bar", gBack), 0f, 0f, 1f, 1f), redBar != null ? redBar : white, redBar != null ? Color.white : Blood);
        ghost.healthBar = gb;

        //small device timer while the device screen is closed
        var hudTimer = Anchor(UI("Device Timer (HUD)", hud), 0.8f, 0.86f, 0.99f, 0.98f);
        Img(hudTimer, timerFrame, Color.white, true);
        var hudTimerText = Txt(Anchor(UI("Text", hudTimer), 0.22f, 0.2f, 0.78f, 0.8f), "DEVICE 20s", 26, Gold, TextAlignmentOptions.Center, true, 12);

        //countdown of the collapsing floor
        var cd = Anchor(UI("Floor Countdown", hud), 0.41f, 0.6f, 0.59f, 0.76f);
        Img(cd, timerFrame, Color.white, true);
        floor.countdownText = Txt(Anchor(UI("Text", cd), 0.25f, 0.15f, 0.75f, 0.85f), "7", 72, Gold, TextAlignmentOptions.Center, true, 20);
        floor.countdownText.fontStyle = FontStyles.Bold;
        floor.countdownBox = cd.gameObject;

        var controls = Anchor(UI("Controls", hud), 0.5f, 0.005f, 0.995f, 0.04f);
        Txt(controls, "← → move    SPACE jump    E interact    F shoot    H inventory", 20, new Color(1, 1, 1, 0.6f), TextAlignmentOptions.Right, true, 10);

        // ---------- device screen (SabrinaDevice opens and closes it)
        var devCanvas = Section("Sabrina's Device UI");
        var devRoot = UI("Device Screen", devCanvas);
        device.devicePanel = devRoot.gameObject;
        Img(UI("Dim", devRoot), white, new Color(0, 0, 0, 0.88f));
        var devArt = Fitted(devRoot, "Device Frame", deviceFrame);
        var dTitle = Txt(Anchor(UI("Title", devArt), 0.25f, 0.64f, 0.75f, 0.73f), "SABRINA'S DEVICE", 54, Gold, TextAlignmentOptions.Center, true, 20);
        dTitle.fontStyle = FontStyles.Bold; dTitle.characterSpacing = 10;

        var tf = Anchor(UI("Timer", devArt), 0.37f, 0.76f, 0.63f, 0.985f);
        Img(tf, timerFrame, Color.white, true);
        device.timerText = Txt(Anchor(UI("Seconds", tf), 0.3f, 0.28f, 0.7f, 0.78f), "20", 64, Gold, TextAlignmentOptions.Center, true, 20);
        device.timerText.fontStyle = FontStyles.Bold;
        var tBack = Anchor(UI("Time Bar", tf), 0.28f, 0.17f, 0.72f, 0.25f);
        Img(tBack, white, new Color(0, 0, 0, 0.7f));
        device.timerBar = Filled(Anchor(UI("Fill", tBack), 0.02f, 0.15f, 0.98f, 0.85f), white, Gold);

        Sprite[] syms = { symSun, symMoon, symTriangle };
        Color[] slotCol = { new Color(1f, 0.55f, 0.15f, 1f), new Color(0.3f, 0.5f, 1f, 1f), new Color(1f, 0.88f, 0.2f, 1f) };
        string[] slotNames = { "Sun Position", "Moon Position", "Triangle Position" };
        float[] sx = { 0.38f, 0.5f, 0.62f };
        device.slotGlows = new Image[3];
        device.slotSymbols = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            var slot = Anchor(UI(slotNames[i], devArt), sx[i] - 0.05f, 0.41f, sx[i] + 0.05f, 0.61f);
            device.slotGlows[i] = Img(Anchor(UI("Glow", slot), -0.45f, -0.45f, 1.45f, 1.45f), glow, slotCol[i]);
            Img(Anchor(UI("Socket", slot), 0.06f, 0.06f, 0.94f, 0.94f), white, new Color(0.07f, 0.02f, 0.02f, 0.92f));
            device.slotSymbols[i] = Img(Anchor(UI("Symbol", slot), 0f, 0f, 1f, 1f), syms[i], Color.white, true);
        }

        device.feedbackText = Txt(Anchor(UI("Feedback", devArt), 0.17f, 0.31f, 0.83f, 0.39f), "", 30, Ivory, TextAlignmentOptions.Center, true, 14);
        Txt(Anchor(UI("Your Symbols", devArt), 0.35f, 0.255f, 0.65f, 0.305f), "YOUR SYMBOLS", 24, Gold, TextAlignmentOptions.Center, true, 12).characterSpacing = 8;
        float[] cx = { 0.42f, 0.5f, 0.58f };
        string[] choiceNames = { "Sun Symbol", "Crescent Moon Symbol", "Triangle Symbol" };
        device.choices = new GameObject[3];
        device.choiceHighlights = new GameObject[3];
        for (int i = 0; i < 3; i++)
        {
            var c = Anchor(UI(choiceNames[i], devArt), cx[i] - 0.032f, 0.125f, cx[i] + 0.032f, 0.25f);
            var h = Anchor(UI("Highlight", c), -0.4f, -0.4f, 1.4f, 1.4f); //gold glow behind the chosen symbol
            Img(h, glow, new Color(1f, 0.85f, 0.4f, 0.9f));
            Img(Anchor(UI("Icon", c), 0f, 0f, 1f, 1f), syms[i], Color.white, true);
            device.choices[i] = c.gameObject;
            device.choiceHighlights[i] = h.gameObject;
        }
        Txt(Anchor(UI("Controls", devArt), 0.67f, 0.13f, 0.86f, 0.25f), "← →  choose\nE  place\nEsc  close", 24, new Color(1, 1, 1, 0.75f), TextAlignmentOptions.Left, true, 12);

        device.hudTimer = hudTimer.gameObject;
        device.hudTimerText = hudTimerText;
        device.insertSound = sCorrect;
        device.wrongSound = sWrong;
        device.timeUpSound = sWrong;

        // ---------- Sabrina's final message
        var msgCanvas = Section("Final Message UI");
        var fmGO = UI("Final Message", msgCanvas);
        var fm = fmGO.gameObject.AddComponent<SabrinaFinalMessage>();
        var msgRoot = UI("Sabrina's Final Message", msgCanvas);
        fm.messageScreen = msgRoot.gameObject;
        Img(UI("Black", msgRoot), white, Color.black);
        var msgArt = Fitted(msgRoot, "Message Frame", messageArt);
        fm.titleText = Txt(Anchor(UI("Title", msgArt), 0.2f, 0.675f, 0.8f, 0.75f), "SABRINA'S FINAL MESSAGE", 46, Gold, TextAlignmentOptions.Center, true, 18);
        fm.titleText.fontStyle = FontStyles.Bold; fm.titleText.characterSpacing = 10;
        fm.messageText = Txt(Anchor(UI("Message", msgArt), 0.17f, 0.34f, 0.83f, 0.665f), MESSAGE_TEXT, 30, Ivory, TextAlignmentOptions.Center, true, 14);
        fm.messageText.lineSpacing = 6;
        fm.messageText.fontStyle = FontStyles.Italic;
        fm.signatureText = Txt(Anchor(UI("Signature", msgArt), 0.5f, 0.295f, 0.82f, 0.345f), "— Sabrina", 36, Gold, TextAlignmentOptions.Right, true, 14);
        fm.signatureText.fontStyle = FontStyles.Italic | FontStyles.Bold;
        var cont = Anchor(UI("Continue", msgArt), 0.405f, 0.1f, 0.595f, 0.18f);
        MakeButton(cont, "Continue Button", buttonFrame, "Continue", 34, fm.Continue);
        fm.continueButton = cont.gameObject;
        fm.exitDoor = door;
        fm.messageMusic = sGhost;
        fm.levelMusic = sMusic;
        fm.continueSound = sClick;
        device.finalMessage = fm;

        // ---------- start screen
        var introCanvas = Section("Start Screen UI");
        var introGO = UI("Start Screen", introCanvas);
        var intro = introGO.gameObject.AddComponent<Level5Intro>();
        var introRoot = UI("Level 5 Start Screen", introCanvas);
        intro.startScreen = introRoot.gameObject;
        Img(UI("Black", introRoot), white, Color.black);
        var introArt = Fitted(introRoot, "Start Screen Art", startArt);
        intro.titleText = Txt(Anchor(UI("Title", introArt), 0.26f, 0.655f, 0.74f, 0.715f), "THE CURSE OF SABRINA", 42, Gold, TextAlignmentOptions.Center, true, 18);
        intro.titleText.fontStyle = FontStyles.Bold; intro.titleText.characterSpacing = 12;
        intro.dialogueText = Txt(Anchor(UI("Dialogue", introArt), 0.225f, 0.355f, 0.775f, 0.645f), INTRO_TEXT, 30, Ivory, TextAlignmentOptions.Center, true, 14);
        intro.dialogueText.lineSpacing = 4;
        intro.lastLineText = Txt(Anchor(UI("Last Line", introArt), 0.225f, 0.30f, 0.775f, 0.35f), INTRO_LAST_LINE, 34, Gold, TextAlignmentOptions.Center, true, 14);
        intro.lastLineText.fontStyle = FontStyles.Bold; //the last sentence stands out
        intro.buttonText = Txt(Anchor(UI("Button Text", introArt), 0.34f, 0.125f, 0.66f, 0.187f), "Press Any Key to Continue", 32, Gold, TextAlignmentOptions.Center, true, 14);
        intro.buttonText.characterSpacing = 4;
        intro.continueSound = sClick;
        manager.intro = intro;

        // ---------- end screen
        var endCanvas = Section("End Screen UI");
        var endGO = UI("End Screen Controller", endCanvas);
        var end = endGO.gameObject.AddComponent<Level5EndScreen>();
        var endRoot = UI("End Screen", endCanvas);
        end.endScreen = endRoot.gameObject;
        Img(UI("Black", endRoot), white, Color.black);
        var endArtRT = Fitted(endRoot, "End Screen Art", endArt);
        var buttons = UI("Buttons", endArtRT);
        end.buttons = buttons.gameObject;
        //the two buttons sit exactly on the button frames of the artwork
        MakeButton(Anchor(UI("Retry", buttons), 0.271f, 0.09f, 0.487f, 0.194f), "RETRY LEVEL Button", buttonFrame, "RETRY LEVEL", 30, end.RetryLevel);
        MakeButton(Anchor(UI("Menu", buttons), 0.512f, 0.09f, 0.727f, 0.194f), "MAIN MENU Button", buttonFrame, "MAIN MENU", 30, end.MainMenu);
        end.messageText = Txt(Anchor(UI("Message", buttons), 0.3f, 0.02f, 0.7f, 0.075f), "", 22, Ivory, TextAlignmentOptions.Center, true, 10);
        var story = UI("Story", endRoot);
        end.storyPanel = story.gameObject;
        Img(UI("Dark", story), white, new Color(0.02f, 0f, 0.01f, 0.95f));
        end.storyTitle = Txt(Anchor(UI("Title", story), 0.1f, 0.68f, 0.9f, 0.8f), "THE CURSE IS BROKEN", 72, Gold, TextAlignmentOptions.Center, true, 24);
        end.storyTitle.fontStyle = FontStyles.Bold; end.storyTitle.characterSpacing = 14;
        end.storyText = Txt(Anchor(UI("Story Text", story), 0.2f, 0.25f, 0.8f, 0.64f), END_TEXT, 36, Ivory, TextAlignmentOptions.Center, true, 16);
        end.storyText.lineSpacing = 8; end.storyText.fontStyle = FontStyles.Italic;
        end.endingMusic = sMusic;
        end.clickSound = sClick;
        end.mainMenuScene = FindMainMenuScene();
        manager.endScreen = end;

        // ---------- fader (on top of everything)
        var faderCanvas = Section("Fade");
        var fader = UI("Fader", faderCanvas);
        manager.fader = Img(fader, white, Color.black);
        manager.fader.raycastTarget = true;
        manager.faderText = Txt(Anchor(UI("Fader Text", fader), 0.15f, 0.35f, 0.85f, 0.65f), "", 48, new Color(1f, 0.45f, 0.35f), TextAlignmentOptions.Center, true, 16);
        manager.faderText.fontStyle = FontStyles.Bold;
    }

    //the Main Menu scene is not guessed: only a scene in the Build Settings whose name contains "menu" is used
    static string FindMainMenuScene()
    {
        foreach (var s in EditorBuildSettings.scenes)
        {
            if (string.IsNullOrEmpty(s.path)) continue;
            string n = Path.GetFileNameWithoutExtension(s.path);
            if (n.ToLower().Contains("menu")) return n;
        }
        Debug.LogWarning("Level 5: no Main Menu scene was found in the Build Settings. Type its name in 'End Screen Controller > Main Menu Scene' when it exists.");
        return "";
    }
}
#endif
