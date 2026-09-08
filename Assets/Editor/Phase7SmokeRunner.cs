using System;
using System.Reflection;
using ChickenRush;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class Phase7SmokeRunner
{
    const string Key = "ChickenRush.Tests.Phase7";
    private static int stage, frames;
    private static float pulseElapsed;
    private static double deadline;
    private static GameManager game;
    private static ChickenController normal, angry, happy;
    private static string PhysicsSnapshot(GameObject root)
    {
        string result = root.transform.localScale.ToString("F5");
        foreach (var c in root.GetComponentsInChildren<Collider2D>())
        {
            result += c.GetType().Name + c.offset + c.isTrigger + c.transform.localPosition + c.transform.localScale + AssetDatabase.GetAssetPath(c.sharedMaterial);
            if (c is CircleCollider2D circle) result += circle.radius;
            if (c is BoxCollider2D box) result += box.size + ":" + box.edgeRadius;
        }
        var body = root.GetComponent<Rigidbody2D>();
        if (body != null) result += body.mass + ":" + body.gravityScale + ":" + body.constraints + ":" + AssetDatabase.GetAssetPath(body.sharedMaterial);
        return result;
    }
    static Phase7SmokeRunner() { if (SessionState.GetBool(Key, false)) EditorApplication.update += Tick; }
    public static void ValidateDeliveredAssets()
    {
        try
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Chicken.prefab");
            var visual = prefab.GetComponent<ChickenVisual>();
            Check(visual != null && prefab.GetComponent<ChickenCostumeApplier>() != null, "Delivered prefab has missing scripts");
            Check(visual.BodyRenderer != null && visual.BodyRenderer.sprite == Resources.Load<Sprite>("Art/ChickenNormal"), "Delivered body reference missing");
            Check(visual.CostumeAnchor != null, "Delivered costume anchor missing");
            var hat = Resources.Load<ItemDataSO>("Items/costume.yellow_hat");
            Check(hat.costumeSprite == Resources.Load<Sprite>("Art/YellowHat"), "Delivered costume sprite missing");
            var nest = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Nest.prefab");
            Check(nest.transform.Find("Visual").GetComponent<SpriteRenderer>().sprite == Resources.Load<Sprite>("Art/Nest"), "Delivered nest sprite missing");
            Debug.Log("PHASE7_DELIVERED_ASSETS_PASS"); EditorApplication.Exit(0);
        }
        catch (Exception ex) { Debug.LogException(ex); EditorApplication.Exit(1); }
    }
    public static void VerifyMigration()
    {
        try
        {
            var chicken = PhysicsSnapshot(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Chicken.prefab"));
            var nest = PhysicsSnapshot(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Nest.prefab"));
            Phase7ArtBuilder.Build();
            Check(chicken == PhysicsSnapshot(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Chicken.prefab")), "Export changed chicken physics");
            Check(nest == PhysicsSnapshot(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Nest.prefab")), "Export changed nest physics");
            Debug.Log("PHASE7_ORIGINAL_PREFAB_MIGRATION_PASS"); EditorApplication.Exit(0);
        }
        catch (Exception ex) { Debug.LogException(ex); EditorApplication.Exit(1); }
    }
    public static void Run()
    {
        string beforeChicken = PhysicsSnapshot(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Chicken.prefab"));
        string beforeNest = PhysicsSnapshot(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Nest.prefab"));
        Phase7ArtBuilder.Build();
        Check(beforeChicken == PhysicsSnapshot(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Chicken.prefab")), "Chicken migration changed physics");
        Check(beforeNest == PhysicsSnapshot(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Nest.prefab")), "Nest migration changed physics");
        MvpSceneBuilder.Build();
        var inv = new SerializedObject(Object.FindFirstObjectByType<InventoryManager>());
        inv.FindProperty("playerPrefsKey").stringValue = Key; inv.ApplyModifiedPropertiesWithoutUndo();
        Object.FindFirstObjectByType<ChickenSpawner>().enabled = false;
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        PlayerPrefs.DeleteKey(Key); PlayerPrefs.DeleteKey(Key + ".backup");
        SessionState.SetBool(Key, true); EditorApplication.EnterPlaymode();
    }
    private static void Check(bool value, string message) { if (!value) throw new Exception(message); }
    private static ChickenController Spawn(Vector3 position)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<ChickenController>("Assets/Prefabs/Chicken.prefab");
        var c = Object.Instantiate(prefab, position, Quaternion.identity); c.Initialize(game, Vector2.zero); return c;
    }
    private static void Tick()
    {
        if (!EditorApplication.isPlaying) return;
        try
        {
            if (deadline == 0) deadline = EditorApplication.timeSinceStartup + 25;
            Check(EditorApplication.timeSinceStartup < deadline, "Phase7 timeout stage " + stage);
            if (stage == 0)
            {
                if (++frames < 5) return;
                game = Object.FindFirstObjectByType<GameManager>();
                Camera.main.aspect = .75f;
                Object.FindFirstObjectByType<GameDifficultyManager>().SelectDifficulty(GameDifficulty.Hell);
                normal = Spawn(new Vector3(-1, 0, 0));
                var visual = normal.GetComponent<ChickenVisual>();
                Check(visual.State == ChickenVisual.VisualState.Normal && visual.BodyRenderer.sprite == Resources.Load<Sprite>("Art/ChickenNormal"), "Normal sprite");
                Check(!visual.CostumeAnchor.enabled, "Unowned costume shown");
                var hat = Resources.Load<ItemDataSO>("Items/costume.yellow_hat");
                Check(game.Inventory.AddCoins(100) && game.Inventory.BuyItem(hat) && game.Inventory.EquipItem(hat), "Costume transaction");
                Check(visual.CostumeAnchor.sprite == hat.costumeSprite && visual.CostumeAnchor.enabled, "Live equipment overlay");
                Check(visual.CostumeAnchor.sortingOrder > visual.BodyRenderer.sortingOrder, "Costume sorting");
                var applier = normal.GetComponent<ChickenCostumeApplier>();
                applier.Initialize(null); Check(!visual.CostumeAnchor.enabled, "Missing inventory leaves stale overlay");
                applier.Initialize(game.Inventory); Check(visual.CostumeAnchor.enabled, "Rebind overlay");
                var originalSprite = hat.costumeSprite; hat.costumeSprite = null;
                game.Inventory.AddCoins(1); Check(!visual.CostumeAnchor.enabled, "Missing sprite not hidden");
                hat.costumeSprite = originalSprite; game.Inventory.AddCoins(1);
                angry = Spawn(Vector3.zero); happy = Spawn(new Vector3(1, 0, 0));
                Check(angry.GetComponent<ChickenVisual>().CostumeAnchor.sprite == hat.costumeSprite, "Spawn costume");
                int angerEvents = 0; var anger = angry.GetComponent<ChickenAnger>(); anger.OnBecameAngry += () => angerEvents++;
                var material = angry.GetComponent<Rigidbody2D>().sharedMaterial;
                float radius = angry.GetComponent<CircleCollider2D>().radius;
                var method = typeof(ChickenAnger).GetMethod("BecomeAngry", BindingFlags.Instance | BindingFlags.NonPublic);
                method.Invoke(anger, null); method.Invoke(anger, null);
                Check(angerEvents == 1 && angry.GetComponent<ChickenVisual>().State == ChickenVisual.VisualState.Angry, "Anger event/state");
                Check(angry.GetComponent<ChickenVisual>().BodyRenderer.sprite == Resources.Load<Sprite>("Art/ChickenAngry"), "Angry sprite");
                Check(angry.GetComponent<CircleCollider2D>().radius == radius && Mathf.Approximately(angry.transform.localScale.x, .32f * 1.2f), "Extra physical inflation");
                Check(Mathf.Approximately(material.bounciness, .4f) && Mathf.Approximately(angry.GetComponent<Rigidbody2D>().sharedMaterial.bounciness, .8f), "Shared physics material mutated");
                happy.GetComponent<ChickenVisual>().Celebrate();
                foreach (var c in new[] { normal, angry, happy }) c.GetComponent<Rigidbody2D>().simulated = false;
                var rotor = Object.FindFirstObjectByType<WindmillVisual>();
                Check(rotor != null && rotor.GetComponent<SpriteRenderer>().sprite == Resources.Load<Sprite>("Art/Windmill"), "Windmill art");
                var obstacle = rotor.transform.parent.GetComponent<CircleCollider2D>();
                Check(Mathf.Approximately(obstacle.radius, 31f / 64f) && Mathf.Approximately(obstacle.sharedMaterial.bounciness, .4f) && obstacle.transform.localScale == Vector3.one * 1.1f, "Obstacle physics changed");
                Check(game.ActiveNest.transform.Find("Visual").GetComponent<SpriteRenderer>().sprite == Resources.Load<Sprite>("Art/Nest"), "Nest art");
                stage = 1; frames = 0; return;
            }
            if (stage == 1)
            {
                pulseElapsed += Time.deltaTime;
                if (pulseElapsed < .35f) return;
                Check(Vector3.Distance(angry.GetComponent<ChickenVisual>().VisualRoot.localScale, Vector3.one) < .001f, "Visual pulse drift");
                CaptureScene();
                Capture();
                foreach (var c in new[] { normal, angry, happy }) Object.Destroy(c.gameObject);
                for (int i = 0; i < game.ActiveNest.Capacity; i++) Check(game.ActiveNest.TryAcceptChicken(Spawn(game.ActiveNest.transform.position)), "Fill failed");
                foreach (var v in game.ActiveNest.GetComponentsInChildren<ChickenVisual>())
                    Check(v.State == ChickenVisual.VisualState.Happy && v.BodyRenderer.sprite == Resources.Load<Sprite>("Art/ChickenHappy"), "Whole nest did not celebrate");
                stage = 2; return;
            }
            if (stage == 2)
            {
                if (game.IsChangingNest) return;
                Check(game.Score == 100, "Score changed");
                Debug.Log("PHASE7_SMOKE_PASS: migration physics, normal/angry/happy, costume spawn/live/missing data, anger event, pulse, nest/rotor art, full nest celebration, score");
                SessionState.SetBool(Key, false); EditorApplication.Exit(0);
            }
        }
        catch (Exception ex) { Debug.LogException(ex); SessionState.SetBool(Key, false); EditorApplication.Exit(1); }
    }
    private static void CaptureScene()
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
        var camera = Camera.main; var target = new RenderTexture(720, 960, 24);
        camera.targetTexture = target; camera.Render();
        var previous = RenderTexture.active; RenderTexture.active = target;
        var image = new Texture2D(720, 960, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, 720, 960), 0, 0); image.Apply();
        System.IO.File.WriteAllBytes("Phase7Scene.png", image.EncodeToPNG());
        RenderTexture.active = previous; camera.targetTexture = null;
        Object.Destroy(image); Object.Destroy(target);
    }
    private static void Capture()
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
        var camera = Camera.main; var size = camera.orthographicSize; var position = camera.transform.position;
        foreach (var c in new[] { normal, angry, happy }) c.transform.localScale = Vector3.one * .8f;
        camera.orthographicSize = 1.05f; camera.transform.position = new Vector3(0, .12f, -10);
        var target = new RenderTexture(1200, 630, 24); camera.targetTexture = target; camera.Render();
        var previous = RenderTexture.active; RenderTexture.active = target;
        var image = new Texture2D(1200, 630, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, 1200, 630), 0, 0); image.Apply();
        System.IO.File.WriteAllBytes("Phase7Characters.png", image.EncodeToPNG());
        RenderTexture.active = previous; camera.targetTexture = null; camera.orthographicSize = size; camera.transform.position = position;
        Object.Destroy(image); Object.Destroy(target);
    }
}
