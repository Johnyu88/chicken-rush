using System;
using ChickenRush;
using UnityEditor;
using UnityEngine;

/// <summary>使用真實 Play Mode 與 Physics2D 的冒煙測試；命令列 -executeMethod MvpSmokeRunner.Run。</summary>
[InitializeOnLoad]
public static class MvpSmokeRunner
{
    private static int stage;
    private static double deadline;
    private static GameManager manager;
    private static ChickenController prefab;
    private static double adStarted;
    static MvpSmokeRunner()
    {
        if (SessionState.GetBool("ChickenRushSmoke", false)) EditorApplication.update += Tick;
    }
    public static void Run()
    {
        MvpSceneBuilder.Build();
        SessionState.SetBool("ChickenRushSmoke", true);
        EditorApplication.EnterPlaymode();
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
    private static ChickenController Chicken(Vector3 position)
    {
        var chicken = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity);
        chicken.Initialize(manager, Vector2.zero);
        return chicken;
    }
    private static void Next(int value, double timeout = 10)
    {
        stage = value; deadline = EditorApplication.timeSinceStartup + timeout;
    }
    private static void Tick()
    {
        if (!EditorApplication.isPlaying || EditorApplication.isPaused) return;
        try
        {
            if (stage != 0 && EditorApplication.timeSinceStartup > deadline) throw new Exception("Timeout at stage " + stage);
            switch (stage)
            {
                case 0:
                    manager = UnityEngine.Object.FindFirstObjectByType<GameManager>();
                    if (manager == null || manager.ActiveNest == null) return;
                    prefab = AssetDatabase.LoadAssetAtPath<ChickenController>("Assets/Prefabs/Chicken.prefab");
                    // 真實掉落進入 Sensor，不直接呼叫收納函式。
                    Chicken(new Vector3(0, -2, 0)); Next(1); break;
                case 1:
                    if (manager.ActiveNest.CurrentCount != 1) return;
                    Check(manager.Score == 0 && manager.Combo == 0, "Premature score");
                    for (int i = 0; i < 9; i++) Chicken(new Vector3(-2.8f + i * 0.65f, -2, 0));
                    Next(2); break;
                case 2:
                    if (manager.Score != 100) return;
                    Check(manager.Combo == 1 && manager.ActiveNest.CurrentCount == 0, "Nest transition failed");
                    Chicken(new Vector3(0, -5, 0)); Next(3); break;
                case 3:
                    if (manager.State != GameManager.GameState.GameOver) return;
                    Check(Time.timeScale == 0 && manager.Combo == 1, "Failure lost combo or did not pause");
                    manager.RequestRescue(); manager.RequestRescue();
                    adStarted = EditorApplication.timeSinceStartup; Next(4, 12); break;
                case 4:
                    if (!manager.IsPlaying) return;
                    Check(EditorApplication.timeSinceStartup - adStarted >= 4.9, "Ad shorter than five seconds");
                    Check(manager.Score == 100 && manager.Combo == 1 && Time.timeScale == 1, "Rescue did not preserve state");
                    Check(UnityEngine.Object.FindObjectsByType<ChickenController>(FindObjectsSortMode.None).Length == 0, "Rescue did not clear chickens");
                    Chicken(new Vector3(0, -5, 0)); Next(5); break;
                case 5:
                    if (manager.IsPlaying) return;
                    manager.RequestRescue();
                    Check(manager.Rescue == GameManager.RescueState.Used, "Second rescue was allowed");
                    manager.RestartGame(); Next(6); break;
                case 6:
                    manager = UnityEngine.Object.FindFirstObjectByType<GameManager>();
                    if (manager == null || manager.ActiveNest == null || !manager.IsPlaying) return;
                    Check(manager.Score == 0 && manager.Combo == 0 && manager.Rescue == GameManager.RescueState.Available, "Restart state incorrect");
                    Debug.Log("CHICKEN_RUSH_SMOKE_PASS: physical collection, scoring, death, 5s rescue, combo preservation, restart");
                    SessionState.SetBool("ChickenRushSmoke", false);
                    EditorApplication.update -= Tick;
                    EditorApplication.Exit(0); break;
            }
        }
        catch (Exception error)
        {
            Debug.LogError("CHICKEN_RUSH_SMOKE_FAIL: " + error);
            SessionState.SetBool("ChickenRushSmoke", false);
            EditorApplication.update -= Tick;
            EditorApplication.Exit(1);
        }
    }
}
