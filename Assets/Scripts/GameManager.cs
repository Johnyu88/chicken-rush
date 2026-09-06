using System.Collections;
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
        [Header("雞窩生成：由管理器產生第一窩及後續雞窩")]
        [SerializeField] private NestController nestPrefab;
        [SerializeField] private Transform nestSpawnPoint;
        [SerializeField] private Camera worldCamera;
        [Header("成功音效：使用場景上的專用音源")]
        [SerializeField] private AudioSource successAudioSource;
        [SerializeField] private AudioClip successClip;
        [SerializeField, Min(0f)] private float pitchPerCombo = 0.1f;
        [System.Serializable] public class IntEvent : UnityEvent<int> { }
        [SerializeField] private IntEvent onComboChanged = new IntEvent();
        [Tooltip("满窩時計算好的獎勵，尚未入帳；可顯示 +100 等提示。")]
        [SerializeField] private IntEvent onNestRewardPrepared = new IntEvent();
        [Tooltip("退場銷毀後發布本次實際加分值。")]
        [SerializeField] private IntEvent onScoreAwarded = new IntEvent();
        [Header("事件：UI 可讀取下方唯讀屬性更新畫面")]
        [SerializeField] private UnityEvent onDataChanged = new UnityEvent();
        [SerializeField] private UnityEvent onRescueRequested = new UnityEvent();

        public GameState State { get; private set; } = GameState.Playing;
        public RescueState Rescue { get; private set; } = RescueState.Available;
        private readonly NestScoreState scoring = new NestScoreState();
        public int Score => scoring.Score;
        public int Combo => scoring.Combo;
        public NestController ActiveNest { get; private set; }
        public bool IsChangingNest { get; private set; }
        public bool IsPlaying => State == GameState.Playing;

        // MVP 假設本元件獨占 Time.timeScale；正式整合其他系統時改由統一的暫停服務管理。
        private void Awake() { Time.timeScale = 1f; }
        private void OnDestroy() { Time.timeScale = 1f; }

        private void Start()
        {
            if (worldCamera == null) worldCamera = Camera.main;
            if (nestPrefab == null || nestSpawnPoint == null || worldCamera == null || !worldCamera.orthographic)
            {
                Debug.LogError("GameManager 需要雞窩 Prefab、生成點與正交相機。", this);
                EndGame();
                return;
            }
            SpawnNextNest();
        }

        private void SpawnNextNest()
        {
            ActiveNest = Instantiate(nestPrefab, nestSpawnPoint.position, nestSpawnPoint.rotation);
            ActiveNest.Initialize(this, worldCamera);
        }

        /// <summary>只有目前的滿窩可開始結算。鎖定獎勵，更新 Combo，再進行退場。</summary>
        public void BeginNestCompletion(NestController nest)
        {
            if (!IsPlaying || IsChangingNest || nest == null || nest != ActiveNest || !nest.IsFull) return;
            if (!scoring.Begin(pointsPerNest, maxMultiplier)) return;
            IsChangingNest = true;
            if (successAudioSource != null && successClip != null)
            {
                successAudioSource.pitch = scoring.Pitch(pitchPerCombo);
                successAudioSource.PlayOneShot(successClip);
            }
            StartCoroutine(FinishNest(nest));
            onComboChanged.Invoke(Combo);
            onNestRewardPrepared.Invoke(scoring.PendingPoints);
            onDataChanged.Invoke();
        }

        private IEnumerator FinishNest(NestController nest)
        {
            yield return nest.SlideOut();
            if (nest != null) Destroy(nest.gameObject);
            // Destroy 延至幀末生效；下一幀才入帳並生成新窩，Coroutine 不掛在即將銷毀的物件。
            yield return null;
            while (!IsPlaying) yield return null;
            ActiveNest = null;
            int awarded = scoring.Settle();
            SpawnNextNest();
            IsChangingNest = false;
            onScoreAwarded.Invoke(awarded);
            onDataChanged.Invoke();
        }

        /// <summary>撞飛或漏接會中斷 Combo；此骨架不自動結束遊戲，失敗規則由外部呼叫 EndGame。</summary>
        public void RegisterMiss()
        {
            if (!IsPlaying || IsChangingNest) return; // 換窩空窗漏接不破壞已完成的連擊。
            scoring.Miss();
            onComboChanged.Invoke(Combo);
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
            scoring.Miss();
            onComboChanged.Invoke(Combo);
            SetState(success ? GameState.Playing : GameState.GameOver);
        }
    }
}
