using UnityEngine;

namespace ChickenRush
{
    /// <summary>Serialized references include TestArt sprites in builds without moving them into Resources.</summary>
    [CreateAssetMenu(menuName = "Chicken Rush/Chicken Art Set")]
    public sealed class ChickenArtSet : ScriptableObject
    {
        public Sprite normal, angry, happy, nest, windmill, hat;
        public static ChickenArtSet Load() => Resources.Load<ChickenArtSet>("ChickenArtSet");
        public static float UnitScale(Sprite sprite) => sprite == null ? 1 : 1f / Mathf.Max(.0001f, Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y));
    }
}
