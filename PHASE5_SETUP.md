# Phase 5：商店與裝備 UI

開啟 `Assets/Scenes/SampleScene.unity` 並進入 Play Mode，主選單按「🛒 商店」開啟商店，按「關閉」返回。既有場景可直接使用；`Chicken Rush > Build MVP Test Scene` 也會建立卡片 Prefab。

- 四個分頁：稱號、飾品、家具、管家。「飾品」對應 Phase 4 的 `ItemType.Costume`，不更動既有存檔類別。
- `ShopCanvas` 由主選單初始化，使用可垂直捲動的雙欄 `ItemGridContainer`。
- 卡片來源：`Assets/Resources/UI/ItemCardPrefab.prefab`。可使用 `Chicken Rush > Build Shop Item Card Prefab` 建立；已有 Prefab 不覆寫。若資產缺失，會建立執行期備援模板。
- 道具來自 `Resources/Items`，依 `itemId` 排序；忽略無效資料及同分類重複 ID。每個道具仍須有全域唯一 ID。
- 未擁有顯示「購買」，成功扣款後顯示「裝備」；購買不自動裝備。餘額不足顯示「金幣不足」。
- 已擁有可裝備；裝備中的按鈕顯示「已裝備」並停用。同類換裝後舊卡片立即恢復「裝備」。
- 商店同時提供已持有道具的裝備操作，沒有另外新增獨立背包頁。

## 貨幣與存檔

`InventoryManager.OnCurrencyChanged` 是無參數事件，訂閱者讀取 `Coins` / `Feathers`。只在成功儲存且貨幣數值變更時觸發；裝備、被拒絕交易及免費購買不觸發。原有 `Changed` 保留，供卡片同步所有背包變動。

主選單及商店頂部都顯示金幣與羽毛，啟用時訂閱並立即刷新，停用時解除訂閱。`AddFeathers(int)` 與 `AddCoins(int)` 一樣拒絕非正數及溢位。羽毛預設 0，沿用原存檔 key；舊 JSON 未包含羽毛欄位時自然補 0。道具價格仍使用金幣，未新增羽毛取得或消費玩法。

## 驗證方式

在隔離 Unity 專案副本執行（不加 `-quit`）：

```
Unity.exe -batchmode -nographics -projectPath <副本路徑> -executeMethod Phase5SmokeRunner.Run -logFile <記錄路徑>
```

成功標記為 `PHASE5_SMOKE_PASS`。測試使用獨立 `ChickenRush.Tests.Phase5` 存檔，涵蓋主選單入口、四類篩選、金幣不足、購買與裝備狀態、重複購買、同類替換、貨幣事件、關閉重開、羽毛存檔及舊資料相容。

環境注意：目前 `ProjectVersion.txt` 記錄 6000.3.23f1，但既有套件包含 6000.6 的內建模組（例如 physicscore2d）。本次功能測試使用 Unity 6000.3.23f1 的隔離副本，將副本套件清單限縮為該版本內建模組與 uGUI 2.0.0，以排除不相容套件。6000.6 副本停在 Burst 編譯。未修改原專案的版本或套件清單；完整套件組合未完成驗證。

