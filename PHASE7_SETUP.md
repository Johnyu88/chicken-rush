# Phase 7：TestArt 正式資產綁定與飾品覆蓋

## 直接使用

現有 Chicken／Nest Prefab 已綁定 `Assets/TestArt` 的正常、生氣、慶祝、雞窩和帽子 Sprite。原本位於專案根目錄的風車 PNG 已移入 TestArt 並設定為 Sprite。目前提供的是黃色帽子，未包含草莓帽素材。

`Assets/Resources/ChickenArtSet.asset` 保存 TestArt Sprite 的直接引用，供 Prefab 初始化、風車生成及建置使用。TestArt 不需移入 Resources；Unity 會隨資產引用收錄貼圖。舊 Resources/Art 圖片保留，但本階段執行路徑使用 ChickenArtSet／TestArt。

## 表情與事件

- Chicken Prefab 根 SpriteRenderer 的預設圖檔設為正常小雞；它停用顯示，由 VisualRoot/Body 呈現並處理尺寸補償，避免縮放物理根物件。
- `ChickenAnger.OnAngerStateChanged` 為 `Action<bool>`。首次暴走時發布 `true`，重複碰撞不重複發布；目前沒有平息玩法。
- `ChickenVisual` 訂閱與解除訂閱該事件，切換生氣貼圖並播放短暫美術膨脹。原 `OnBecameAngry` 保留相容舊訂閱者。
- 高速碰撞（相對速度 > 6）或持續擠壓超過 1.5 秒，沿用既有暴走條件。
- 滿窩時整窩顯示 Happy 慶祝貼圖。

## 穿戴

```
Chicken                ← 原本 Collider2D / Rigidbody2D
└── VisualRoot          ← 視覺膨脹動畫
    ├── Body
    └── CostumeAnchor  ← SpriteRenderer，排序在 Body 前方
```

在商店購買並裝備黃色帽子後，新生成及現有小雞會透過 InventoryManager 自動顯示飾品。

`ItemDataSO.costumeSprite` 是穿戴貼圖；`icon` 仍為商店圖示。`costumeOffset`、`costumeScale`、`costumeRotation` 控制美術根節點中的位置、尺寸與角度。Sprite 會先依長邊正規化為 1 單位，因此 PPU 不同也能使用一致的穿戴尺寸。

新增草莓帽等飾品時，建立 Costume 類別的 ItemDataSO，指定穿戴 Sprite，放入 Resources/Items 並保持 itemId 唯一。沒有裝備、缺少資產或沒有穿戴 Sprite 時，覆蓋層自動隱藏。

## 匯入與物理保護

保留已配置 TestArt 的 PPU、Pivot 與匯入設定，不強制改為之前的 PPU 1254。只為新移入的風車設定初始 Sprite 匯入。

Body、CostumeAnchor、WindmillRotor 分別在美術子物件上補償 Sprite 尺寸；不改 Collider2D 大小、Trigger、Rigidbody2D 或 PhysicsMaterial2D。Nest 邊界計算忽略停用的舊 Renderer，避免它的大尺寸貼圖影響退場距離。原有暴走時 1.2 倍根縮放及 0.8 彈性仍保留。

`Chicken Rush > Apply Phase 7 Art To Prefabs` 更新資產設定與 Prefab；不重建場景。`Build MVP Test Scene` 仍是完整場景重建操作。

## 驗證

Unity 6000.3.23f1 隔離副本：
- Phase7SmokeRunner.Run：事件、表情、裝備、PPU、滿窩與物理回歸。
- Phase7SmokeRunner.VerifyMigration：原始 Prefab 遷移前後物理欄位比對。
- Phase7SmokeRunner.ValidateDeliveredAssets：確認直接 TestArt 引用與 Prefab 元件完整。
- Phase5SmokeRunner.Run／Phase6SmokeRunner.Run：商店與 Juice 回歸。

沿用內建模組＋uGUI 2.0.0 的精簡套件驗證環境；完整原套件組合仍有既有版本不相容問題。
