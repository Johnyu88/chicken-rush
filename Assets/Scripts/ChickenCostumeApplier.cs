using UnityEngine;

namespace ChickenRush
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ChickenVisual))]
    public sealed class ChickenCostumeApplier : MonoBehaviour
    {
        private InventoryManager inventory;
        private SpriteRenderer overlay;
        public void Initialize(InventoryManager manager)
        {
            Unsubscribe(); inventory = manager;
            var visual = GetComponent<ChickenVisual>();
            overlay = visual.CostumeAnchor;
            if (isActiveAndEnabled && inventory != null) inventory.Changed += Refresh;
            Refresh();
        }
        private void OnEnable() { if (inventory != null) inventory.Changed += Refresh; Refresh(); }
        private void OnDisable() { Unsubscribe(); }
        private void Unsubscribe() { if (inventory != null) inventory.Changed -= Refresh; }
        private void Refresh()
        {
            if (overlay == null) return;
            var item = inventory != null ? inventory.GetEquippedItem(ItemType.Costume) : null;
            overlay.sprite = item != null ? item.costumeSprite : null;
            overlay.enabled = overlay.sprite != null;
            if (!overlay.enabled) return;
            overlay.transform.localPosition = new Vector3(item.costumeOffset.x, item.costumeOffset.y, 0);
            overlay.transform.localScale = new Vector3(item.costumeScale.x, item.costumeScale.y, 1);
            overlay.transform.localRotation = Quaternion.Euler(0, 0, item.costumeRotation);
            overlay.color = Color.white;
        }
    }
}
