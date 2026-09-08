# Phase 7：2D 美術、表情與飾品覆蓋

## 使用方式

開啟現有場景進入 Play Mode。Chicken／Nest Prefab 已套用新美術，風車在選擇有障礙物的難度時載入。現有場景不需重建。

- `ChickenVisual` 顯示 Normal、Angry、Happy 三種貼圖。
- `ChickenAnger.OnBecameAngry` 每隻小雞首次暴走觸發一次，切換 Angry 並播放 0.24 秒的美術膨脹動畫。
- 滿窩時，Nest 內所有小雞切換 Happy；後續暴走事件不會覆蓋慶祝狀態。
- `ChickenCostumeApplier` 在小雞 Initialize 時讀取遊戲的 InventoryManager，並訂閱 Changed，支援現有小雞同步換裝。
- 新增黃色小帽穿戴貼圖；商店購買並裝備後，新生成的小雞會戴上帽子。未擁有、沒有裝備、道具 ID 無法解析或沒有穿戴貼圖時不顯示覆蓋。

## Prefab 結構與資料欄位

```
Chicken                         ← 原 Rigidbody2D、CircleCollider2D、ChickenAnger 等
└── VisualRoot                  ← 新的視覺膨脹只作用在此
    ├── Body                    ← 表情 SpriteRenderer
    └── CostumeAnchor           ← 飾品 SpriteRenderer，排序高於 Body
```

`ItemDataSO` 的 `icon` 保持商店用途；以下新欄位只用於穿戴：

- `costumeSprite`：透明飾品 Sprite。
- `costumeOffset`：相對 VisualRoot 的 XY 位置，帽子預設 `(0, 0.36)`。
- `costumeScale`：XY 尺寸，黃色小帽預設 `(0.65, 0.65)`。
- `costumeRotation`：Z 軸角度。

道具仍放在 `Assets/Resources/Items`，永久 itemId 與存檔格式不變。InventoryManager.GetEquippedItem(ItemType.Costume) 將已裝備 ID 解析為有效且已擁有的道具資產。舊道具未設定穿戴貼圖時隱藏覆蓋，不拿商店圖示代替。

## 美術與物理

六張透明 PNG 位於 `Assets/Resources/Art`。三種小雞貼圖尺寸與 Pivot 一致；雞窩維持貼圖比例，移動美術子物件對齊原收納區域。風車只旋轉 Rotor 子物件，父物件 Collider 不旋轉。

此階段不重算 PolygonCollider、不根據 Sprite 外框調整碰撞體。保留原本小雞 CircleCollider2D 半徑、Nest 的 BoxCollider2D 尺寸／位置／Trigger、風車 CircleCollider2D 半徑及 PhysicsMaterial2D 引用。原有暴走時 root 放大 1.2 倍、彈性改為 0.8 的行為保留；新的短暫視覺 pulse 不額外放大物理範圍。

`Chicken Rush > Apply Phase 7 Art To Prefabs` 可重新套用本階段美術，只更新 Prefab／範例帽子資產，不寫入場景。`Build MVP Test Scene` 也已接入新美術，但該既有操作會重建場景。

## 驗證

隔離副本執行 `-executeMethod Phase7SmokeRunner.Run`（勿加 `-quit`）：

- Prefab 遷移前後的碰撞體、縮放、Trigger 與材質比對。
- Normal／Angry／Happy、暴走事件只觸發一次、視覺 pulse 還原。
- 商店道具購買／裝備、新生小雞穿戴、現有小雞刷新、缺少資料時隱藏。
- Nest／風車美術、滿窩全體慶祝與計分不變。
- 圖形模式產生 Phase7Characters.png 與 Phase7Scene.png 供檢視。

沿用前階段的 Unity 6000.3.23f1 精簡套件隔離驗證環境；完整原套件組合仍有既有版本不相容問題。本次沒有修改場景、Packages 或 ProjectVersion。

## 美術來源

使用內建 imagegen 生成，保留透明 Alpha。提示詞與生成來源記錄在 `Assets/Resources/Art/ART_PROMPTS.md`；目前美術為本專案產出的遊戲資產，可繼續替換同名貼圖並保留 .meta GUID。
