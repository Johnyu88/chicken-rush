using System.IO;
using ChickenRush;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>一次建立可重現的 MVP 測試場景；只在 Editor 執行。</summary>
public static class MvpSceneBuilder
{
    [MenuItem("Chicken Rush/Build MVP Test Scene")]
    public static void BuildFromMenu()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Build();
    }

    public static void Build()
    {
        Directory.CreateDirectory("Assets/Scenes");
        Directory.CreateDirectory("Assets/Prefabs");
        Directory.CreateDirectory("Assets/TestArt");
        Directory.CreateDirectory("Assets/Physics");
        const string materialPath = "Assets/Physics/ChickenPhysicsMaterial.physicsMaterial2D";
        var chickenMaterial = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(materialPath);
        if (chickenMaterial == null)
        {
            chickenMaterial = new PhysicsMaterial2D("ChickenPhysicsMaterial");
            AssetDatabase.CreateAsset(chickenMaterial, materialPath);
        }
        chickenMaterial.bounciness = 0.4f;
        chickenMaterial.friction = 0.2f;
        EditorUtility.SetDirty(chickenMaterial);
        var texture = new Texture2D(32, 32);
        var pixels = new Color[1024];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        texture.SetPixels(pixels); texture.Apply();
        File.WriteAllBytes("Assets/TestArt/Square.png", texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset("Assets/TestArt/Square.png");
        var importer = (TextureImporter)AssetImporter.GetAtPath("Assets/TestArt/Square.png");
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 32;
        importer.SaveAndReimport();
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/TestArt/Square.png");
        BuildExampleItems(sprite);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0, 0, -10);
        var camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true; camera.orthographicSize = 5;
        camera.backgroundColor = new Color(0.08f, 0.14f, 0.2f);
        camera.clearFlags = CameraClearFlags.SolidColor;

        var chicken = new GameObject("Chicken", typeof(ChickenController), typeof(SpriteRenderer));
        chicken.GetComponent<SpriteRenderer>().sprite = sprite;
        chicken.GetComponent<SpriteRenderer>().color = new Color(1, 0.8f, 0.15f);
        chicken.transform.localScale = Vector3.one * 0.32f;
        chicken.GetComponent<CircleCollider2D>().radius = 0.5f;
        chicken.GetComponent<Rigidbody2D>().collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        chicken.GetComponent<Rigidbody2D>().sharedMaterial = chickenMaterial;
        chicken.GetComponent<CircleCollider2D>().sharedMaterial = chickenMaterial;
        chicken.AddComponent<ChickenAnger>();
        var chickenPrefab = PrefabUtility.SaveAsPrefabAsset(chicken, "Assets/Prefabs/Chicken.prefab").GetComponent<ChickenController>();
        Object.DestroyImmediate(chicken);

        var nest = new GameObject("Nest", typeof(NestController));
        var visual = new GameObject("Visual", typeof(SpriteRenderer));
        visual.transform.SetParent(nest.transform);
        visual.transform.localScale = new Vector3(7, 0.45f, 1);
        visual.GetComponent<SpriteRenderer>().sprite = sprite;
        visual.GetComponent<SpriteRenderer>().color = new Color(0.45f, 0.8f, 0.55f);
        var sensor = new GameObject("InteriorSensor", typeof(NestSensor));
        sensor.transform.SetParent(nest.transform);
        sensor.transform.localPosition = new Vector3(0, 0.4f, 0);
        sensor.GetComponent<BoxCollider2D>().size = new Vector2(7, 0.6f);
        sensor.GetComponent<BoxCollider2D>().isTrigger = true;
        Set(sensor.GetComponent<NestSensor>(), "nest", nest.GetComponent<NestController>());
        var nestPrefab = PrefabUtility.SaveAsPrefabAsset(nest, "Assets/Prefabs/Nest.prefab").GetComponent<NestController>();
        Object.DestroyImmediate(nest);

        var spawnPoint = new GameObject("NestSpawnPoint");
        spawnPoint.transform.position = new Vector3(0, -3.5f, 0);
        var manager = new GameObject("GameManager", typeof(GameManager), typeof(AudioSource)).GetComponent<GameManager>();
        Set(manager, "nestPrefab", nestPrefab); Set(manager, "nestSpawnPoint", spawnPoint.transform);
        Set(manager, "worldCamera", camera);
        var inventory = new GameObject("InventoryManager", typeof(InventoryManager)).GetComponent<InventoryManager>();
        Set(manager, "inventoryManager", inventory);
        var managerData = new SerializedObject(manager);
        managerData.FindProperty("waitForDifficulty").boolValue = true;
        managerData.ApplyModifiedPropertiesWithoutUndo();
        var audio = manager.GetComponent<AudioSource>(); audio.playOnAwake = false; audio.spatialBlend = 0;
        Set(manager, "successAudioSource", audio);
        // 測試用短提示音；正式美術／音效可替換此資產。
        using (var wav = new BinaryWriter(File.Create("Assets/TestArt/Success.wav")))
        {
            const int rate = 22050, samples = 4410;
            wav.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); wav.Write(36 + samples * 2);
            wav.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); wav.Write(16);
            wav.Write((short)1); wav.Write((short)1); wav.Write(rate); wav.Write(rate * 2);
            wav.Write((short)2); wav.Write((short)16);
            wav.Write(System.Text.Encoding.ASCII.GetBytes("data")); wav.Write(samples * 2);
            for (int i = 0; i < samples; i++) wav.Write((short)(Mathf.Sin(2 * Mathf.PI * 660 * i / rate) * 8000 * (1f - (float)i / samples)));
        }
        AssetDatabase.ImportAsset("Assets/TestArt/Success.wav");
        Set(manager, "successClip", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/TestArt/Success.wav"));
        var spawner = new GameObject("ChickenSpawner", typeof(ChickenSpawner)).GetComponent<ChickenSpawner>();
        Set(spawner, "gameManager", manager); Set(spawner, "worldCamera", camera); Set(spawner, "chickenPrefab", chickenPrefab);
        var spawnerData = new SerializedObject(spawner);
        spawnerData.FindProperty("horizontalMargin").floatValue = 0.35f;
        spawnerData.ApplyModifiedPropertiesWithoutUndo();
        var difficulty = new GameObject("GameDifficultyManager", typeof(GameDifficultyManager)).GetComponent<GameDifficultyManager>();
        var menu = new GameObject("MainMenuCanvas", typeof(RectTransform), typeof(MainMenuCanvas)).GetComponent<MainMenuCanvas>();
        Set(difficulty, "gameManager", manager);
        Set(difficulty, "spawner", spawner);
        Set(difficulty, "mainMenuCanvas", menu.gameObject);
        Set(difficulty, "obstacleSprite", BuildObstacleSprite());
        Set(menu, "difficultyManager", difficulty);
        Set(menu, "inventoryManager", inventory);
        var ui = new GameObject("RescueUI", typeof(RescueMockUI)).GetComponent<RescueMockUI>();
        Set(ui, "gameManager", manager);
        new GameObject("MVP Test Controls", typeof(MvpTestControls));
        Zone("DeathZone Bottom", new Vector2(0, -5.8f), new Vector2(100, 1));
        Zone("DeathZone Left", new Vector2(-12, -0.4f), new Vector2(1, 10));
        Zone("DeathZone Right", new Vector2(12, -0.4f), new Vector2(1, 10));
        Physics2D.gravity = new Vector2(0, -9.81f);
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), "Assets/Scenes/SampleScene.unity");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", true) };
        AssetDatabase.SaveAssets();
        Debug.Log("CHICKEN_RUSH_SCENE_READY");
    }
    private static void Set(Object target, string field, Object value)
    {
        var serialized = new SerializedObject(target);
        serialized.FindProperty(field).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
    private static void BuildExampleItems(Sprite icon)
    {
        Directory.CreateDirectory("Assets/Resources/Items");
        string[] ids = { "title.rookie", "costume.yellow_hat", "furniture.wooden_chair", "butler.apprentice" };
        string[] names = { "接雞新手", "黃色小帽", "小木椅", "見習管家" };
        int[] prices = { 50, 100, 200, 500 };
        for (int i = 0; i < ids.Length; i++)
        {
            string path = "Assets/Resources/Items/" + ids[i] + ".asset";
            if (AssetDatabase.LoadAssetAtPath<ItemDataSO>(path) != null) continue;
            var item = ScriptableObject.CreateInstance<ItemDataSO>();
            item.itemId = ids[i]; item.itemName = names[i]; item.itemType = (ItemType)i;
            item.price = prices[i]; item.icon = icon;
            AssetDatabase.CreateAsset(item, path);
        }
    }
    private static Sprite BuildObstacleSprite()
    {
        var texture = new Texture2D(64, 64);
        var pixels = new Color[64 * 64];
        for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
                pixels[y * 64 + x] = new Vector2(x - 31.5f, y - 31.5f).sqrMagnitude <= 31f * 31f
                    ? Color.white : Color.clear;
        texture.SetPixels(pixels); texture.Apply();
        const string path = "Assets/TestArt/ObstacleCircle.png";
        File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 64;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    private static void Zone(string name, Vector2 position, Vector2 size)
    {
        var zone = new GameObject(name, typeof(DeathZone));
        zone.transform.position = position;
        zone.GetComponent<BoxCollider2D>().size = size;
        zone.GetComponent<BoxCollider2D>().isTrigger = true;
    }
}
