# 雞窩收納、計分與 Inspector 設定

更新：失敗／救援規則已由 [RESCUE_SETUP.md](RESCUE_SETUP.md) 取代本文件早期的漏接斷 Combo 描述。現在越界會暫停並顯示救援，撞飛不立即斷 Combo，救援保留 Combo；换窩期間越界也會失敗。以下雞窩 Prefab 與計分接線仍適用。

## 執行順序

1. GameManager 依 Nest Prefab 與 Nest Spawn Point 自動生成第一窩。
2. NestSensor 的 Trigger Enter／Stay 找到小雞剛體上的 ChickenController，交由 NestController 收納。小雞變為 Nested、停止物理模擬並成為雞窩子物件；重複碰撞不重複計數。
3. 預設收到第 10 隻時鎖住雞窩，Combo +1，鎖定獎勵並播放成功音效。此時發布 On Combo Changed、On Nest Reward Prepared，Score 尚未增加。
4. Coroutine 用 SmoothStep 曲線向左移動，預設 0.7 秒，終點根據正交相機左界與雞窩／小雞範圍計算。
5. 舊窩與子物件銷毀，下一幀 GameManager 入帳，生成新窩並發布 On Score Awarded。暫停或 GameOver 時流程等待恢復 Playing。

換窩期間停止 Spawner，忽略漏接對 Combo 的影響。觸控使用者需放開再按下以開始下一窩；滑鼠持續按住會自動恢復生成。已在空中的小雞仍由物理系統處理。

## 場景與 Prefab

```text
GameManager                 GameManager + AudioSource
NestSpawnPoint              空物件，放在雞窩應出現的位置
Main Camera                 Orthographic，無旋轉，朝 +Z
ChickenSpawner              ChickenSpawner

Nest Prefab                 NestController（Capacity = 10）
├── Visual                  SpriteRenderer
├── InteriorSensor          NestSensor + BoxCollider2D（Is Trigger）
└── Rim（可選）             非 Trigger Collider2D

Chicken Prefab              ChickenController + Dynamic Rigidbody2D
└── 圖片／碰撞區             SpriteRenderer、Collider2D
```

### 雞窩根物件

- 掛 NestController，Capacity 設 10，Slide Duration 設 0.7，Exit Padding 設 0.5。可按畫面調整。
- 根物件不要加 Dynamic Rigidbody2D；感測由小雞的 Dynamic 剛體觸發。
- 儲存為 Project 中的 Prefab；場景中的製作樣本移除，避免與管理器生成的第一窩重疊。
- 不再使用舊版的 GameManager、Next Nest Prefab、Slide Speed、Exit X 欄位；統一改由 GameManager 生成並注入引用。

### InteriorSensor

- 掛 NestSensor 和 BoxCollider2D，勾 Is Trigger。Nest 欄位拖雞窩根物件的 NestController；未指定會向父層尋找。
- 以 Edit Collider 調整 Size／Offset，範圍放在窩口下方的內部，不能把外壁整個包住。物理碰撞與 Trigger 是不同用途。
- 小雞碰撞範圍首次碰到這個區域就算收納，不要求整隻完全進入或穩定落地。透過區域位置控制判定嚴格度。
- Physics 2D 的 Layer Collision Matrix 需允許雞窩感測層與小雞層互動。不需要 Tag。

### GameManager

| 欄位 | 指定內容 |
|---|---|
| Nest Prefab | Project 中的雞窩 Prefab |
| Nest Spawn Point | 場景空物件 NestSpawnPoint |
| World Camera | Main Camera（正交、無旋轉） |
| Points Per Nest | 100 |
| Max Multiplier | 5 |
| Success Audio Source | 場景上專用 AudioSource，建議掛 GameManager |
| Success Clip | 自行匯入的成功音效 AudioClip |
| Pitch Per Combo | 0.1 |

AudioSource 關閉 Play On Awake、Loop，Spatial Blend 設 0（2D）。音源不能放在即將銷毀的雞窩上。未提供音效素材時仍可計分，但不會有聲音；本次沒有附帶音效檔。

Pitch = min(2.0, 1.0 + (Combo - 1) × Pitch Per Combo)，例如 1.0、1.1、1.2……2.0；音量保持 AudioSource 的設定。此專用音源不應拿來播放背景音樂。暫停只停止遊戲流程，已開始的成功音效允許播放完畢。

### 事件接線

- NestController：On Chicken Accepted 讀 CurrentCount／Capacity 更新容量 UI；On Filled 接滿窩粒子或特效。
- GameManager：On Combo Changed 的動態 int 是新 Combo；On Nest Reward Prepared 的 int 是尚待入帳的本次分數，可接浮動文字。
- On Score Awarded 的 int 是本次實際加分，不是總分。On Data Changed 可由 UI 讀取 Score、Combo、State。
- 事件只接 UI／特效，不要再呼叫加分、生成、銷毀或停用管理器／雞窩，程式已負責完整流程。移除舊版手動 AudioSource.Play 接線，避免重複播放。

## 驗收

- 前 9 隻：CurrentCount 依次增加，Score 與 Combo 不變。第 10 隻：Combo=1、Pitch=1，退場前 Score=0，退場銷毀後 Score=100。
- 同一小雞多個 Collider、Trigger Stay 或第 11 隻同幀抵達：不重複計數、不重複完成。
- 第二窩成功：Combo=2、Pitch=1.1，退場後累積 Score=300。
- 非換窩期間漏接／撞飛：Combo 歸零；下一窩獎勵回 100，Pitch 回 1。
- 持續成功：分數倍率最高 5，Pitch 最高 2；音源不隨雞窩被刪除。
- 中途 Pause：雞窩不移動、不生成；Resume 後繼續，場景始終只有一個有效雞窩。
- 首窩與新窩 Trigger 都能接收；舊窩及收納小雞已銷毀。

可執行 `Tests/Test-NestScoreState.ps1` 驗證計分規則與防重結算；生成器原有測試仍保留。Unity 編譯、碰撞回呼、實際音效與 Coroutine 場景驗收尚需在 Unity Editor 執行。
