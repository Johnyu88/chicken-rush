using System;
using System.Reflection;
using ChickenRush;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class Phase6SmokeRunner
{
    private const string Key = "ChickenRush.Tests.Phase6";
    private static int frames, stage;
    private static double deadline;
    private static GameManager game;
    private static ChickenController chicken;
    private static GameObject floor;
    private static CameraShake shake;
    private static Vector3 cameraOrigin;
    private static bool sawImpactShake;
    static Phase6SmokeRunner() { if (SessionState.GetBool(Key, false)) EditorApplication.update += Tick; }
    public static void Run()
    {
        MvpSceneBuilder.Build();
        Object.FindFirstObjectByType<ChickenSpawner>().enabled = false;
        var inventory = new SerializedObject(Object.FindFirstObjectByType<InventoryManager>());
        inventory.FindProperty("playerPrefsKey").stringValue = Key;
        inventory.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        PlayerPrefs.DeleteKey(Key); PlayerPrefs.DeleteKey(Key + ".backup");
        SessionState.SetBool(Key, true); EditorApplication.EnterPlaymode();
    }
    private static void Check(bool value, string label) { if (!value) throw new Exception(label); }
    private static ChickenController Spawn(Vector3 position)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<ChickenController>("Assets/Prefabs/Chicken.prefab");
        var value = Object.Instantiate(prefab, position, Quaternion.identity);
        value.Initialize(game, Vector2.zero); return value;
    }
    private static void CaptureEffects(ChickenController target)
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
        var aura = target.GetComponentInChildren<ParticleSystem>();
        aura.Simulate(.3f, true, false); aura.Play();
        var burst = EffectManager.Instance.PlayFeathers(target.transform.position + Vector3.right * .6f);
        burst.Simulate(.15f, true, false); burst.Play();
        var camera = Camera.main;
        var oldSize = camera.orthographicSize; var oldPosition = camera.transform.position;
        var rt = new RenderTexture(640, 640, 24); camera.targetTexture = rt;
        camera.orthographicSize = 1.3f;
        camera.transform.position = target.transform.position + new Vector3(.25f, 0, -10);
        camera.Render();
        var prior = RenderTexture.active; RenderTexture.active = rt;
        var image = new Texture2D(640, 640, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, 640, 640), 0, 0); image.Apply();
        System.IO.File.WriteAllBytes("Phase6Effects.png", image.EncodeToPNG());
        RenderTexture.active = prior; camera.targetTexture = null;
        camera.orthographicSize = oldSize; camera.transform.position = oldPosition;
        Object.Destroy(image); Object.Destroy(rt);
    }
    private static void Tick()
    {
        if (!EditorApplication.isPlaying) return;
        try
        {
            if (deadline == 0) deadline = EditorApplication.timeSinceStartup + 35;
            Check(EditorApplication.timeSinceStartup < deadline, "Phase6 timed out in stage " + stage);
            if (stage == 0)
            {
                if (++frames < 5) return;
                game = Object.FindFirstObjectByType<GameManager>();
                Object.FindFirstObjectByType<GameDifficultyManager>().SelectDifficulty(GameDifficulty.Relaxed);
                Check(game.IsPlaying, "Start game");
                Check(AudioManager.Instance != null && EffectManager.Instance != null, "Managers not bootstrapped");
                var audio = AudioManager.Instance;
                Check(Object.FindObjectsByType<AudioManager>(FindObjectsSortMode.None).Length == 1, "Duplicate mixer");
                var a = audio.PlayCombo(0); var b = audio.PlayCombo(1); var c = audio.PlayCombo(2);
                Check(a != b && b != c && Mathf.Approximately(a.pitch, 1) && Mathf.Approximately(b.pitch, 1.05f) && Mathf.Approximately(c.pitch, 1.1f), "Independent combo pitch");
                Check(AudioManager.ComboPitch(-1) == 1 && AudioManager.ComboPitch(int.MaxValue) == 2, "Pitch bounds");
                Check(audio.MusicSource.loop && audio.MusicSource.clip != null && audio.MusicSource.pitch == 1, "BGM not independent");
                shake = Camera.main.GetComponent<CameraShake>(); cameraOrigin = shake.transform.localPosition;
                floor = new GameObject("Impact Test Floor", typeof(BoxCollider2D));
                floor.transform.position = Vector3.zero; floor.GetComponent<BoxCollider2D>().size = new Vector2(4, .25f);
                chicken = Spawn(new Vector3(0, 1, 0)); chicken.GetComponent<Rigidbody2D>().linearVelocity = Vector2.down * 10;
                stage = 1; return;
            }
            if (stage == 1)
            {
                sawImpactShake |= shake.IsShaking;
                var anger = chicken.GetComponent<ChickenAnger>();
                if (!anger.isAngry) return;
                Check(sawImpactShake, "Strong physical collision did not shake");
                Check(chicken.GetComponentsInChildren<ParticleSystem>().Length == 1, "Missing anger aura");
                if (chicken.GetComponentInChildren<ParticleSystem>().particleCount == 0) return;
                var aura = chicken.GetComponentInChildren<ParticleSystem>();
                typeof(ChickenAnger).GetMethod("BecomeAngry", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(anger, null);
                Check(chicken.GetComponentsInChildren<ParticleSystem>().Length == 1, "Repeated anger duplicated aura");
                Check(aura.transform.parent == chicken.transform && aura.main.loop, "Aura must follow chicken");
                CaptureEffects(chicken);
                int before = EffectManager.Instance.GetComponentsInChildren<ParticleSystem>().Length;
                Check(game.ActiveNest.TryAcceptChicken(chicken), "Nest rejected chicken");
                Check(!aura.gameObject.activeSelf, "Nested chicken kept aura");
                Check(EffectManager.Instance.GetComponentsInChildren<ParticleSystem>().Length == before + 1, "Missing feather burst");
                Check(!game.ActiveNest.TryAcceptChicken(chicken), "Duplicate accepted");
                Check(EffectManager.Instance.GetComponentsInChildren<ParticleSystem>().Length == before + 1, "Duplicate feather event");
                Object.Destroy(floor);
                shake.StopShake();
                for (int i = game.ActiveNest.CurrentCount; i < game.ActiveNest.Capacity; i++)
                    Check(game.ActiveNest.TryAcceptChicken(Spawn(game.ActiveNest.transform.position)), "Fill failed");
                Check(game.Combo == 1 && shake.IsShaking, "Full nest juice or combo missing");
                int fullBursts = EffectManager.Instance.GetComponentsInChildren<ParticleSystem>().Length;
                game.BeginNestCompletion(game.ActiveNest);
                Check(EffectManager.Instance.GetComponentsInChildren<ParticleSystem>().Length == fullBursts, "Full nest effect duplicated");
                stage = 2; return;
            }
            if (stage == 2)
            {
                if (game.IsChangingNest || shake.IsShaking) return;
                Check(game.Score == 100, "Juice changed scoring");
                Check(Vector3.Distance(cameraOrigin, shake.transform.localPosition) < .0001f, "Shake drift");
                shake.Shake(.3f, .2f); shake.Shake(.1f, .1f);
                stage = 3; return;
            }
            if (stage == 3)
            {
                shake.enabled = false;
                Check(Vector3.Distance(cameraOrigin, shake.transform.localPosition) < .0001f, "Disable failed to restore camera");
                shake.enabled = true;
                var lost = Spawn(new Vector3(4, 2, 0));
                game.ReportChickenLost(lost); game.RequestRescue();
                stage = 4; return;
            }
            if (stage == 4)
            {
                if (game.Rescue != GameManager.RescueState.Used) return;
                Check(game.IsPlaying && game.Combo == 1 && shake.IsShaking, "Rescue feedback/state");
                stage = 5; return;
            }
            if (stage == 5)
            {
                if (shake.IsShaking || EffectManager.Instance.GetComponentsInChildren<ParticleSystem>().Length != 0) return;
                Check(Vector3.Distance(cameraOrigin, shake.transform.localPosition) < .0001f, "Rescue shake drift");
                Check(Object.FindObjectsByType<ChickenController>(FindObjectsSortMode.None).Length == 0, "Rescue left chicken/aura");
                game.RestartGame(); stage = 6; frames = 0; return;
            }
            if (stage == 6)
            {
                if (++frames < 5) return;
                Check(Object.FindObjectsByType<AudioManager>(FindObjectsSortMode.None).Length == 1 && AudioManager.Instance.MusicSource.clip != null, "Reload duplicated/lost audio manager");
                Check(Object.FindObjectsByType<EffectManager>(FindObjectsSortMode.None).Length == 1 && Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None).Length == 0, "Reload left particle systems");
                Debug.Log("PHASE6_SMOKE_PASS: audio pitch/BGM, physical impact, anger lifecycle, feathers, full nest, rescue, cleanup, camera restoration");
                SessionState.SetBool(Key, false); EditorApplication.Exit(0);
            }
        }
        catch (Exception ex) { Debug.LogException(ex); SessionState.SetBool(Key, false); EditorApplication.Exit(1); }
    }
}


