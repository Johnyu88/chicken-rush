using System;
using ChickenRush;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Batch entry: -executeMethod Phase3SmokeRunner.Run (without -quit).</summary>
[InitializeOnLoad]
public static class Phase3SmokeRunner
{
    private static int difficultyIndex, stage, frames;
    private static double deadline, sampleTime;
    private static GameManager manager;
    private static Vector3 nestPosition;
    static Phase3SmokeRunner()
    {
        if (SessionState.GetBool("ChickenRushPhase3Smoke", false)) EditorApplication.update += Tick;
    }
    public static void Run()
    {
        MvpSceneBuilder.Build();
        SessionState.SetBool("ChickenRushPhase3Smoke", true);
        EditorApplication.EnterPlaymode();
    }
    private static void Check(bool condition, string message)
    { if (!condition) throw new Exception(message); }
    private static void Tick()
    {
        if (!EditorApplication.isPlaying) return;
        try
        {
            if (deadline == 0) deadline = EditorApplication.timeSinceStartup + 30;
            Check(EditorApplication.timeSinceStartup < deadline, "Phase 3 timeout at stage " + stage);
            var settings = GameDifficultySettings.For((GameDifficulty)difficultyIndex);
            if (stage == 0)
            {
                if (++frames < 5) return;
                manager = Object.FindFirstObjectByType<GameManager>();
                Check(manager != null && manager.State == GameManager.GameState.MainMenu, "Must start in menu");
                Check(manager.ActiveNest == null && Object.FindObjectsByType<ChickenController>(FindObjectsSortMode.None).Length == 0,
                    "Gameplay started before selection");
                // Exercise narrow portrait bounds as well as the default landscape view.
                if (difficultyIndex == 3) Camera.main.aspect = 9f / 16f;
                var menu = Object.FindFirstObjectByType<MainMenuCanvas>();
                var buttons = menu.GetComponentsInChildren<Button>();
                Check(buttons.Length == 4, "Expected four difficulty buttons");
                buttons[difficultyIndex].onClick.Invoke();
                Check(!menu.gameObject.activeSelf && manager.IsPlaying, "Button did not start game/hide menu");
                Check(GameObject.Find("Pachinko Obstacles").transform.childCount == settings.ObstacleCount, "Obstacle count");
                Check(Mathf.Approximately(manager.NestMoveSpeed, settings.NestSpeed), "Nest speed");
                var selector = Object.FindFirstObjectByType<GameDifficultyManager>();
                selector.SelectDifficulty(GameDifficulty.Relaxed);
                Check(selector.SelectedDifficulty == (GameDifficulty)difficultyIndex, "Repeated click changed active difficulty");
                Object.FindFirstObjectByType<ChickenSpawner>().SpawnChicken();
                var chicken = Object.FindFirstObjectByType<ChickenController>();
                Check(chicken != null && Mathf.Approximately(chicken.GetComponent<Rigidbody2D>().sharedMaterial.bounciness,
                    settings.BaseBounciness), "Spawned chicken base bounciness");
                Check(chicken.GetComponent<Collider2D>().sharedMaterial == chicken.GetComponent<Rigidbody2D>().sharedMaterial,
                    "Collider overrides difficulty material");
                Object.Destroy(chicken.gameObject);
                nestPosition = manager.ActiveNest.transform.position;
                sampleTime = EditorApplication.timeSinceStartup;
                stage = 1;
            }
            else if (stage == 1 && EditorApplication.timeSinceStartup - sampleTime > 0.4)
            {
                float distance = Mathf.Abs(manager.ActiveNest.transform.position.x - nestPosition.x);
                Check(settings.NestSpeed == 0 ? distance < 0.001f : distance > 0.01f, "Nest movement does not match difficulty");
                manager.Pause();
                nestPosition = manager.ActiveNest.transform.position;
                sampleTime = EditorApplication.timeSinceStartup;
                stage = 2;
            }
            else if (stage == 2 && EditorApplication.timeSinceStartup - sampleTime > 0.2)
            {
                Check(manager.ActiveNest.transform.position == nestPosition, "Paused nest moved");
                manager.Resume();
                var prefab = AssetDatabase.LoadAssetAtPath<ChickenController>("Assets/Prefabs/Chicken.prefab");
                for (int i = 0; i < manager.ActiveNest.Capacity; i++)
                {
                    var chicken = Object.Instantiate(prefab, manager.ActiveNest.transform.position, Quaternion.identity);
                    chicken.Initialize(manager, Vector2.zero);
                    Check(manager.ActiveNest.TryAcceptChicken(chicken), "Nest rejected test chicken");
                }
                stage = 3;
            }
            else if (stage == 3 && !manager.IsChangingNest)
            {
                Check(manager.Score == 100 && manager.ActiveNest.CurrentCount == 0, "Nest replacement/score regression");
                nestPosition = manager.ActiveNest.transform.position;
                sampleTime = EditorApplication.timeSinceStartup;
                stage = 4;
            }
            else if (stage == 4 && EditorApplication.timeSinceStartup - sampleTime > 0.4)
            {
                float distance = Mathf.Abs(manager.ActiveNest.transform.position.x - nestPosition.x);
                Check(settings.NestSpeed == 0 ? distance < 0.001f : distance > 0.01f, "Replacement nest lost difficulty");
                Debug.Log("PHASE3_PASS: " + (GameDifficulty)difficultyIndex);
                if (++difficultyIndex == 4)
                {
                    SessionState.SetBool("ChickenRushPhase3Smoke", false);
                    Debug.Log("CHICKEN_RUSH_PHASE3_PASS");
                    EditorApplication.Exit(0);
                    return;
                }
                stage = 0; frames = 0; deadline = 0;
                SceneManager.LoadScene("SampleScene");
            }
        }
        catch (Exception ex)
        {
            SessionState.SetBool("ChickenRushPhase3Smoke", false);
            Debug.LogException(ex);
            EditorApplication.Exit(1);
        }
    }
}
