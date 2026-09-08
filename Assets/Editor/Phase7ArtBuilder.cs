using ChickenRush;
using UnityEditor;
using UnityEngine;

public static class Phase7ArtBuilder
{
    [MenuItem("Chicken Rush/Apply Phase 7 Art To Prefabs")]
    public static void Build()
    {
        ImportArt();
        UpdatePrefab("Assets/Prefabs/Chicken.prefab", ConfigureChicken);
        UpdatePrefab("Assets/Prefabs/Nest.prefab", ConfigureNest);
        ConfigureExampleCostume();
        AssetDatabase.SaveAssets();
        Debug.Log("PHASE7_ART_READY");
    }
    public static void ImportArt()
    {
        const string root = "Assets/TestArt/";
        var art = AssetDatabase.LoadAssetAtPath<ChickenArtSet>("Assets/Resources/ChickenArtSet.asset");
        if (art == null)
        {
            art = ScriptableObject.CreateInstance<ChickenArtSet>();
            AssetDatabase.CreateAsset(art, "Assets/Resources/ChickenArtSet.asset");
        }
        art.normal = LoadSprite(root + "exec-09c8cebe-12fe-48e1-a08a-eb0c3c07808f.png");
        art.angry = LoadSprite(root + "exec-45ac9b0c-e671-4260-837b-76b744109f66.png");
        art.happy = LoadSprite(root + "exec-cb257855-b4fe-4d11-b63b-ba1f0c2b4829.png");
        art.nest = LoadSprite(root + "exec-cee4cc27-8e31-45e3-86fa-497d19298773.png");
        art.hat = LoadSprite(root + "exec-8f422321-4282-4a36-93d5-4028544a91db.png");
        const string windmillPath = root + "exec-80494f4d-adcf-448a-b4a0-11ea3f0df387.png";
        AssetDatabase.ImportAsset(windmillPath);
        var importer = (TextureImporter)AssetImporter.GetAtPath(windmillPath);
        // Only the newly moved rotor needs initial Sprite setup. Preserve all existing import settings.
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
        art.windmill = LoadSprite(windmillPath);
        EditorUtility.SetDirty(art); AssetDatabase.SaveAssets();
    }
    private static Sprite LoadSprite(string path)
    {
        AssetDatabase.ImportAsset(path);
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
                if (asset is Sprite candidate) { sprite = candidate; break; }
        if (sprite == null) throw new System.InvalidOperationException("Configured TestArt Sprite missing: " + path);
        return sprite;
    }
    private static void UpdatePrefab(string path, System.Action<GameObject> configure)
    {
        var root = PrefabUtility.LoadPrefabContents(path);
        try { configure(root); PrefabUtility.SaveAsPrefabAsset(root, path); }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }
    public static void ConfigureChicken(GameObject chicken)
    {
        var visual = chicken.GetComponent<ChickenVisual>();
        if (visual == null) visual = chicken.AddComponent<ChickenVisual>();
        var art = ChickenArtSet.Load();
        visual.SetSprites(art.normal, art.angry, art.happy);
        if (chicken.GetComponent<ChickenCostumeApplier>() == null) chicken.AddComponent<ChickenCostumeApplier>();
        EditorUtility.SetDirty(visual);
    }
    public static void ConfigureNest(GameObject nest)
    {
        var visual = nest.transform.Find("Visual");
        if (visual == null) throw new System.InvalidOperationException("Nest Visual missing");
        var renderer = visual.GetComponent<SpriteRenderer>();
        Vector3 dimensions = renderer.sprite != null ? Vector3.Scale(renderer.sprite.bounds.size, visual.localScale) : new Vector3(7, .45f, 1);
        var sprite = ChickenArtSet.Load().nest;
        renderer.sprite = sprite; renderer.color = Color.white;
        float scale = dimensions.x / sprite.bounds.size.x;
        visual.localScale = new Vector3(scale, scale, visual.localScale.z);
        visual.localPosition = new Vector3(visual.localPosition.x, -.35f, visual.localPosition.z);
        renderer.sortingOrder = -1;
    }
    public static void ConfigureExampleCostume()
    {
        var item = AssetDatabase.LoadAssetAtPath<ItemDataSO>("Assets/Resources/Items/costume.yellow_hat.asset");
        if (item == null) return;
        var hat = ChickenArtSet.Load().hat;
        if (item.costumeSprite == null || AssetDatabase.GetAssetPath(item.costumeSprite).StartsWith("Assets/Resources/Art/"))
        {
            item.costumeSprite = hat; item.costumeOffset = new Vector2(0, .36f);
            item.costumeScale = new Vector2(.65f, .65f); item.costumeRotation = 0;
        }
        if (item.icon == null || AssetDatabase.GetAssetPath(item.icon).StartsWith("Assets/TestArt/") || AssetDatabase.GetAssetPath(item.icon).StartsWith("Assets/Resources/Art/")) item.icon = hat;
        EditorUtility.SetDirty(item);
    }
}
