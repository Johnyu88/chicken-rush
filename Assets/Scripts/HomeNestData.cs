using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenRush
{
    [Serializable]
    public sealed class PlacedHomeItemData
    {
        public string itemId = "";
        public Vector3 position;
        public Vector3 rotation; // Euler degrees, reserved for future 3D placement.
        public Vector3 scale = Vector3.one;
        public bool IsValid => !string.IsNullOrWhiteSpace(itemId) && itemId == itemId.Trim() &&
            Finite(position) && Finite(rotation) && Finite(scale) && scale.x > 0 && scale.y > 0 && scale.z > 0;
        private static bool Finite(Vector3 v) => Valid(v.x) && Valid(v.y) && Valid(v.z);
        private static bool Valid(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        public PlacedHomeItemData Copy() => new PlacedHomeItemData { itemId = itemId, position = position, rotation = rotation, scale = scale };
    }

    [Serializable]
    public sealed class HomeNestData
    {
        public const int CurrentVersion = 1;
        public int version = CurrentVersion;
        public List<PlacedHomeItemData> placedFurniture = new List<PlacedHomeItemData>();
        public string activeButlerId = "";
        public string exteriorItemId = ""; // Reserved; no Exterior ItemType or runtime application yet.
        public List<PlacedHomeItemData> decorations = new List<PlacedHomeItemData>();
        public void Normalize()
        {
            if (version <= 0) version = CurrentVersion;
            if (placedFurniture == null) placedFurniture = new List<PlacedHomeItemData>();
            if (decorations == null) decorations = new List<PlacedHomeItemData>();
            placedFurniture.RemoveAll(item => item == null);
            decorations.RemoveAll(item => item == null);
            if (activeButlerId == null) activeButlerId = "";
            if (exteriorItemId == null) exteriorItemId = "";
            // Preserve unknown asset IDs so they can resolve when the catalog returns.
        }
        public HomeNestData Copy()
        {
            var copy = new HomeNestData { version = version, activeButlerId = activeButlerId, exteriorItemId = exteriorItemId };
            if (placedFurniture != null) foreach (var item in placedFurniture) if (item != null) copy.placedFurniture.Add(item.Copy());
            if (decorations != null) foreach (var item in decorations) if (item != null) copy.decorations.Add(item.Copy());
            return copy;
        }
    }
}
