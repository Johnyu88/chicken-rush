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
        foreach (var name in new[] { "ChickenNormal", "ChickenAngry", "ChickenHappy", "Nest", "Windmill", "YellowHat" })
        {
            string path = "Assets/Resources/Art/" + name + ".png";
            AssetDatabase.ImportAsset(path);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new System.InvalidOperationException("Missing Phase 7 art: " + path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.GetSourceTextureWidthAndHeight(out int width, out int height);
            importer.spritePixelsPerUnit = Mathf.Max(width, height);
            importer.spritePivot = new Vector2(.5f, .5f);
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
        }
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
        visual.Configure();
        if (chicken.GetComponent<ChickenCostumeApplier>() == null) chicken.AddComponent<ChickenCostumeApplier>();
        EditorUtility.SetDirty(visual);
    }
    public static void ConfigureNest(GameObject nest)
    {
        var visual = nest.transform.Find("Visual");
        if (visual == null) throw new System.InvalidOperationException("Nest Visual missing");
        var renderer = visual.GetComponent<SpriteRenderer>();
        Vector3 dimensions = renderer.sprite != null ? Vector3.Scale(renderer.sprite.bounds.size, visual.localScale) : new Vector3(7, .45f, 1);
        var sprite = Resources.Load<Sprite>("Art/Nest");
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
        var hat = Resources.Load<Sprite>("Art/YellowHat");
        if (item.costumeSprite == null)
        {
            item.costumeSprite = hat; item.costumeOffset = new Vector2(0, .36f);
            item.costumeScale = new Vector2(.65f, .65f); item.costumeRotation = 0;
        }
        if (item.icon == null || AssetDatabase.GetAssetPath(item.icon).StartsWith("Assets/TestArt/")) item.icon = hat;
        EditorUtility.SetDirty(item);
    }
}
