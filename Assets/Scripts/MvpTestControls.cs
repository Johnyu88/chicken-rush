using UnityEngine;

namespace ChickenRush
{
    /// <summary>測試場景專用 HUD，方便觀察分數與快速重現成功／死亡流程。</summary>
    public class MvpTestControls : MonoBehaviour
    {
        private GameManager manager;
        private void Start() { manager = FindFirstObjectByType<GameManager>(); }
        private void OnGUI()
        {
            if (manager == null) return;
            GUI.Box(new Rect(10, 10, 350, 90), "");
            GUI.Label(new Rect(20, 15, 330, 25), "Chicken Rush MVP | Hold mouse/touch to spawn");
            GUI.Label(new Rect(20, 42, 330, 25), "Score: " + manager.Score + "  Combo: " + manager.Combo + "  State: " + manager.State);
            GUI.Label(new Rect(20, 68, 330, 25), "Nest: " + (manager.ActiveNest == null ? "-" : manager.ActiveNest.CurrentCount + "/" + manager.ActiveNest.Capacity));
        }
    }
}
