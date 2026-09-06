# DeathZone 與 5 秒 Mock 救援

## 場景設定

1. 延續 NEST_SETUP.md 的 GameManager、小雞、雞窩與 Spawner 設定。
2. 在場景下方及左右各建一個空物件，掛 DeathZone 與 BoxCollider2D，勾 Is Trigger。範圍放在畫面外，三區接合不要留縫；側邊死區頂端避開上方小雞生成帶。使用足夠厚度避免高速穿越，Physics 2D Layer Collision Matrix 要允許小雞與死區互動。
3. 小雞 Rigidbody2D 使用 Dynamic，建議 Continuous 碰撞偵測；腳本的 Despawn Below Y 現在是「漏接失敗」保底界線，不再直接銷毀。其值應位於畫面底部外。
4. 新建場景物件 RescueUI，掛 RescueMockUI，將場景 GameManager 拖入 Game Manager 欄位。這是 OnGUI Mock，不需要 Canvas、Button 物件、EventSystem 或手動 OnClick 接線。可指定支援繁體中文的 Font。
5. 儲存場景，加入 Build Settings／Unity 6 Build Profiles 的場景清單並啟用。GameManager 與 RescueUI 都保持啟用，且不要 DontDestroyOnLoad。「放棄重來」會重新載入目前場景。

## 行為

- Falling 或 KnockedAway 小雞接觸死區：GameOver、timeScale=0，發布 On Failure，顯示救援／重來按鈕。Nested 小雞不觸發失敗；同幀多次死亡只觸發一次。
- 撞飛不再立即斷 Combo／定時刪除，真正越界才失敗；失敗不重設 Combo。
- 按「看5 秒廣告救援（母雞掃場）」：顯示 Mock 遮罩，以 unscaledDeltaTime 倒數 5 秒。期間不能重複救援或重來，沒有真實廣告 SDK、連網或收益驗證。
- 倒數結束停用並銷毀所有由目前管理器登記的未入窩小雞（含畫面外溢出小雞），等待一幀銷毀完成，再恢復 Playing 與 timeScale=1。保留分數、Combo、已收納小雞及滿窩待入帳獎勵。
- 母雞掃場目前是清場效果，不包含母雞圖像／動畫。On Rescue Completed 可接後續特效。實際遊戲若另有未經 Initialize 生成的小雞，需確保它們也登記到同一個 GameManager。
- 每局一次救援，第二次失敗救援按鈕停用，只能放棄重來。重來重新載入場景，因此 Score=0、Combo=0，第一窩、生成器及救援額度重置。
- 觸控恢復後需放開再按下；滑鼠若仍按住則重新開始生成。
- 若換窩時失敗，退場 Coroutine 會保持在原進度，救援後接續；此時漏接同樣會觸發失敗。

## Play Mode 驗收（需在 Unity 實際執行）

1. 先成功填兩窩確認 Combo=2，再讓一隻小雞進入各個 DeathZone。確認 timeScale=0、Combo 仍為 2、救援彈窗只出現一次。
2. 按救援：遮罩顯示 5→4→3→2→1，物理／生成／雞窩退場保持停止，約 5 秒加一幀後恢復。
3. 確認 Falling／KnockedAway 小雞全清，Nested 小雞與雞窩計數保留、Score 及 Combo 不變，沒有下一幀立即再次失敗。
4. 再次失敗確認不能再救援；按放棄重來，Score／Combo 歸零，場景只生成第一個新窩。
5. 測試同幀多隻死亡、底部保底界線、救援連點，以及換窩中死亡再救援。

既有 Tests/Test-NestScoreState.ps1 與 Tests/Test-HoldSpawnInput.ps1 是規則回歸測試，不涵蓋本次 Unity Coroutine、觸控 UI、物理或場景重載。此資料夾尚未包含完整 Unity 專案與可執行測試場景。
