# 小雞衝衝 MVP

目前提供 Unity C# 原始碼與獨立規則測試，尚未包含可直接開啟的 Unity 專案設定、場景、Prefab 或音效素材。

## 程式結構

- ChickenSpawner：按住生成、左右拖動控制初速，支援滑鼠及觸控；換窩／暫停時停止生成。
- HoldSpawnInput：生成頻率與拖動偏速的獨立計算。
- ChickenController：物理、撞飛、漏接與收納狀態。
- NestSensor：雞窩內部 Trigger 偵測，辨認小雞剛體。
- NestController：容量（預設 10）、收納防重、向左滑出動畫。
- GameManager：第一窩／後續窩生成、Combo、成功音效、延後得分及遊戲／救援狀態。
- NestScoreState：鎖定獎勵、一次入帳、倍率與 Pitch 上限。

## Unity 設定

完整雞窩層級、Inspector 引用、事件與驗收步驟請見 [NEST_SETUP.md](NEST_SETUP.md)。匯入 Assets/Scripts 中全部檔案；純 C# 輔助類別無需掛物件。

小雞 Prefab 掛 ChickenController、Dynamic Rigidbody2D 與非 Trigger CircleCollider2D。Gravity 的 Y 應小於 0，不可凍結 X/Y 位置。指定 Spawner 的 GameManager、正交相機及小雞 Prefab。相機無旋轉、朝 +Z，例如 Z=-10，遊戲平面 Z=0。

Spawner 預設每秒 5 隻，按下立即生成第一隻；拖動四分之一螢幕寬達到 1.5 世界單位／秒水平初速上限。放開重設，拖回按下點恢復垂直；只影響新小雞，出生位置仍在上緣隨機。Input System 使用 Dynamic Update；Both 模式優先用 Input System。

障礙物的非 Trigger Collider2D 所在 Layer 加入小雞 knockAwayLayers。設定小雞 despawnBelowY 到畫面底部外。GameManager 的 Pause／Resume 可接 UI；EndGame 由未來生命或時間規則呼叫。

失敗與救援現已實作，詳見 [RESCUE_SETUP.md](RESCUE_SETUP.md)：DeathZone 觸發 GameOver；RescueMockUI 顯示 5 秒 Mock 廣告與重來按鈕，GameManager 負責清場並保留 Combo、或重新載入場景。每局一次救援，沒有真實廣告 SDK、物件池或正式 Canvas UI。

## 測試與版本控制

Tests/Test-HoldSpawnInput.ps1：按住、放開、拖動限制、不同幀率的生成頻率。
Tests/Test-NestScoreState.ps1：延後計分、防重入帳、Combo 中斷、倍率及 Pitch 上限。

以上為不依賴 Unity 的規則測試；物理／場景整合仍需 Play Mode 驗收。

上傳指令見 [GITHUB_SETUP.md](GITHUB_SETUP.md)；其中 2026-09-06 首次上傳盤點為歷史快照。本次新增 NestSensor、NestScoreState、雞窩測試與 NEST_SETUP.md。
