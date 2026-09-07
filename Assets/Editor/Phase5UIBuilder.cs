using ChickenRush;
using UnityEditor;
using UnityEngine;

public static class Phase5UIBuilder
{
    [MenuItem("Chicken Rush/Build Shop Item Card Prefab")]
    public static void Build()
    {
        System.IO.Directory.CreateDirectory("Assets/Resources/UI");
        const string path = "Assets/Resources/UI/ItemCardPrefab.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;
        var card = ShopItemCard.CreateTemplate(null, null);
        PrefabUtility.SaveAsPrefabAsset(card.gameObject, path);
        Object.DestroyImmediate(card.gameObject);
        AssetDatabase.SaveAssets();
    }
}
