# 小雞衝衝 MVP 核心腳本

這是四個核心 Unity C# 腳本與一個生成輸入輔助類別，不是完整 Unity 專案。採用 MonoBehaviour、Inspector 引用與 UnityEvent。生成器可使用舊 Input Manager，亦支援已安裝的 Input System。

## 職責與呼叫方向

- ChickenSpawner → ChickenController.Initialize：生成小雞與初始衝量。
- ChickenController → NestController.TryAcceptChicken：Trigger 進窩判定。
- ChickenController → GameManager.RegisterMiss：撞飛／漏接中斷連擊。
- NestController → GameManager.RegisterNestCompleted：填滿得分，接著播放事件、向左滑動並生成下一窩。
- GameManager：保存遊戲、分數、Combo 與救援狀態；其他腳本讀取 IsPlaying。

## Unity 場景設定

1. 將 Assets/Scripts 中全部檔案放入 Unity 專案（包含 HoldSpawnInput.cs，無需掛在物件上）。建立空物件掛 GameManager；場景只放一個。
2. 相機使用 Orthographic、無旋轉、朝向 +Z；例如 Z=-10，遊戲平面 Z=0。
3. 小雞 Prefab 掛 ChickenController、Dynamic Rigidbody2D（Gravity Scale > 0）及非 Trigger CircleCollider2D。高速運動可在 Inspector 選擇 Continuous 碰撞偵測。小雞由 Spawner 生成，手動放入場景的小雞需要呼叫 Initialize。
4. 建立雞窩根物件掛 NestController，在根或子物件加 Collider2D 並勾選 Is Trigger。第一個場景雞窩指定 GameManager；儲存雞窩 Prefab，將 nextNestPrefab 設為它自己或另一個雞窩 Prefab。
5. 建立 Spawner，指定 GameManager、相機與小雞 Prefab。設定 Chickens Per Second（預設 5）、Max Horizontal Speed（1.5）、Drag Width Fraction（0.25）、Initial Downward Speed（0.5）與 Gravity Scale（1）。舊版的 spawnInterval、angleRange、launchImpulse 已替換，原有 Inspector 數值需重新設定。
6. 建立障礙物的非 Trigger Collider2D，將其 Layer 加入小雞的 knockAwayLayers；Physics 2D Layer Collision Matrix 必須允許相關碰撞。
7. 按鏡頭大小設定雞窩 exitX（整窩完全離開左邊）與小雞 despawnBelowY。事件可接場景音源的 Play、UI 的更新方法；onFilled 不要再呼叫加分，程式已處理。

## MVP 規則與擴充接口

- 進入收納 Trigger 立即算進窩，不要求落地穩定；收納後保留位置並停止小雞物理模擬。
- Combo 為連續填滿窩數；得分 = pointsPerNest × 限制上限後的 Combo。撞飛或落出底部只斷 Combo。
- 按住螢幕或滑鼠左鍵立即生成一隻，之後每秒生成 N 隻，放開停止並清除計時餘數。N=0 停止生成。左右拖動以本次按下位置為基準，拖動四分之一畫面寬達到預設水平初速上限；停住手指會維持偏角，拖回起點恢復垂直。
- 保留上緣隨機出生位置；拖動控制初速而非出生位置，也不會轉向已生成的小雞。生成器設定 Dynamic 剛體與正的 Gravity Scale；Physics 2D 全域 Gravity 的 Y 須小於 0，Prefab 不可凍結 X/Y 位置。
- 單指操作，第二根手指不接管正在進行的拖動。支援舊 Input Manager 或 Input System；Both 優先用 Input System，無需建立 Input Actions。Input System 使用預設 Dynamic Update 輸入更新模式。
- 雞窩滑出期間按住仍會生成；如需切換保護期，可再加遊戲階段或生成開關。暫停、失焦與停用會清除生成餘數及偏角，不補發中斷期間的小雞；觸控恢复後需重新按下，滑鼠若仍按住則視為新一輪按下。
- EndGame 由未來的生命／時間系統呼叫。Pause、Resume 可接 UI；GameOver 與 Paused 皆將 timeScale 設為 0。
- 救援只有狀態接口：RequestRescue 發事件後，外部清理危險並呼叫 CompleteRescue(true/false)。救援 UI 動畫應使用不受 timeScale 影響的時間。此版本沒有廣告 SDK、清場或重新開局流程。
- 尚未包含物件池、UI 點擊排除、視覺排列或完整關卡；事件監聽器應只做表現，避免同步銷毀控制器。

## Unity Play Mode 驗收清單（待執行）

- 未按住不生成；Playing 按住時立即生成，再以 N 隻／秒生成；放開停止。生成點位於上邊界外，左右拖動只影響新小雞初速，放開重按偏速歸零。
- 在手機測試單指拖動、第二指加入、放開及取消觸控；桌面測試滑鼠拖動、失焦及恢復。檢查新舊輸入後端，以及 Rigidbody2D 在重力作用下持續下落。
- 單隻小雞即使接觸多個 Trigger 也只加一次數量；滿窩只加分與播放事件一次。
- 容量 3：前兩隻不計分，第三隻得 100 分；連續下一窩得 200 分；漏接後下一窩回到 100 分。
- 障礙碰撞只回報一次失誤；撞飛後落底不重複回報。
- 雞窩完全離開左界後切換，原窩與其小雞一併清理。
- 暫停／GameOver 停止物理與生成；Resume 只恢復 Paused，救援最多一次。

API 參考：[Unity Rigidbody2D.AddForce](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Rigidbody2D.AddForce.html)、[Unity OnTriggerEnter2D](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/MonoBehaviour.OnTriggerEnter2D.html)。

## 已執行的獨立測試

`Tests/Test-HoldSpawnInput.ps1` 編譯並執行實際 HoldSpawnInput.cs，驗證按下立即生成、放開停止、左右速度限制、重設與 N=0，以及 30／60／120 FPS 的生成數一致、低幀率補足生成。可用 PowerShell 執行該測試；這些測試不涵蓋 Unity 輸入與物理，仍需完成上述 Play Mode 驗收。
