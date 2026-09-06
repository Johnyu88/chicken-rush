using UnityEngine;
using UnityEngine.Events;

namespace ChickenRush
{
    /// <summary>單一場景的遊戲資料中心；場景中只放一個，由 Inspector 明確注入其他元件。</summary>
    public class GameManager : MonoBehaviour
    {
        public enum GameState { Playing, GameOver, Paused }
        public enum RescueState { Available, Active, Used }

        [Header("計分設定")]
        [SerializeField, Min(1)] private int pointsPerNest = 100;
        [SerializeField, Min(1)] private int maxMultiplier = 5;
        [Header("事件：UI 可讀取下方唯讀屬性更新畫面")]
        [SerializeField] private UnityEvent onDataChanged = new UnityEvent();
        [SerializeField] private UnityEvent onRescueRequested = new UnityEvent();

        public GameState State { get; private set; } = GameState.Playing;
        public RescueState Rescue { get; private set; } = RescueState.Available;
        public int Score { get; private set; }
        public int Combo { get; private set; }
        public bool IsPlaying => State == GameState.Playing;

        // MVP 假設本元件獨占 Time.timeScale；正式整合其他系統時改由統一的暫停服務管理。
        private void Awake() { Time.timeScale = 1f; }
        private void OnDestroy() { Time.timeScale = 1f; }

        /// <summary>由雞窩完成時呼叫一次。Combo 定義為「連續填滿雞窩」，首次倍率為 1。</summary>
        public void RegisterNestCompleted()
        {
            if (!IsPlaying) return;
            Combo++;
            Score += Mathf.Max(1, pointsPerNest) * Mathf.Clamp(Combo, 1, Mathf.Max(1, maxMultiplier));
            onDataChanged.Invoke();
        }

        /// <summary>撞飛或漏接會中斷 Combo；此骨架不自動結束遊戲，失敗規則由外部呼叫 EndGame。</summary>
        public void RegisterMiss()
        {
            if (!IsPlaying) return;
            Combo = 0;
            onDataChanged.Invoke();
        }

        public void Pause() { if (IsPlaying) SetState(GameState.Paused); }
        public void Resume() { if (State == GameState.Paused) SetState(GameState.Playing); }
        public void EndGame() { if (State != GameState.GameOver) SetState(GameState.GameOver); }

        private void SetState(GameState state)
        {
            State = state;
            Time.timeScale = IsPlaying ? 1f : 0f; // 同時停止物理、生成倒數與雞窩滑動。
            onDataChanged.Invoke();
        }

        /// <summary>預留一次救援流程：GameOver → Active → Used。事件接救援 UI 或復活動畫。</summary>
        public void RequestRescue()
        {
            if (State != GameState.GameOver || Rescue != RescueState.Available) return;
            Rescue = RescueState.Active;
            onDataChanged.Invoke();
            onRescueRequested.Invoke();
        }

        /// <summary>外部先清理危險物件／重設場景位置，再回報救援是否成功；取消仍消耗救援。</summary>
        public void CompleteRescue(bool success)
        {
            if (Rescue != RescueState.Active) return;
            Rescue = RescueState.Used;
            Combo = 0;
            SetState(success ? GameState.Playing : GameState.GameOver);
        }
    }
}
