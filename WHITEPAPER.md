# 🐣《小雞衝衝衝 (Chicken Rush)》遊戲設計與營運白皮書
**Version:** 1.0.0 (Phase 7 Implemented; Isolated Verification)
**Engine:** Unity 6 (6000.3.23f1)  
**Architecture:** Data-Driven (ScriptableObject) + Web2.5 Invisible Economy + AI-Led Operations

---

## 1. 專案概述 (Project Overview)
* **遊戲名稱**：《小雞衝衝衝》(Chicken Rush)
* **核心定位**：基於 2D 物理引擎的超休閒 (Hyper-Casual) 堆疊與解壓遊戲，融合「迷因 (Meme) 蒐集」與「受虐型挑戰 (Sadistic Gameplay)」。
* **商業模式**：
  * **5 秒短廣告救援**：關卡失敗時跳出 Rescue UI，點擊觀看廣告可清空場上溢出小雞並 **保留當前 Combo 數**。
  * **虛擬道具生態系**：稱號、飾品衣服、家具裝潢與管家助手（金幣與羽毛雙幣制）。
* **技術架構**：Unity 6 (6000.x) + GitHub 版本控制 + AI 導向開發 (Cursor/Codex/n8n Automation)。

---

## 2. 核心 Gameplay 與物理 Juice 機制

### 2.1 基礎玩法閉環 (Core Loop)
1. **點擊生成**：玩家點擊/長按螢幕生成小雞，小雞受重力掉落。
2. **物理進窝**：小雞掉入下方雞窩，滿 10 隻觸發「滿窩滑出」並計算得分與 Combo。
3. **死區判定**：小雞掉出畫面兩側或底部 (DeathZone)，觸發遊戲暫停。
4. **廣告救援**：跳出 5 秒 Rescue UI，觀看廣告即可續局並保留當前連擊數。

### 2.2 物理 Juice (爽感回饋)
* **Q 彈物理材質 (`PhysicsMaterial2D`)**：
  * 小雞：Bounciness = 0.4, Friction = 0.2（產生相互擠壓彈動感）。
  * 雞窩：微幅摩擦力，確保小雞滑動時不產生過度穿透。
* **視聽覺打擊感**：
  * **Combo 音效**：每進一隻小雞音調 (Pitch) 遞增 +0.05，滿窩時播放清脆解壓音效。
  * **Screen Shake**：滿窩滑出瞬間觸發相機微震動。

### 2.3 怒火機制 (Chicken Anger System)
* **觸發條件**：小雞受高速撞擊（如被障礙物打飛）或卡在狹窄口超過 1.5 秒。
* **表現與效果**：
  * 視覺：臉部變為 `>_<` 表情、頭頂跳出 💢 符號、身體變紅並膨脹 1.3 倍。
  * 物理變因：`Bounciness` 瞬間飆升至 0.8~0.9，進入暴走彈珠狀態，極易把周圍小雞震出雞窩。

---

## 3. 分級動態難度系統 (Difficulty Tiers)

提供 4 種梯度難度，滿足不同客群並最大化廣告救援需求：

| 難度等級 | 雞窩運動邏輯 | 半空障礙物 (Pachinko Obstacles) | 物理與受虐機制 |
| :--- | :--- | :--- | :--- |
| **☕ 悠閒玩 (Casual)** | 靜止大開口 | 無 | 容錯率高，主打 ASMR 舒壓與堆疊音效。 |
| **🔥 地獄級 (Hell)** | 左右 Ping-Pong 勻速擺動 | 1 個慢速自轉風車 | 需預判物理落點，撞擊風車會被甩出死區。 |
| **😈 惡魔級 (Demon)** | 窄口瓶頸窩 (開口縮小 30%) | 2 個反向高速風車 + 頂部微風 | 瓶口極易產生「小雞塞車」，誘發小生氣暴走。 |
| **💀 變態級 (Sadistic)** | 變速擺動 + 傾斜關節 | 3 個不定速打蛋器 + 彈跳鋼珠 | 小雞彈性全開，一隻大胖雞掉落即可震飛全場。 |

---

## 4. 虛擬資產與 Web2.5 無感經濟 (Asset & Economy)

採用 `ScriptableObject` (`ItemDataSO`) 模組化設計，支援無縫擴充與鏈上轉化：

### 4.1 道具四大分類
1. **🏷️ 稱號 (Titles)**：顯示於主介面與排行榜（例：「悠閒玩家」、「地獄雞王」、「受虐狂雞神」）。
2. **👗 飾品衣服 (Costumes)**：草莓帽、恐龍連身衣、天使翅膀，影響小雞外觀與滿窩粒子特效。
3. **🛋️ 家具裝潢 (Furniture)**：佈置母雞主基地/小屋，品質越高提供越多離線被動收益。
4. **🤵 管家系統 (Butlers)**：
   * 離線自動收租金幣。
   * **安撫技能**：變態難度中可手動/被動釋放「安撫笛聲」，將生氣暴走小雞還原為平靜狀態。

### 4.2 Web2.5 無感鏈化與注意力轉化 (Invisible Web3)
1. **零門檻進入 (Zero-Friction Onboarding)**：
   * 採用社交登入 (Social Login) 與 AI 託管錢包，玩家全程無須接觸私鑰、助記詞與 Gas Fee。
2. **注意力實質化 (Attention Monetization)**：
   * 玩家投入的時間、5 秒廣告觀看與高難度通關行為，由 AI 自動計算貢獻度並轉化為具備可交換價值的「實質資產/羽毛」。
3. **意圖導向 P2P 轉贈 (Intent-Based Management)**：
   * 玩家無須操作複雜的交易市場，只需向「AI 管家」下達自然語言指令（如：「幫我把重複的家具贈送給好友」），由 AI Agent 全自動完成資產的打包與轉贈。

---

## 5. AI Agent 自動化營運與安全護欄 (AI Operations)

### 5.1 雙模組 AI 分工
* **🎨 趨勢與資產 Agent (Trends & Asset Agent)**：
  * 自動監控全球社群熱點 (Threads/TikTok/X)，將流行符號抽象化。
  * 自動產出 `ItemDataSO` 數值規格、提示詞與 2D/3D 資產概念。
* **📢 行銷與社群 Agent (Marketing Agent)**：
  * 配合當週熱點生成短影音腳本與社群迷因文案。
  * 以「企鵝管家」第一人稱視角在社群互動引流。

### 5.2 合規與安全護欄 (Safety & Compliance Guardrails)
採用獨立的 **Auditor Agent（合規監察官）**，所有 AI 提案必須通過以下硬性紅線 (Red Lines)：

* **🚫 絕對禁區**：政治政黨、歷史傷痛/戰爭、種族/國籍歧視、宗教敏感、性別對立、直接版權侵權。
* **💡 安全轉化原則**：去政治化、去傷痛化，僅提取「摔倒、尷尬、熱血」等無害情緒，轉化為「Q 彈笨拙小雞」等萌寵符號。

### 5.3 人類一鍵拍板介面 (Human-in-the-Loop)
* **決策形式**：AI 將整合好的「資產 Preview + 廣告文案 + 合規檢查報告」直接發送至開發者的 **Telegram / Discord 頻道**。
* **操作流程**：開發者只需點擊 `[👍 一鍵批准上架]` 或 `[👎 退回銷毀]`，AI 隨即透過 Addressables 雲端熱更新推送到遊戲，實現極低負擔的全球化營運。

---

## 6. 開發里程碑與當前進度 (Development Status)
* **Phase 1 (MVP Core Loop)**：完成核心落體、`Nest` 計數 (`6/10`)、`DeathZone` 判定與 `MvpSceneBuilder` 自動建置。
* **Phase 2 (Juice & Anger)**：導入 `PhysicsMaterial2D` 彈性、`Pachinko Obstacles` 風車障礙物與 `ChickenAnger` 撞擊變紅膨脹機制。
* **Phase 3 (Difficulty UI)**：建立 `MainMenuCanvas`，實現「悠閒、地獄、惡魔、變態」四級難度動態切換與傳參。
* **Phase 4 (Data & Save)**：完成 `ItemDataSO`、`PlayerData` (JSON/PlayerPrefs 本地存檔)、`InventoryManager` 雙幣制管理，數據已 Commit/Push 至 GitHub 倉庫。
* **Phase 5 (Shop & Inventory UI)**：**已完成**。完成四類商店分頁、道具卡片購買／裝備狀態，以及金幣／羽毛即時顯示。
* **Phase 6 (Juice 音效、粒子特效與 Camera Shake)**：**已完成**。完成 SFX／BGM 管理、Combo 遞增音高、進窩羽毛與暴走火花，以及強烈撞擊、滿窩與救援清場的相機震動。
* **Phase 7 (2D Art & Costume Overlay)**：**已完成**。導入正常／生氣／慶祝小雞貼圖、事件驅動的表情與視覺膨脹、CostumeAnchor 飾品覆蓋，以及雞窩與風車美術；保留既有碰撞體與物理材質設定。Phase 7 功能及 Phase 5／6 回歸測試於隔離套件環境通過，完整原套件組合仍待驗證。

---

## 7. 專案核心願景 (Core Vision)

* **玩家端的極致減法**：保持 100% 的 Hyper-Casual 純粹樂趣——無門檻、極速上手、Q 彈解壓、隨玩隨關。不讓任何複雜的機制（如交易、錢包、任務系統）干擾玩家體驗。
* **營運端的極致加法**：透過 AI Agent 作為幕後數位團隊，實現「24 小時時事跟風、安全護欄審查、資產無感轉化」。
* **終極目標**：打造一款**「玩起來極度簡單舒壓，幕後由 AI 驅動且具備永續迷因生命力」**的全球化爆款遊戲。
