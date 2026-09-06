# Phase 3：主選單與難度

在 Unity 執行 `Chicken Rush > Build MVP Test Scene`，儲存後進入 Play Mode。
`MainMenuCanvas` 在執行時建立標題和四個 uGUI 按鈕；選擇後才生成第一個雞窩及障礙物。

| 難度 | 障礙物數量 | 雞窩速度（世界單位／秒） | 小雞基本彈性 |
|---|---:|---:|---:|
| 悠閒 | 0 | 0 | 0.4 |
| 地獄 | 1 | 1.5 | 0.4 |
| 惡魔 | 2 | 2.5 | 0.55 |
| 變態 | 3 | 4 | 0.7 |

- 數值集中於 `Assets/Scripts/GameDifficulty.cs`。
- `MvpSceneBuilder` 僅負責 Editor 場景與素材生成；`GameDifficultyManager` 負責執行期套用難度，支援建置後的遊戲。
- 雞窩以 Ping-Pong 左右移動，滿窩退場時停止巡航；後續雞窩沿用本局速度。
- 窄畫面會一起縮放雞窩和感應區，保留移動空間。移動範圍依生成時的鏡頭寬度計算。
- 主選單點擊必須放開後，下一次按住才開始落雞。
- 生氣後彈性仍為 0.8；本局基本材質不修改 Prefab 共用資產。
- 「放棄重來」重新載入場景，返回主選單選擇難度。
- 選單優先使用 Inspector 的 `Chinese Font`；未指定時使用系統中文字型。發行至其他平台時請指定可隨遊戲發布的繁體中文字型。

驗證命令：

```powershell
./Tests/Test-GameDifficulty.ps1
./Tests/Test-HoldSpawnInput.ps1
./Tests/Test-NestScoreState.ps1
```

Unity 整合測試入口：`-batchmode -projectPath "專案路徑" -executeMethod Phase3SmokeRunner.Run -logFile "log路徑"`。
不要加入 `-quit`；測試完成後會自行退出。請在隔離副本執行，因為測試會重建測試場景。
測試包含四個 UI 按鈕、重複選擇防護、障礙物、出生材質、雞窩移動、暫停、滿窩計分及換窩後難度，變態模式另以 9:16 鏡頭驗證。
