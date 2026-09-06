# Phase 4：道具資產與玩家存檔

重新開啟 `Assets/Scenes/SampleScene.unity` 後進入 Play Mode，主選單右上角會顯示金幣。
場景可透過 `Chicken Rush > Build MVP Test Scene` 重建；重建不會清除玩家存檔或覆寫已有的範例道具。

## 新增道具

1. 在 Project 視窗選擇 `Create > Chicken Rush > Item`。
2. 設定名稱、類別（Title／Costume／Furniture／Butler）、圖示與非負價格。
3. `itemId` 自動產生；每件道具必須唯一，發布後不可改名或重新產生 ID。若使用 Duplicate 複製資產，請手動換成新的唯一 ID。
4. 可把資產放在 `Assets/Resources/Items`；未來選單可以用 `Resources.LoadAll<ItemDataSO>("Items")` 取得清單。

已有四個範例：接雞新手（50）、黃色小帽（100）、小木椅（200）、見習管家（500）。圖示使用測試素材，可直接替換。

## 操作接口

取得場景中的 `InventoryManager` 引用後呼叫：

```csharp
bool granted = inventory.AddCoins(10);
bool bought = inventory.BuyItem(item);
bool equipped = inventory.EquipItem(item);
int coins = inventory.Coins;
string titleId = inventory.GetEquippedItemId(ItemType.Title);
PlayerData snapshot = inventory.GetSnapshot();
```

方法回傳成功與否。負數／零金幣增額、整數溢位、餘額不足、重複購買、無效道具及裝備未持有道具都會被拒絕。
價格 0 代表免費道具；每個類別保留一個裝備 ID，再裝備同類別道具會替換該欄位。
`Changed` 事件供 UI 訂閱，資料成功存檔後才發布。`GetSnapshot()` 可讀取副本，不能藉此修改玩家資料。
此階段保存道具與裝備資料；服裝外觀、家具擺放及管家行為由之後的呈現元件讀取裝備 ID。

## 存檔與獎勵

- 初始金幣 0。`GameManager > Coins Per Nest` 預設 10，可在 Inspector 調整；0 可關閉獎勵。
- 每次滿窩退場、分數正式結算後發放一次固定金幣，不乘 Combo 倍率。
- 存檔格式為 `JsonUtility` JSON，存在 PlayerPrefs key `ChickenRush.PlayerData.v1`，上一份資料存在 `.backup` key。
- 成功加幣、購買及換裝都立即呼叫 `PlayerPrefs.Save()`；重新開始遊戲、返回選單與關閉後重開都保留資料。
- 場景使用同一個 `InventoryManager`，重載時重新載入存檔；不需要跨場景 Singleton。
- 無存檔時建立空資料；主存檔 JSON 損壞時嘗試備份，缺少欄位、重複 ID 與無效裝備引用會正規化。
- PlayerPrefs 是本地存檔，未提供雲端同步。

## 驗證

在隔離專案副本執行 Unity 命令列，不加 `-quit`：

1. `-executeMethod Phase4SmokeRunner.Run`：空存檔、購買／裝備、非法輸入、金幣 UI、滿窩一次性獎勵與重載。
2. 下一個 Unity 程序執行 `-executeMethod Phase4SmokeRunner.VerifySaved`：關閉後重開的持久化、四類裝備、免費道具、備份復原與資料清理。

測試只使用 `ChickenRush.Tests.Phase4` 與其備份 key，不清除正式玩家存檔。
