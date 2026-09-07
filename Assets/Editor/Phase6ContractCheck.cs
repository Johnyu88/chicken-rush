using System;
using ChickenRush;
using UnityEditor;
using UnityEngine;

public static class Phase6ContractCheck
{
    public static void Run()
    {
        try
        {
            var assembly = typeof(GameManager).Assembly;
            var audio = assembly.GetType("ChickenRush.AudioManager");
            Check(audio != null, "AudioManager is missing");
            Check(audio.GetMethod("PlaySFX") != null && audio.GetMethod("PlayBGM") != null, "Audio playback APIs missing");
            var shake = assembly.GetType("ChickenRush.CameraShake");
            Check(shake != null && shake.GetMethod("Shake", new[] { typeof(float), typeof(float) }) != null, "Shake API missing");
            Check(assembly.GetType("ChickenRush.EffectManager") != null, "EffectManager is missing");
            Debug.Log("PHASE6_CONTRACT_PASS"); EditorApplication.Exit(0);
        }
        catch (Exception ex) { Debug.LogException(ex); EditorApplication.Exit(1); }
    }
    private static void Check(bool value, string message) { if (!value) throw new Exception(message); }
}
