using UnityEngine;

namespace ChickenRush
{
    /// <summary>
    /// 無需 Canvas／額外 UI 套件的 IMGUI 測試彈窗。正式版本可替換成 uGUI，保留管理器接口。
    /// 所有倒數與清場由 GameManager 執行；畫面僅顯示狀態及轉送按鈕動作。
    /// </summary>
    public class RescueMockUI : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [Tooltip("可指定支援繁體中文的 Font，避免測試裝置缺字。")]
        [SerializeField] private Font chineseFont;

        private void OnGUI()
        {
            if (gameManager == null || gameManager.State != GameManager.GameState.GameOver) return;
            Matrix4x4 oldMatrix = GUI.matrix;
            int oldDepth = GUI.depth;
            Color oldColor = GUI.color;
            bool oldEnabled = GUI.enabled;
            // 以 600 虛擬寬度呈現；直橫向皆縮放到畫面內，不依賴 timeScale。
            float scale = Mathf.Min(Screen.width / 600f, Screen.height / 400f);
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * scale);
            GUI.depth = -1000;
            float width = Screen.width / scale;
            float height = Screen.height / scale;
            GUI.color = new Color(0f, 0f, 0f, 0.85f);
            GUI.DrawTexture(new Rect(0f, 0f, width, height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUIStyle label = new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            GUIStyle button = new GUIStyle(GUI.skin.button) { fontSize = 20, wordWrap = true };
            if (chineseFont != null) { label.font = chineseFont; button.font = chineseFont; }
            float x = (width - 540f) / 2f;
            float y = (height - 280f) / 2f;
            if (gameManager.Rescue == GameManager.RescueState.Active)
            {
                GUI.Label(new Rect(x, y, 540f, 100f), "Mock 廣告（無真實廣告）", label);
                GUI.Label(new Rect(x, y + 100f, 540f, 100f),
                    "母雞掃場準備中… " + Mathf.CeilToInt(gameManager.RescueSecondsRemaining) + " 秒", label);
            }
            else
            {
                GUI.Label(new Rect(x, y, 540f, 70f), "小雞掉出畫面了！", label);
                GUI.enabled = gameManager.Rescue == GameManager.RescueState.Available;
                if (GUI.Button(new Rect(x, y + 85f, 540f, 75f),
                    GUI.enabled ? "看5 秒廣告救援（母雞掃場）" : "本局救援已使用", button)) gameManager.RequestRescue();
                GUI.enabled = true;
                if (GUI.Button(new Rect(x, y + 180f, 540f, 75f), "放棄重來", button)) gameManager.RestartGame();
            }
            GUI.matrix = oldMatrix;
            GUI.depth = oldDepth;
            GUI.color = oldColor;
            GUI.enabled = oldEnabled;
        }
    }
}
