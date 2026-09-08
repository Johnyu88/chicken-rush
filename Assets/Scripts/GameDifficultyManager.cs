using UnityEngine;

namespace ChickenRush
{
    public sealed class GameDifficultyManager : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private ChickenSpawner spawner;
        [SerializeField] private GameObject mainMenuCanvas;
        [SerializeField] private Sprite obstacleSprite;
        private PhysicsMaterial2D sessionMaterial;
        private GameObject obstacleRoot;
        public GameDifficulty SelectedDifficulty { get; private set; }
        public bool HasStarted { get; private set; }

        public void SelectDifficulty(GameDifficulty difficulty)
        {
            if (HasStarted || gameManager == null || spawner == null ||
                gameManager.State != GameManager.GameState.MainMenu) return;
            var settings = GameDifficultySettings.For(difficulty);
            sessionMaterial = new PhysicsMaterial2D("Chicken Difficulty (Session)")
            { friction = 0.2f, bounciness = settings.BaseBounciness };
            obstacleRoot = new GameObject("Pachinko Obstacles");
            for (int i = 0; i < settings.ObstacleCount; i++)
            {
                var obstacle = new GameObject("Pachinko Obstacle " + (i + 1), typeof(SpriteRenderer), typeof(CircleCollider2D));
                obstacle.transform.SetParent(obstacleRoot.transform);
                obstacle.transform.position = new Vector3(i % 2 == 0 ? -0.65f : 0.65f, 2f - 1.6f * i, 0f);
                obstacle.transform.localScale = Vector3.one * 1.1f;
                var visual = obstacle.GetComponent<SpriteRenderer>();
                var windmill = Resources.Load<Sprite>("Art/Windmill");
                visual.sprite = obstacleSprite;
                if (windmill != null)
                {
                    visual.enabled = false;
                    var rotor = new GameObject("WindmillRotor", typeof(SpriteRenderer), typeof(WindmillVisual));
                    rotor.transform.SetParent(obstacle.transform, false);
                    rotor.GetComponent<SpriteRenderer>().sprite = windmill;
                    rotor.GetComponent<WindmillVisual>().degreesPerSecond = i % 2 == 0 ? 55 : -55;
                }
                visual.color = new Color(0.35f, 0.7f, 1f);
                var collider = obstacle.GetComponent<CircleCollider2D>();
                collider.radius = 31f / 64f;
                collider.sharedMaterial = sessionMaterial;
            }
            spawner.ConfigureDifficulty(sessionMaterial);
            if (!gameManager.StartGame(settings))
            {
                obstacleRoot.SetActive(false);
                Destroy(obstacleRoot);
                spawner.ConfigureDifficulty(null);
                Destroy(sessionMaterial);
                Debug.LogError("無法開始遊戲：請檢查 GameManager 的場景引用。", this);
                return;
            }
            SelectedDifficulty = difficulty;
            HasStarted = true;
            if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
        }

        private void OnDestroy()
        {
            if (sessionMaterial != null) Destroy(sessionMaterial);
        }
    }
}
