#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Автосборка сцены: меню Tools → Catch The Dot → Build Scene.
/// Создаёт спрайты, камеру, Dot, Basket, Canvas, тексты, панель Game Over,
/// связывает все поля и добавляет сцену в Build Settings.
/// </summary>
public static class CatchTheDotSetup
{
    private const string SpritesDir = "Assets/Sprites";
    private const string ScenesDir = "Assets/Scenes";

    [MenuItem("Tools/Catch The Dot/Build Scene")]
    public static void BuildScene()
    {
        // TMP Essentials должны быть импортированы, иначе тексты не создадутся
        if (TMP_Settings.instance == null)
        {
            EditorUtility.DisplayDialog("Catch The Dot",
                "Сначала импортируйте TMP Essentials:\nWindow → TextMeshPro → Import TMP Essential Resources.\nПотом снова запустите Tools → Catch The Dot → Build Scene.",
                "OK");
            return;
        }

        EnsureFolder("Assets", "Sprites");
        EnsureFolder("Assets", "Scenes");

        Sprite circle = MakeSprite("Circle.png", 128, true);
        Sprite square = MakeSprite("Square.png", 200, false);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Камера (ортографическая, под портретный экран 9:16)
        var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 9.6f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.10f, 0.12f, 0.18f);
        camGo.transform.position = new Vector3(0, 0, -10);
        camGo.AddComponent<AudioListener>();

        // Dot
        var dotGo = new GameObject("Dot");
        var dotSr = dotGo.AddComponent<SpriteRenderer>();
        dotSr.sprite = circle;
        dotSr.color = new Color(1f, 0.35f, 0.35f);
        var dot = dotGo.AddComponent<Dot>();

        // Basket
        var basketGo = new GameObject("Basket");
        var basketSr = basketGo.AddComponent<SpriteRenderer>();
        basketSr.sprite = square;
        basketSr.color = new Color(0.35f, 0.7f, 1f);
        basketSr.sortingOrder = 1;
        basketGo.transform.position = new Vector3(0, -5, 0);
        var basket = basketGo.AddComponent<Basket>();

        // Canvas
        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        // EventSystem (нужен для кнопки)
        var esGo = new GameObject("EventSystem");
        esGo.AddComponent<EventSystem>();
        esGo.AddComponent<StandaloneInputModule>();

        // Тексты счёта и таймера
        var scoreText = MakeText("ScoreText", canvasGo.transform, "Score: 0", 56,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(-40, -40),
            new Vector2(500, 100), TextAlignmentOptions.TopRight);
        var timerText = MakeText("TimerText", canvasGo.transform, "Time: 30", 56,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -40),
            new Vector2(500, 100), TextAlignmentOptions.TopLeft);

        // Панель Game Over
        var panelGo = new GameObject("GameOverPanel", typeof(RectTransform));
        panelGo.transform.SetParent(canvasGo.transform, false);
        var panelRt = panelGo.GetComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;
        var panelImg = panelGo.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.7f);

        var goText = MakeText("GameOverText", panelGo.transform, "Game Over", 80,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 200),
            new Vector2(900, 300), TextAlignmentOptions.Center);

        // Кнопка «Играть снова»
        var btnGo = new GameObject("RestartButton", typeof(RectTransform));
        btnGo.transform.SetParent(panelGo.transform, false);
        var btnRt = btnGo.GetComponent<RectTransform>();
        btnRt.anchorMin = btnRt.anchorMax = btnRt.pivot = new Vector2(0.5f, 0.5f);
        btnRt.anchoredPosition = new Vector2(0, -150);
        btnRt.sizeDelta = new Vector2(500, 150);
        var btnImg = btnGo.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.7f, 0.4f);
        var button = btnGo.AddComponent<Button>();
        button.targetGraphic = btnImg;
        var btnLabel = MakeText("Text (TMP)", btnGo.transform, "Play again", 56,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
            new Vector2(500, 150), TextAlignmentOptions.Center);

        // GameManager
        var gmGo = new GameObject("GameManager");
        var gm = gmGo.AddComponent<GameManager>();

        // Связываем поля через SerializedObject
        var gmSo = new SerializedObject(gm);
        gmSo.FindProperty("scoreText").objectReferenceValue = scoreText;
        gmSo.FindProperty("timerText").objectReferenceValue = timerText;
        gmSo.FindProperty("gameOverText").objectReferenceValue = goText;
        gmSo.FindProperty("gameOverPanel").objectReferenceValue = panelGo;
        gmSo.ApplyModifiedPropertiesWithoutUndo();

        var basketSo = new SerializedObject(basket);
        basketSo.FindProperty("dot").objectReferenceValue = dot;
        basketSo.ApplyModifiedPropertiesWithoutUndo();

        // OnClick → GameManager.RestartGame()
        UnityEventTools.AddPersistentListener(button.onClick, gm.RestartGame);

        // Панель выключена до Game Over
        panelGo.SetActive(false);

        // Сохраняем сцену и добавляем в Build Settings
        string scenePath = ScenesDir + "/Game.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };

        Debug.Log("Catch The Dot: сцена собрана → " + scenePath + ". Нажмите Play.");
    }

    [MenuItem("Tools/Catch The Dot/Configure Android Settings")]
    public static void ConfigureAndroid()
    {
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.student.catchthedot");
        PlayerSettings.productName = "Catch The Dot";
        Debug.Log("Android: портретная ориентация, package com.student.catchthedot. " +
                  "Поменяйте 'student' на свою фамилию латиницей.");
    }

    // ---------- вспомогательные методы ----------

    private static void EnsureFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + name))
            AssetDatabase.CreateFolder(parent, name);
    }

    private static Sprite MakeSprite(string fileName, int size, bool round)
    {
        string path = SpritesDir + "/" + fileName;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float r = size / 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float a = 1f;
                if (round)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                    a = Mathf.Clamp01(r - d); // сглаженный край
                }
                tex.SetPixel(x, y, new Color(1, 1, 1, a));
            }
        }
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path);

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 100;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static TextMeshProUGUI MakeText(string name, Transform parent, string text, float fontSize,
        Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size, TextAlignmentOptions align)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = align;
        tmp.color = Color.white;
        return tmp;
    }
}
#endif
