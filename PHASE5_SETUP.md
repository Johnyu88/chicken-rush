# Phase 5 / Phase 8A：Wishing Well & Santa Event System

開啟 Assets/Scenes/SampleScene.unity，主選單按「✨ 許願池」。UI 預設關閉，沿用 runtime UI 與現有 Item Icon，沒有商品清單或四分類分頁。

在 MainMenuCanvas 的 WishingWellManager Inspector 設定 Coin Price（預設 100）、Feather Price（預設 10）及 Theme（WishingWell / Santa）。既有場景已配置；新建主選單會自動包含管理器。Santa 僅切換標題與等待文字，不啟用全球活動。

- 候選為 Resources/Items 中有效 itemId、尚未擁有的 Costume；等機率抽取，重複 ID 設定會拒絕交易。
- 結果為 Success、InsufficientCurrency、AllCollected、InvalidConfiguration。沒有有效 Costume 為設定錯誤；全部已擁有優先回報 AllCollected。失敗不扣款。
- 按鈕顯示花費；等待動畫使用不受遊戲暫停影響的時間，期間禁止重複點擊。關閉等待會取消尚未扣款的操作。
- 揭曉後可裝備；上一件／下一件逐件查看已擁有飾品。裝備中的按鈕停用，沿用 Phase 7 動態覆蓋。
- 空圖示仍能顯示道具名稱；不修改 Costume Overlay 資料結構。

## 交易與相容性

TryUnlockFromWish 使用 PlayerData 副本驗證兩種餘額、費用、類型與持有狀態，扣款及 ownedItemIds 一次 Commit，存檔成功後發布事件。保持原本 PlayerPrefs key、備份、Normalize 與裝備槽；舊存檔不清除。BuyItem 僅保留相容 API，玩家 UI 不再呼叫。ItemDataSO 的舊 price 只用於此相容 API，不再控制許願候選或裝備有效性。

## 驗證

在使用相同完整 Packages 清單的 Unity 6000.3.23f1 副本執行：

```
Unity.exe -batchmode -projectPath <副本> -executeMethod Phase5SmokeRunner.Run -logFile <log>
```

Run 會重建副本測試場景；RunDeliveredScene 可改為載入副本現有 SampleScene。測試使用獨立存檔 key，成功標記 PHASE5_WISHING_WELL_SMOKE_PASS，涵蓋雙幣原子交易、UI、重複／收集完成、存檔重讀與舊存檔 migration；金幣和羽毛使用獨立存檔，只有一個 Costume 也可驗證。

Phase6SmokeRunner.Run、Phase7SmokeRunner.Run 保留回歸驗證。完整套件解析問題已在先前版本修正；Unity SearchDatabase 啟動索引例外仍是既有編輯器問題。

## Phase 8B-1：企鵝管家

許願池底部顯示企鵝 placeholder 與對話泡泡，點擊企鵝可取得引導。MainMenuCanvas 的 Penguin Portrait 可替換 Sprite；未指定時使用幾何企鵝，不依賴 emoji 字型作為角色外觀。

台詞依歡迎、互動、等待、WishResult、WishCurrency 與 WishTheme 切換。結果播報優先於 Santa 主題歡迎，開關許願池會重置歡迎台詞。PenguinButlerAgent 不持有 InventoryManager；訊息來源 IPenguinMessageSource 僅接收不可變的呈現狀態。現在只使用本地規則式來源，不提供網路或 LLM。

Phase5SmokeRunner.Run 同時輸出 PHASE8B1_PENGUIN_SMOKE_PASS，驗證 NPC 建立／可見、互動、結果、Santa 台詞、來源替換及資產不變。
