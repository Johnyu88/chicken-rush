# Phase 6-1：Juice 音效、粒子與相機震動

在現有 `SampleScene` 進入 Play Mode 即可體驗。`GameManager` 會補齊 `AudioManager`、`EffectManager` 與主相機上的 `CameraShake`，不必重建場景。使用 `Chicken Rush > Build MVP Test Scene` 重建時，這些元件會直接放入場景，便於調整 Inspector 欄位。

## 回饋時機

| 事件 | 音效 | 粒子 | 震動 |
| --- | --- | --- | --- |
| 小雞成功進窩 | 連擊提示音 | 12 片羽毛 | 無 |
| 首次暴走 | 上揚強化音 | 跟隨小雞的橘紅火花氣場 | 強烈碰撞另行觸發 |
| 相對碰撞速度 > 6 | 若首次暴走則播放暴走音 | 若首次暴走則建立氣場 | 0.12 秒、0.08 世界單位 |
| 滿窩開始退場 | 爆破強化音 | 36 片羽毛 | 0.3 秒、0.22 世界單位 |
| 廣告救援倒數完成清場 | 爆破強化音 | 36 片羽毛 | 0.3 秒、0.22 世界單位 |

Combo 沿用既有「連續滿窩數」，不修改計分；進窩音高為 `1 + max(0, Combo) * 0.05`，上限 2。Combo 0／1／2 對應 1.0／1.05／1.1。滿窩的最後一隻先播放進窩音，再更新滿窩 Combo 並播放爆破音。

暴走沿用原本紅色、放大與彈性變化。氣場在進窩、停用或銷毀小雞時停止；停用後重新啟用仍在暴走的物理小雞，只恢復氣場，不重播暴走音。

## 音效 API 與素材替換

```csharp
AudioManager.Instance.PlaySFX(clip, pitch: 1f, volume: 1f);
AudioManager.Instance.PlayBGM(musicClip, loop: true);
AudioManager.Instance.StopBGM();
AudioManager.Instance.PlayCombo(gameManager.Combo);
```

管理器是場景內唯一實例，隨場景重載重建，不使用 `DontDestroyOnLoad`。預設 12 個獨立 SFX 聲道，滿載時輪替；每個聲道各自保存 Pitch，BGM 使用獨立循環音源。

Inspector 可指定 `Nest Clip`、`Angry Clip`、`Burst Clip`、`Bgm Clip`，並調整 SFX/BGM 音量及聲道數。留空時 `JuiceAudioSynth` 產生短音效和 8 秒循環背景旋律，供立即試玩；正式音樂與音效可直接替換。這些程序資源在管理器銷毀時釋放。舊 `GameManager` 成功音源保留為相容備援，有 AudioManager 時不會重複播放。

## 相機與粒子 API

```csharp
camera.GetComponent<CameraShake>().Shake(0.2f, 0.1f);
camera.GetComponent<CameraShake>().StopShake();
EffectManager.Instance.PlayFeathers(worldPosition);
EffectManager.Instance.PlayFeathers(worldPosition, burst: true);
ParticleSystem aura = EffectManager.Instance.AttachAnger(chicken.transform);
```

相機使用世界單位量級的局部 XY 位移；重疊震動合併，最大幅度預設 0.35，單次持續上限 2 秒。暫停時移除可見位移並凍結計時，結束／停用時清除剩餘位移。使用獨立亂數，不改變 Unity 全域遊戲亂數序列。設計用於目前固定相機；若日後接入其他絕對位置控制器，應改用獨立相機震動子節點。

EffectManager 可指定 `Feather Prefab` / `Anger Prefab`；留空時建立程序羽毛貼圖、材質及粒子系統。羽毛使用世界座標模擬、不跟著退場雞窩移動，播放完自動銷毀；同時最多 24 組噴發。暴走氣場的發射器是小雞子物件，粒子採世界座標保留短暫軌跡。自訂暴走 Prefab 應設定循環播放。

## 驗證

在隔離副本以 Unity 6000.3.23f1 執行，勿加 `-quit`：

- `-executeMethod Phase6ContractCheck.Run`：三個管理類別與公開 API。
- `-executeMethod Phase6SmokeRunner.Run`：獨立 Pitch/BGM、真實碰撞、暴走氣場生命週期、重複進窩／滿窩防護、計分、救援清場、粒子回收、相機還原與場景重載。使用專用 `ChickenRush.Tests.Phase6` 存檔。

圖形模式測試會產生 `Phase6Effects.png` 供材質／視覺檢查。音效測試驗證聲道與 Pitch 設定，不替代實機音量與主觀聽感調整。

既有專案套件與標記 Unity 版本不相容；驗證副本使用 6000.3 內建模組與 uGUI 2.0.0。原專案的套件清單與版本設定保持原樣，完整套件環境未驗證。


不想重建場景時，也可在 Edit Mode 建立空物件並加入 AudioManager／EffectManager 元件，填入素材後儲存場景；GameManager 會使用這些已配置的實例。
