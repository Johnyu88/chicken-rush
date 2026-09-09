# 🐣《小雞衝衝衝 (Chicken Rush)》遊戲設計與營運白皮書
**Version:** 1.8.0 (Phase 8F Furniture Editor & Mobile Interaction Foundation)
**Engine:** Unity 6 (6000.3.23f1)  
**Architecture:** Data-Driven (ScriptableObject) + Web2.5 Invisible Economy + AI-Led Operations

> **文件狀態說明**：「已完成」以第 6 章 Phase 1～8F 的交付紀錄為準；「已規劃」表示已納入設計，「未實作」表示尚無對應執行功能。本文經濟擴充、AI 營運與全球活動願景不代表已上線。Phase 8A 已將 Phase 5 玩家入口改為本地許願；Phase 8B-1 已加入規則式企鵝 NPC。全球活動、真正 LLM 與 AI 分析仍屬規劃。

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

採用 `ScriptableObject` (`ItemDataSO`) 模組化設計；未來鏈上證明是可選擴充，不是現有功能或遊戲運作前提（見第 9 章）：

### 4.1 現有四類資料與規劃用途
現有 ItemType 為 Title、Costume、Furniture、Butler；分類存在不代表下列所有用途均已實作。家園擺放、離線收益與安撫技能仍為規劃。Exterior／Decoration 的未來擴充分工見第 8.3 節。

1. **🏷️ 稱號 (Titles)**：顯示於主介面與排行榜（例：「悠閒玩家」、「地獄雞王」、「受虐狂雞神」）。
2. **👗 飾品衣服 (Costumes)**：草莓帽、恐龍連身衣、天使翅膀，影響小雞外觀與滿窩粒子特效。
3. **🛋️ 家具裝潢 (Furniture)**：佈置母雞主基地/小屋，品質越高提供越多離線被動收益。
4. **🤵 管家系統 (Butlers)**：
   * 離線自動收租金幣。
   * **安撫技能**：變態難度中可手動/被動釋放「安撫笛聲」，將生氣暴走小雞還原為平靜狀態。

### 4.2 Web2.5 無感體驗與可選資產證明（已規劃／未實作）
1. **零門檻進入 (Zero-Friction Onboarding)**：
   * 以低門檻登入與無需理解區塊鏈的遊戲體驗為方向。帳號／錢包方案尚未選定或實作；AI 不持有簽署權限，核心玩法不要求錢包或鏈上操作。
2. **注意力實質化 (Attention Monetization)**：
   * 玩家投入的時間、廣告與通關行為可作為未來分析訊號；獎勵只能由經批准的規則與 Asset Operations Service 驗證發放。AI 分析不自動創造可交換資產，不改變現有金幣／羽毛規則或承諾其可兌換價值。
3. **意圖導向 P2P 轉贈 (Intent-Based Management)**：
   * 玩家可向企鵝 AI 管家提出轉贈意圖，由服務查詢可轉贈資產並產生提案；確認資產、收件對象與條件後，才由服務驗權及執行。AI 不直接扣資產、改 ownership 或簽署交易（見第 9.4 節）。目前去重所有權不代表已有多件重複收藏或轉贈功能。


### 4.3 Wishing Well & Santa Event System（Phase 8A）

**已完成：本地許願核心與 UI 主題。** 玩家主要取得入口為「✨ 許願池」，取代傳統四分類商品購買清單。金幣預設 100／次、羽毛預設 10／次，價格由 WishingWellManager Inspector 設定，沿用 InventoryManager 雙幣制。

每次只從 Resources/Items 中具有有效 itemId、尚未擁有的 ItemType.Costume 隨機抽取；已擁有不再抽中。餘額不足、全部收集或設定無效時不扣款；空獎池或重複 ID 視為 InvalidConfiguration。交易採「Copy PlayerData → 驗證 → 扣款 → 加 OwnedItemId → Commit」，扣款與解鎖同時存檔成功才發布變更事件。BuyItem 保留相容舊測試，不再作為玩家 UI 購買入口。

玩家按下許願後看到等待訊息，再以 Fade／Scale 揭曉飾品，可按「裝備」或逐件查看已解鎖飾品。關閉等待畫面會取消尚未執行的許願；已完成交易則保留於背包。WishTheme.WishingWell 與 WishTheme.Santa 共用相同價格、抽取與存檔核心，Santa 目前只有 UI Theme。

**玩家許願 → 解鎖虛擬飾品 → 裝備率／使用率 → AI Agent 分析 → Meme 熱度 → O2O 實體周邊候選。** 本階段完成許願、解鎖與裝備；後續使用率分析、AI Agent、Meme 診斷與 O2O 仍為已規劃／未實作。沒有世界地圖、即時追蹤或全球領獎後端。

#### Phase 8B-1：企鵝 AI 管家 NPC 基礎版（已完成）

許願池顯示可點擊的企鵝管家與對話泡泡。現有管家資產僅是通用方塊，因此採可替換的幾何 placeholder；可在 MainMenuCanvas Inspector 的 Penguin Portrait 指定正式 Sprite，不下載新素材。

PenguinButlerAgent 依歡迎、點擊互動、等待、金幣／羽毛不足、成功、全部收集與設定錯誤播報固定台詞；Santa Theme 顯示準備禮物的主題提示，不代表取得即時旅程資料。NPC 只接收不可變 PenguinDialogueContext，不持有資產管理器，不扣款、不解鎖、不決定抽獎結果，交易仍由 WishingWellManager／InventoryManager 負責。

IPenguinMessageSource 保留可替換訊息來源介面，預設 RuleBasedPenguinMessages。基礎 NPC 互動已實作；真正 LLM、外部 API、Phase 8B-2、Santa Live Journey 與 Global Live Event 均為已規劃／未實作，本階段沒有網路服務。

### 4.4 Santa Live Journey & Global Wishing Event（聖誕老人全球送禮與即時許願活動）

**狀態：已規劃／未實作。** 本設計是 **Wishing Well & Santa Event System** 的擴充與統一定位；許願井、Santa 旅程、禮物開箱共用同一活動核心，不建立第二套活動經濟。

#### 玩家體驗與世界地圖

聖誕活動期間，玩家可主動進入世界地圖，查看 Santa 目前送禮區域、下一站、預計抵達時間（ETA）與倒數。活動入口不打斷原有核心遊戲流程。

**世界地圖 → AI 管家播報 → Santa 位置／下一站 → 玩家許願 → Santa 抵達玩家區域 → 禮物掉進雞窩 → 開箱 → 解鎖限定 ItemDataSO 飾品。**

玩家以活動區域設定參與，不以精確 GPS 定位為前提。時間排程以 UTC 為基準，介面轉換成玩家時區並標示區域，避免跨日或夏令時間造成誤解。禮物掉進雞窩是領獎演出；不因物理掉落失敗而遺失已取得的領獎資格。錯過抵達演出的玩家，可在活動設定的領取期限內回到入口查看待領禮物。

#### 🐧 企鵝管家：玩家可見的 AI Agent / NPC

企鵝管家正式定位為玩家看得到、可以互動的 **Penguin AI Agent / NPC**。Phase 8B-1 已完成本地規則式互動；以下全球活動職責仍為規劃，未實作：

- Santa 目前送禮區域與資料來源播報。
- 下一站、預計抵達時間與抵達提醒。
- 引導玩家進入共用許願井，確認許願與查看進度。
- 禮物抵達通知、雞窩開箱引導與飾品裝備說明。
- 活動規則說明與玩家互動。

播報依活動核心的結構化狀態生成，不由 AI 自行猜測位置、到站時間或獎勵。AI 暫不可用時，使用同一狀態的固定 NPC 台詞與操作選單；不阻擋許願、領取或開箱。AI 不直接決定扣款或發放道具。

#### Santa 位置與安全降級

| 資料模式 | 啟用條件與行為 | 玩家端標示 |
| :--- | :--- | :--- |
| 可信外部追蹤資料 | 有可信公開 Santa Tracker 資料、可合法使用且更新未過期時，透過來源介接取得區域／下一站；是否存在可用資料介面須在實作前驗證。 | 顯示來源、更新時間與來源提供的精度；不把外部活動追蹤故事包裝成真實 GPS 證明。 |
| 官方活動路線＋時區模擬 | 無即時資料、資料過期或外部介接失敗時，依遊戲官方活動路線、UTC 排程與時區推算送禮區域、下一站及 ETA。 | 明確顯示「官方活動路線模擬」及「預計抵達」；**模擬資料不得宣稱為真實 GPS 即時位置。** |
| 暫無可用排程 | 連官方活動路線也無法取得時，保留最後更新時間並顯示狀態暫不可用，不捏造位置或倒數。 | 「旅程資訊暫不可用」；依既有活動規則保留已成立的許願與待領禮物。 |

世界地圖與企鵝播報共用相同來源模式及狀態，切換模式時同步更新標籤。外部 Tracker 僅提供旅程呈現資料；領獎資格、抵達觸發與發放仍由官方活動規則判定，來源切換不得重複發獎。

#### 全球活動數據

可顯示全球累計送出禮物數、各地區收到禮物數、Santa 下一站及距離抵達時間。送出／收到統計以成功發放的禮物紀錄去重彙總，與「開箱數」分開計算；時間與倒數則來自當前旅程來源。所有數據標示更新時間，彙總延遲不得包裝成秒級即時。

模擬旅程不代表可以模擬玩家參與量：無正式統計時顯示暫無資料；展示用數值必須標記「示意資料」，不得計入正式活動成果。

### 4.5 共用 Global Live Event Framework

**狀態：已規劃／未實作。** 以 Wishing Well & Santa Event System 的許願與領獎核心擴充可重複使用框架，Santa 是第一個活動主題，而非另一套獨立系統。

| 共用部分 | 設計責任 |
| :--- | :--- |
| 活動設定 | 活動 ID、有效期間、區域、時區路線、主題素材、NPC 台詞及限定道具清單。 |
| 旅程來源與排程 | 外部資料介接、官方模擬路線、來源狀態、下一站與 ETA；統一提供給地圖與 NPC。 |
| 許願與領獎 | 共用許願紀錄、參與資格、區域抵達、待領取、已領取與已開箱狀態；以活動／玩家／獎勵識別防止重複發放。 |
| 既有道具與經濟接點 | 沿用 ItemDataSO 及 InventoryManager 的道具、持有與裝備模型；全球活動權威紀錄與現有本地存檔的同步接點另待實作。 |
| 呈現與分析 | 世界地圖、NPC 通知、禮物落窩／開箱演出，以及去重的活動漏斗事件。 |

**共用經濟原則**：保留現有金幣／羽毛雙幣制，不新增活動幣、第二個錢包或獨立經濟帳本。許願若需成本，僅能引用同一套既有貨幣與扣款規則；是否免費、成本、獎池及重複道具處理須由活動規則定義並在許願前揭露，本設計不預設新收費或兌換率。活動資格紀錄不是可交易貨幣。開箱解鎖既有 ItemDataSO 模型的限定飾品，交由同一 InventoryManager 管理持有與裝備。

未來以設定與主題資產替換為春節「財神全球／區域巡遊」、萬聖節「幽靈雞城市出沒」或其他全球季節活動，重用相同的路線、區域觸發、許願、通知、領獎與分析流程。

---

## 5. AI Agent 自動化營運與安全護欄 (AI Operations)

### 5.1 玩家端與幕後 AI Agent 分工（基礎 NPC 已實作，其餘規劃）

| Agent | 定位與責任 |
| :--- | :--- |
| 🐧 Penguin AI Agent | 已實作本地規則式企鵝管家 NPC、點擊互動及許願播報；真正 LLM 與第 4.4 節位置播報、到站提醒、全球禮物通知等仍未實作。 |
| 🎨 Trends & Asset Agent | 幕後分析許願率、開箱率、裝備率及活動參與度；結合全球社群熱點，將流行符號抽象化，提出 ItemDataSO 規格、提示詞與 2D／3D 資產概念。 |
| 📢 Marketing Agent | 依熱門活動、道具與迷因產生短影音腳本及社群內容，可延用企鵝管家第一人稱；依第 5.3 節經人類批准後發布。 |
| 🛡️ Auditor Agent | 維持第 5.2 節既有安全與合規審查，檢查活動台詞、資產及行銷提案，並檢查模擬旅程與示意統計是否如實標示。 |

企鵝提供玩家互動介面，其他 Agent 支援幕後營運；各 Agent 共用活動核心與統計定義，不各自維護一套路線、獎勵或貨幣狀態。以下自動化與人類審批流程均為規劃，並非 Phase 1～7 已完成能力。

### 5.2 合規與安全護欄 (Safety & Compliance Guardrails)
採用獨立的 **Auditor Agent（合規監察官）**，所有 AI 提案必須通過以下硬性紅線 (Red Lines)：

* **🚫 絕對禁區**：政治政黨、歷史傷痛/戰爭、種族/國籍歧視、宗教敏感、性別對立、直接版權侵權。
* **💡 安全轉化原則**：去政治化、去傷痛化，僅提取「摔倒、尷尬、熱血」等無害情緒，轉化為「Q 彈笨拙小雞」等萌寵符號。

### 5.3 人類一鍵拍板介面 (Human-in-the-Loop)
* **決策形式**：AI 將整合好的「資產 Preview + 廣告文案 + 合規檢查報告」直接發送至開發者的 **Telegram / Discord 頻道**。
* **操作流程**：開發者只需點擊 `[👍 一鍵批准上架]` 或 `[👎 退回銷毀]`，經授權的發布服務再透過規劃中的 Addressables 雲端熱更新推送到遊戲，實現極低負擔的全球化營運。


### 5.4 O2O 實體周邊與迷因受歡迎程度診斷（已規劃／未實作）

**玩家許願 → 開箱 → 裝備 → 分享／互動 → AI 分析需求 → 熱門虛擬商品 → 實體周邊候選商品。**

分析以活動 ID、道具 ID、區域與觀察期間串接去重事件；以彙總資料診斷需求，不將精確位置或私人對話作為必要分析資料。同一漏斗固定期間與資格群體，區分以下口徑：

- **活動參與度**：有效參與人數／看過活動入口的合資格人數，並觀察回訪。
- **許願率**：完成許願人數／進入許願頁的合資格人數。
- **開箱率**：已開箱禮物數／已可領取禮物數，區分發放、領取與開箱。
- **裝備率**：已裝備限定道具人數／已解鎖該道具人數，並觀察持續裝備情形。
- **分享／互動**：分開記錄遊戲內分享操作、可驗證的分享完成與可取得的社群互動；點擊分享按鈕不等於已發布。

Trends & Asset Agent 結合漏斗轉換、持續裝備、回訪與互動品質辨識受歡迎迷因，避免只按曝光量或單次開箱數判定需求。Marketing Agent 依分析提出內容，Auditor Agent 沿用既有審查，人類依第 5.3 節批准。

熱門虛擬飾品可提出帽子、玩偶等 O2O 實體周邊候選；虛擬偏好只是需求訊號，不等於實體購買承諾。候選商品仍需另行驗證意願、授權與製作可行性，經人類決策後才進入商品流程，不自動下單或生產，也不新增貨幣系統。
#### 家園資產熱門度訊號（設計擴充，未實作）

未來依同一 Item ID 及 Costume／Furniture／Decoration 類別，串接許願率、裝備率、家園擺放率、使用時間與分享率，作為 AI Agent 的 Meme／需求診斷訊號。分析以相同觀察期間、曝光及持有群體比較：

- **許願率**：與該資產／主題相關的有效許願人數占合資格曝光人數比例；目前隨機 Costume 許願不能直接視為玩家指定某件道具的需求，須區分獎池參與與特定資產偏好。
- **裝備率**：已裝備 Costume 人數／已解鎖該 Costume 人數。家具及裝飾物以擺放率判讀，不套用穿戴指標。
- **家園擺放率**：已擺放 Furniture／Decoration 的家園數／持有該道具且可使用家園的帳號數；另外觀察保留與撤下情形。
- **使用時間**：區分 Costume 有效穿戴遊玩時間、家園物件擺放期間與家園有效互動時間，不把離線擺放時間視為玩家持續注視。
- **分享率**：可驗證的分享完成者／曾使用或查看該資產的玩家；分享點擊與外部完成仍分開記錄。

AI Agent 結合以上訊號判斷哪些虛擬造型值得轉成**實體玩具、吊飾、服飾、模型或其他周邊**，產生候選與分析理由，再沿用 Auditor／人類審批。跨 2D／3D 的同一 ownership 不重複計為兩次需求；所有分析與 O2O 轉化仍為規劃，不代表已有家園遙測或實體訂單。

---

## 6. 開發里程碑與當前進度 (Development Status)
* **Phase 1 (MVP Core Loop)**：完成核心落體、`Nest` 計數 (`6/10`)、`DeathZone` 判定與 `MvpSceneBuilder` 自動建置。
* **Phase 2 (Juice & Anger)**：導入 `PhysicsMaterial2D` 彈性、`Pachinko Obstacles` 風車障礙物與 `ChickenAnger` 撞擊變紅膨脹機制。
* **Phase 3 (Difficulty UI)**：建立 `MainMenuCanvas`，實現「悠閒、地獄、惡魔、變態」四級難度動態切換與傳參。
* **Phase 4 (Data & Save)**：完成 `ItemDataSO`、`PlayerData` (JSON/PlayerPrefs 本地存檔)、`InventoryManager` 雙幣制管理，數據已 Commit/Push 至 GitHub 倉庫。
* **Phase 5 (Wishing Well & Santa Event System)**：**已完成，Phase 8A 重構**。玩家入口改為許願池，完成金幣／羽毛許願、未擁有 Costume 隨機解鎖、原子存檔、揭曉與裝備，以及 Santa UI 主題。全球旅程仍未實作。
* **Phase 6 (Juice 音效、粒子特效與 Camera Shake)**：**已完成**。完成 SFX／BGM 管理、Combo 遞增音高、進窩羽毛與暴走火花，以及強烈撞擊、滿窩與救援清場的相機震動。
* **Phase 7 (2D Art & Costume Overlay)**：**已完成**。完成 TestArt 正式資產直接綁定、OnAngerStateChanged 事件驅動的正常／生氣／慶祝表情、CostumeAnchor 動態穿戴，以及雞窩與風車 Sprite；以美術子物件補償不同 PPU，保留既有 Collider2D、Rigidbody2D 與物理材質設定。已修正 Unity 6000.3.23f1 的套件版本不相容，Phase 7 功能及 Phase 5／6 回歸測試於使用完整修正套件清單的副本通過。


### 6.1 交付與後續設計狀態

| 範圍 | 設計狀態 | 實作狀態 |
| :--- | :--- | :--- |
| Phase 1～7 | Phase 5 依 Phase 8A 更新玩家入口，其餘保留既有交付 | 已完成 |
| Phase 8A：本地 Wishing Well & Santa 共用核心／UI Theme | 見第 4.3 節 | 已完成 |
| Santa Live Journey & Global Wishing Event | 已規劃，見第 4.4～4.5 節 | 未實作 |
| Global Live Event Framework 與季節換皮 | 已規劃，見第 4.5 節 | 未實作 |
| Phase 8B-1：企鵝 NPC 基礎互動與規則式台詞 | 見第 4.3 節；Smoke Test 通過 | 已完成 |
| 真正 LLM／Phase 8B-2、幕後 AI 營運及 O2O 需求診斷 | 已規劃，見第 5 章 | 未實作 |
| Phase 8C My Nest Foundation：入口、資料模型、存檔與共用參照骨架 | 見第 8 章；Smoke Test 通過 | 已完成 |
| Phase 8E：3D My Nest Prototype、Camera、placeholder、Furniture Placement 與 persistence | 見第 8.7 節；Unity Smoke Test 通過 | 已完成（MVP） |
| Phase 8F：Furniture Editor、owned Furniture Inventory、Commands、Preview／Confirm 與輸入基礎 | 見第 8.8 節；Unity Smoke Test 通過 | 已完成 |
| 正式 3D 美術、完整家具素材庫與完整 Touch UX | 已規劃，見第 8 章 | 未實作 |
| Phase 8D Asset Ledger & AI Asset Operations Architecture | 已規劃，設計文件見第 9 章 | Server Ledger／AI Service／Adapter／Contract／Wallet／Redeem Ledger 未實作 |

Phase 8A 交付本地許願交易與 UI Theme；Phase 8B-1 已補上規則式企鵝 NPC 與訊息來源介面。Phase 8C 已完成 My Nest 入口、個人家園資料及本地存檔骨架；Phase 8D 交付第 9 章架構設計；Phase 8E 已完成本地 3D 家園原型與家具擺放存檔；Phase 8F 已升級為共用命令的家具編輯器。Santa Live Journey／Global Live Event 為已規劃／未實作，不包含世界地圖、Santa Tracker 介接、全球後端或 AI Agent 執行服務。未新增貨幣系統。

---

### 6.2 Development Checkpoint / 交接儲存點

- Phase 8F：完成；功能交付 commit：`31e7ee4f3c64843a2c16c4d4167faa6dbbcc58bf`，已確認推送至 origin/main。
- Phase 8G：尚未開始。
- 下一階段：**2D ↔ 3D Shared Costume / Appearance**。
- 手機 multi-touch：待實機驗證。
- LLM / Server Ledger / Blockchain / Santa Live Journey：未實作。

此儲存點僅記錄交接狀態，不開始任何新功能開發。

---
## 7. 專案核心願景 (Core Vision)

* **玩家端的極致減法**：保持 100% 的 Hyper-Casual 純粹樂趣——無門檻、極速上手、Q 彈解壓、隨玩隨關。不讓任何複雜的機制（如交易、錢包、任務系統）干擾玩家體驗。
* **營運端的極致加法（願景）**：由可互動的企鵝 AI 管家連接玩家與幕後 AI 數位團隊，規劃實現「24 小時時事跟風、安全護欄審查、資產無感轉化」。
* **終極目標**：打造一款**「玩起來極度簡單舒壓，幕後由 AI 驅動且具備永續迷因生命力」**的全球化爆款遊戲。
---

## 8. Personal Chicken Nest / My Nest

**個人雞窩家園系統｜Phase 8C Foundation、Phase 8E 3D Prototype 與 Phase 8F Furniture Editor 已完成**

核心定位：**每一個玩家帳號都擁有一個自己的雞窩家園。** 這是未來帳號家園設計；目前仍以既有本地 PlayerData／InventoryManager 存檔為基礎，已有本地 3D 家園 MVP，尚未實作帳號綁定家園或雲端同步。

### 8.1 兩個互補的遊戲層

| 層級 | 定位與內容 | 狀態 |
| :--- | :--- | :--- |
| **2D Gameplay Layer** | 保留目前 2D 物理玩法：掉落、雞窩、Combo、障礙、怒火、難度與救援。主要功能是「玩」與「產生資源」，延續現有金幣／羽毛經濟。 | 已完成部分功能，既有交付見第 6 章；後續資源來源與平衡另行規劃。 |
| **3D Home Nest Layer** | 玩家自己的 3D 雞窩／小屋，可旋轉查看與裝潢，擺放家具、燈具、牆面、地板及節慶裝飾。玩家擁有的小雞可在家園活動，Penguin Butler／AI Agent 作為家園 NPC，未來承接 Santa／Global Live Event。 | Phase 8E 已完成基本房間、Camera、角色 placeholder 與家具擺放；完整裝潢、小雞收藏與自主活動規則仍為規劃。 |

2D 關卡中的接雞用 Nest 與持久化的個人家園用途不同；家園不替換原本關卡、不改變現有 Collider／物理玩法。家園是可選擇進入的裝扮與互動空間，維持隨玩隨關的核心體驗。企鵝規則式 NPC 已在許願池實作，3D 家園新增企鵝 placeholder 與「歡迎回家」提示；活動整合仍未實作。

### 8.2 同一資產、多種 Representation

**同一 Item ID／ItemDataSO → 多種 Representation；玩家只擁有一次資產 ownership。** 不建立「2D 資產一套、3D 資產另一套」的重複庫存或經濟系統。

以規劃範例 `costume.santa_hat` 為例（不代表現有資產已上架）：

| Representation | 用途 |
| :--- | :--- |
| 2D Sprite Representation | 套用既有小雞 Costume Overlay。 |
| 3D Model／Mesh Representation | 在家園小雞模型上呈現同一頂帽子。 |
| Icon／Preview Representation | 許願揭曉、背包與預覽展示。 |

目前本地以 Item ID 記錄持有種類；未來 AssetInstance／ownerId 是實例所有權依據，itemId 仍指向共同定義（見第 9.2 節）。2D 穿戴、3D 顯示與圖示都是該資產的不同表現，不要求重複解鎖。家園擺放位置／旋轉等配置資料與 ownership 分開保存；配置引用同一 ID，不另發一份資產。缺少 3D Representation 時保留既有所有權及 2D 使用能力，Furniture／Butler 在家園使用可替換 placeholder；Costume 的 3D 穿戴仍待實作，不因未有模型而扣款或重新許願。

目前 ItemDataSO 保留 icon 與 Costume Sprite／Overlay，Phase 8C 新增可為空的 model3DPrefab 參照；尚無模型不影響有效性或所有權。本地家園配置存檔骨架已完成；Phase 8E 支援 Furniture 與啟用 Butler 的 model3DPrefab 呈現及家具擺放 UI，缺省使用 primitive。正式 3D 模型與 Costume 3D Overlay 仍未實作，ItemType 未擴充。

### 8.3 資產分類與擴充界線

| 分類 | 統一資產定位 | 現有資料模型／後續界線 |
| :--- | :--- | :--- |
| **Costume** | 小雞穿戴，2D／3D 共用所有權。 | 現有 ItemType；2D Overlay 已完成，3D 穿戴待實作。 |
| **Furniture** | 3D 家園家具、燈具等可擺放物件。 | 現有 ItemType；Phase 8F 支援家具庫、選擇、擺放、移動、旋轉、收回及 Preview／Confirm。 |
| **Exterior** | 雞窩外觀、建築造型。 | 未來 ItemType／資料模型擴充，目前 enum 未包含。 |
| **Decoration** | 節慶與裝飾物，可包含牆面、地板等裝飾配置。 | 未來 ItemType／資料模型擴充，目前 enum 未包含。 |
| **Butler** | AI 管家／NPC。 | 現有 ItemType；許願池規則式企鵝 NPC 與家園基本呈現已完成；基礎企鵝嚮導不授予 Butler 道具所有權，管家道具解鎖 UX 仍待規劃。 |
| **Title** | 玩家稱號。 | 現有 ItemType；沿用既有所有權模型。 |

未來加入新分類須保留現有 enum 數值、Item ID 與存檔相容性。目前 Wishing Well 僅抽取未擁有 Costume，不因本章規劃而自動開放 Furniture／Exterior／Decoration 獎池；各分類解鎖與擺放數量規則須於後續階段明確定義。

### 8.4 玩家核心循環

**玩 2D 關卡 → 獲得金幣／羽毛 → 許願 → 解鎖虛擬資產 → 穿戴小雞／裝潢 3D 雞窩 → AI 管家互動 → 節慶活動 → 再回到遊戲。**

這是完整目標循環。已完成的本地許願、2D 穿戴及規則式企鵝互動可持續使用；基本 3D 家具擺放已完成，完整裝潢、更多類別的許願與全球節慶活動仍為規劃。沿用同一金幣／羽毛與 ownership，不新增家園貨幣或第二套 Inventory。家園偏好訊號連接第 5.4 節的 O2O／Meme 分析。

### 8.5 狀態與本次交付範圍

| 功能 | 狀態 |
| :--- | :--- |
| 2D Gameplay | **已完成部分功能**；保留 Phase 1～7 的既有交付。 |
| 2D Costume Overlay | **已完成**。 |
| Wishing Well | **已完成**；目前為本地 Costume 許願。 |
| Penguin Butler Agent NPC 基礎版 | **已完成**；許願池規則式互動，非真正 LLM。 |
| Personal 3D Home Nest | **Phase 8E MVP 已完成**；本地 runtime 3D 房間。 |
| 3D Furniture Placement UI | **Phase 8F 編輯器已完成**；owned 家具庫、選取、拖曳、旋轉、收回、Undo、Cancel 與 Confirm。 |
| 2D／3D Shared Representation | 共用 itemId、可空 3D 參照及 Furniture／Butler 呈現已完成；Costume 3D 穿戴仍未實作。 |
| 正式 3D Models | **未實作**；目前僅 primitive placeholder。 |
| Phase 8C My Nest Foundation | **已完成**；入口、摘要、資料與存檔安全測試通過。 |

Phase 8C 僅交付資料骨架；Phase 8E 接續完成 runtime 3D 原型，未新增 Scene 檔案或下載美術。Santa Live Journey、Global Live Event、LLM 與雲端帳號仍未實作。

### 8.6 Phase 8C：My Nest Foundation（已完成）

主選單新增「🏡 我的雞窩」，開啟 runtime MyNestCanvas，顯示已擁有的 Costume／Furniture／Butler 數量、家園版本、有效家具紀錄及啟用管家，並可返回主選單。Phase 8C 當時只提供摘要；Phase 8E 已在同一入口加入下節的 3D 擺放操作。

每份 PlayerData 內嵌一份 HomeNestData：version、placedFurniture、activeButlerId，以及預留的 exteriorItemId／decorations。PlacedHomeItemData 僅保存 itemId、position、Euler rotation 與 scale，不序列化完整 ItemDataSO。舊存檔缺少 homeNest 時建立空家園，原貨幣、所有權與裝備槽保持相容。

InventoryManager.TryPlaceHomeFurniture 驗證現有資產為已擁有 Furniture、位置／旋轉為有限值、scale 為正數及家園版本受支援，再透過深拷貝與同一 Commit 保存。每個 itemId 目前最多一筆家具紀錄，重放只更新配置，不增加 ownership 或扣款。TrySetHomeButler 只接受已擁有的 Butler，空字串可清除；不新增管家或任何道具。

載入時保留無法解析的 ID 與未來 Exterior／Decoration 資料，以便恢復；GetActiveHomeFurniture／GetActiveHomeButler 僅回傳仍可解析且具所有權的有效配置，不讓未知或未擁有資產生效。回傳資料為深拷貝，不可藉修改快照繞過交易。未支援的家園版本不允許新增配置。Exterior／Decoration 欄位僅預留，未加入 ItemType 或操作功能。

驗證涵蓋舊存檔 migration、UI 開關、家園存檔重讀、未知 ID 恢復保留、未擁有／錯誤類型／非法 Transform 拒絕，以及同一資產 2D／3D 參照不重複所有權。

---

### 8.7 Phase 8E：3D My Nest Prototype（已完成，以下為當時交付）

主選單 →「🏡 我的雞窩」→ 3D 房間 → 選擇已擁有 Furniture → 點擊／拖曳地板移動 → 旋轉 → 儲存 → 離開 → 再進入，位置與 rotation 保持不變。沿用 HomeNestData／PlacedHomeItemData 與 InventoryManager.TryPlaceHomeFurniture，不新增存檔或 ownership 系統。

- **環境與角色**：HomeNestPrototype 建立地板、三面牆、單一無陰影方向光及獨立 RenderTexture Camera；MyNestCanvas 顯示 3D 視窗。小雞、企鵝與缺少模型的家具採輕量 primitive，非正式 3D 美術。企鵝提示「歡迎回家」，不執行 LLM 或資產交易；基本嚮導不等於玩家已擁有 Butler 道具。
- **視角與操作**：大按鈕提供左右轉動、Zoom、家具切換、45° 旋轉與儲存；視角限制為左右 55°、固定俯角 35°、距離 9～14，避免翻轉或穿過房間牆面。HomeNestPointerInput 使用 Pointer Event，支援滑鼠點擊／拖曳與滾輪，保留 Touch EventSystem 接入方式；完整手機手勢尚未完成。
- **資產呈現**：Furniture 與已啟用 Butler 優先使用 ItemDataSO.model3DPrefab，模型縮放至原型尺寸；缺少時使用可替換 placeholder。保留現有 2D Sprite／Costume Overlay，不新增第二份所有權。家園內的小雞僅展示 placeholder，不代表已完成個別小雞收藏、活動 AI 或 Costume 3D 穿戴。
- **資料安全與儲存**：選單只列出 Resources/Items 中有效、ID 唯一且已擁有的 Furniture。選取／移動僅改預覽；儲存時再次透過 InventoryManager 驗證所有權及原子 Commit。切換家具或離開會放棄未儲存預覽，不扣款、不發道具；每個 itemId 最多一筆配置。家具上限預設 24 件（Phase 8F 集中到 InventoryManager）；MVP 限定地板區域 x ±3、z ±2.5、y=0、水平 rotation 與單位 scale。未知 ID、超出此原型範圍及未支援 transform 的舊記錄保留於原存檔，載入顯示安全跳過；明確選取並儲存才更新該 ID。
- **經濟界線**：目前許願仍只解鎖 Costume。沒有 Furniture 的存檔顯示空狀態，不自動送家具或擴充獎池；測試以隔離存檔透過既有 API 建立已擁有家具 fixture。新玩家的家具獲得 UX 留待後續設計。
- **資源生命週期**：離開時停用並釋放房間、Camera、RenderTexture 與臨時材質，不更動既有 2D Scene 的物理物件；無昂貴反射、無大型素材下載。

**驗證**：於相同 Unity 6000.3.23f1 與完整套件清單的隔離副本實際編譯及執行 Phase8ESmokeRunner。測試覆蓋進出、環境與角色建立、視角限制、按鈕操作、地板投影選點、未擁有／未知 ID 拒絕、未儲存預覽丟棄、position／rotation 儲存與新 InventoryManager 重載、可替換 prefab 呈現及 ownership 不重複。Phase 8E 及 Phase 5（含 8A、8B-1）、6、7、8C 回歸測試均通過，批次程序 exit code 皆為 0。測試使用獨立 PlayerPrefs key，未寫入正常玩家存檔；已檢視 3D 畫面截圖。批次 Editor 啟動仍有既存 SearchDatabase 的 ArgumentOutOfRangeException，須與功能測試結果區分；未進行實機手機或 Player Build 驗證。

**仍未實作**：正式 3D Chicken／Penguin 美術、完整 Furniture library、完整 Touch UX、Costume 3D 穿戴、AI／LLM、Server Ownership Ledger、Blockchain／Smart Contract／Wallet、Santa Live Journey／Global Live Event、Multiplayer／Visit Friends、家具交易與 O2O 兌換。Phase 8F 的後續編輯器交付見第 8.8 節；本節保留 Phase 8E 當時紀錄。

---

### 8.8 Phase 8F：3D Furniture Editor & Mobile Interaction Foundation（已完成）

**Furniture Inventory 與編輯器**：My Nest 加入分頁家具庫，只顯示既有 Inventory 真正擁有的 Furniture 名稱及 Item Icon（無圖示時使用中性 placeholder）。卡片辨識「可擺放／預覽中／已擺放／待收回」，不新增 ownership 或家具獎池。點卡片或點 3D 家具可選取；支援擺放預覽、拖曳移動、旋轉、收回、Cancel、Confirm ✓ 與一次 Undo。綠色底座標示目前選取家具；現有 2D Gameplay／Costume 不變。

**Preview → Confirm transaction**：FurnitureEditSession 持有 HomeNestData 的深拷貝與命令清單。選取新家具、移動、旋轉、收回和企鵝提案都只改預覽；切換家具保留同一 session 的整體方案，Cancel／離開丟棄所有未確認變更。只有玩家 Confirm 才呼叫 InventoryManager 的命令提交入口：比對預覽起始 Home State → 重新驗證全部命令 → Copy PlayerData → 一次 Commit，避免部分方案寫入。若家園已被其他操作改動，拒絕過期 Confirm；貨幣等無關更新不被覆蓋。原 Phase 8C TryPlaceHomeFurniture 保留舊程式／測試相容性，不作為玩家或 AI 編輯器入口。

**Placement Command architecture**：可序列化 PlacementCommand 定義 PlaceFurniture、MoveFurniture、RotateFurniture、RemoveFurniture，以 itemId、position 與 yaw 表示操作。PlacementRules 同時用於玩家預覽、AI 提案與 Confirm。統一驗證有效且唯一的 catalog ID、Furniture 類型與 ownership、合法命令、有限數值、家園版本及家具數量上限；未知 ID／未擁有道具均拒絕。位置 clamp 至 x ±3、z ±2.5、y=0，rotation 限水平、scale 為單位大小。上限由 InventoryManager 的 homeFurnitureLimit 管理，預設 24 件，命令不能自行指定較寬鬆規則；每個 itemId 仍只有一筆配置。保留存檔中無法解析的 ID，不讓它們佔用畫面的有效家具名額。

每批命令先在副本完整驗證，任一失敗都不改預覽或永久存檔；傳入資料及讀取快照均複製。一次 Undo 回復目前未確認 session 的上一個命令／提案批次；連續同家具 Move 合併，避免長拖曳累積超過 256 條命令上限，Undo 可退回該段移動前。Confirm／Cancel 清除 Undo，未提供永久存檔的歷史回復。

**Penguin AI decoration preview interface**：企鵝入口顯示「🐧 歡迎回家！要我幫忙整理雞窩嗎？」；「企鵝方案」產生本地規則示範，將一件已擁有家具移至建議位置。這不是 LLM。未來流程為 Player Request → Penguin AI → Placement Commands → Validation → Preview → Player Confirm → HomeNestData。未來方案來源只取得 IHomeDecorationPreview（無 Confirm／Commit 方法）；玩家 UI 持有確認操作。AI 沒有特殊驗權模式，也不能繞過 InventoryManager 創造或扣除資產。

**Mouse／Touch input foundation**：HomeNestPointerInput 以 EventSystem pointer ID 與視窗標準化座標處理輸入。點家具選取、單指拖家具移動、拖空白區 Orbit、雙指距離變化 Pinch Zoom；保留滑鼠拖曳、滾輪及視角／旋轉／Confirm／Cancel 按鈕。進入雙指操作後抑制家具拖曳，直到所有手指放開，避免 Pinch 誤移家具；關閉 UI 清除手勢狀態。Camera 沿用 Phase 8E 的距離、俯角與左右轉動限制，沒有接網路或第三方 Touch 套件。

**驗證**：Phase8FSmokeRunner 使用隔離 PlayerPrefs key 及臨時 Furniture fixture，驗證 owned-only 家具庫、選取／預覽／移動／旋轉／收回、Cancel 不寫入、Confirm 單筆提交、Undo、Save／Reload、unknown ID 保留、ownership 拒絕、bounds、NaN／Infinity、數量上限、整批命令拒絕、AI 不自行 Commit、過期 session 拒絕，以及 300 次連續拖曳更新。已執行合成 pointer ID 的單指移動、空白拖曳及雙指 Pinch 測試，並檢視 Unity 畫面截圖。Phase 8E 的入口測試改為點擊家具庫卡片，以符合更新後 UI；其既有存檔、安全與 3D 功能回歸仍保留。已於 Unity 6000.3.23f1 完整套件隔離副本編譯並執行 Phase 8F、8E、5（含 8A／8B-1）、6、7、8C，全部 exit code 0；Editor 仍有既存 SearchDatabase 啟動例外，不屬於上述功能測試通過的宣稱。未做手機 Player Build 或實機測試。

**需要實機驗證／仍未完成**：完整手機 Touch UX 與實機 multi-touch（含不同 DPI、手勢取消與系統中斷）、正式 3D 美術及完整家具素材庫、真正 LLM、Server Ledger、Blockchain／Wallet、Multiplayer／好友參觀、Santa Live Journey、家具交易。合成手勢通過不代表已完成手機實機測試；不開始 Phase 8G。

---
## 9. Web2.5 Asset Ledger & AI Asset Operations Architecture

**Phase 8D｜Architecture：已規劃。本次交付架構設計，不代表服務已上線。** 目標是在 100 萬以上玩家、每人數十至數百資產的規模下，安全管理 Costume、Furniture、Decoration、Butler、Exterior 及個人家園。保留現有金幣／羽毛經濟與本地系統；不選定任何鏈、不部署 Smart Contract、不建立錢包。

### 9.1 三層資料架構

| 資料層 | 回答的問題 | 規劃資料與邊界 |
| :--- | :--- | :--- |
| **A. Asset Registry** | 這是什麼資產？ | 以 itemId 定義 ItemDefinition；包含 asset type、rarity、2D representation、3D representation、Icon／Preview、transferable、blockchainEligible、O2O redeemable 等 metadata。定義及政策可版本化，不包含玩家持有清單。 |
| **B. Ownership Ledger** | 誰擁有什麼？ | 每個 AssetInstance 對應唯一 assetInstanceId、itemId、ownerId，預留 acquiredAt、source、transferState、chainProof 與版本。是未來遊戲服務唯一的 ownership truth；2D／3D 不各存一份所有權。 |
| **C. Runtime / Home State** | 資產現在怎麼被使用？ | 記錄 Costume 穿戴對象、Furniture 所在家園及 position／rotation／scale、active Butler、Exterior、Decoration。配置引用資產，不能反向創造所有權；高頻資料原則上鏈下保存。 |

Registry 的政策旗標是可用條件，不是發放證據：blockchainEligible 不等於已鑄造，redeemable 不等於已兌換，transferable 也不等於玩家已同意轉移。未知定義或缺少 Representation 不應讓整份玩家資料失效，保留待恢復參照並禁止不合法使用。

### 9.2 ItemDefinition、AssetInstance、Representation、RuntimeState

| 概念 | 聖誕帽範例 | 不可混淆的責任 |
| :--- | :--- | :--- |
| **ItemDefinition** | costume.santa_hat：聖誕帽這種東西是什麼。 | 種類與規則，沿用 ItemDataSO／itemId 的定義角色。 |
| **AssetInstance** | instance-001：某玩家實際擁有的那一件。 | 實例識別、ownerId、取得來源、轉移／兌換狀態；不是 Sprite 或模型。 |
| **Representation** | 同一帽子的 Sprite、Mesh、Icon。 | 不因新增 3D 外觀而產生第二件資產或第二份 ownership。 |
| **RuntimeState** | instance-001 現在戴在哪隻雞上。 | 可變使用配置；家具則記錄擺在哪個家園及 Transform。 |

以上四個概念分開建模，對應第 9.1 節三個資料層：Definition／Representation 屬 Registry，Instance 屬 Ledger，使用配置屬 Runtime／Home State。未來若允許同款多件，各件具有不同 assetInstanceId；是否允許多件仍由產品規則決定，本階段不改變許願去重及每 itemId 一份本地 ownership 的行為。

未來每份個人家園以 account/player ID 對應 Home State；家園配置不是可轉贈商品本身。若日後某種家園外觀或兌換權被定義為資產，仍透過 Registry／Ledger 表達，不因存在一份 Home State 自動鑄造資產。

### 9.3 Hybrid Asset Ledger：Blockchain ≠ 遊戲資料庫

**Blockchain ≠ 遊戲資料庫。** 移動家具、旋轉家具、換帽子、NPC 移動與普通遊戲操作保存於鏈下 Runtime／Home State，不逐次上鏈，也不要求每次遊玩等待鏈上結算。

只有需要可驗證所有權、稀缺性或轉移證明的高價值事件，才評估使用區塊鏈：限定稀有資產、玩家間可轉贈資產、特殊活動收藏品、O2O 實體兌換權，以及未來需公開驗證的 ownership proof。這些都是候選用途，不表示必須上鏈。

Server Ownership Ledger 是遊戲查詢與授權的單一真相；本地快取及 chainProof 皆非可各自改寫所有權的第二套帳本。chainProof 僅連結已核驗的外部證明，包含必要的交易／證明識別、驗證狀態及時間。若未來接受外部鏈上轉移，須由受控對帳流程驗證並更新同一 Ledger，不能信任客戶端自報持有人。

需鏈上證明的操作可採 proposed → reserved／pending → confirmed 或 failed 狀態。服務先驗權、保留資產並寫入待處理事件，再非同步提交；pending 期間禁止同一實例再次轉移或兌換。確認後才完成相應權利變動，失敗以可審計的補償／解除保留處理，不先在本地複製另一份資產。重複回呼、超時或證明衝突須去重與重新查證；遇到可能的結算回退，暫停該實例的高價值操作並對帳，不以快取覆蓋權威狀態。

### 9.4 AI Asset Operations Layer

**Penguin AI Agent → Intent / Query → Asset Operations Service → Permission / Rule Validation → Ownership Ledger / Home State → 必要時 Blockchain Adapter。**

AI 可查詢玩家資產、找長期未使用家具、推薦雞窩配置、辨識重複／相似收藏、提出轉贈建議、整理節慶裝飾、解釋取得來源及分析熱門 Costume／Furniture。現有去重資產庫只能辨識相似或不同收藏；真正重複實例需未來產品規則支援。

AI 只產生結構化 Intent 或執行受限 Query，不能直接存取資料庫憑證或簽署金鑰。Asset Operations Service 根據已驗證登入身分檢查 ownerId、權限、定義政策、當前持有狀態、轉移／兌換鎖定與預期版本；AI 輸入的 playerId 不具授權效力。服務拒絕越權、失效版本及不合法數量，不能因模型聲稱玩家同意而放行。

建議與執行分離：先提供可檢視的配置或轉贈提案，再由玩家授權相應修改。高價值轉移必須明確確認實例、對象與條件，確認綁定操作內容且有期限；執行前再次驗證。AI **不得繞過權限、直接創造 ownership、直接扣資產或直接簽署鏈上交易**。任何未來簽署能力位於獨立受控流程，現階段不選定錢包或金鑰管理方案。

### 9.5 百萬玩家規模與有限查詢

容量規劃以「100 萬以上玩家 × 每人數十至數百資產」為目標，資料庫實作與供應商尚未選定。設計要求：

- **account/player ID 與 asset instance ID**：穩定、不依賴顯示名稱；Ledger 對 assetInstanceId 唯一約束，每件資產只有一個有效持有人。
- **Indexed ownership lookup**：以 ownerId 加狀態／分類等索引支援篩選；Registry 可共用快取，Ownership 查詢依帳號隔離，避免掃描所有玩家庫存。需要時按 player ID 分區；跨玩家轉移仍須受控協調與原子提交。
- **Event／audit log**：每次 ownership 變更記錄 actor、operation ID、來源、對象、前後版本、原因與時間；歷史不可由 AI 任意刪改。發放、移轉、保留、補償與兌換皆可追溯。
- **Idempotent transaction**：操作綁定唯一 key 與請求內容；同 key 同內容回傳既有結果，不重扣／重發；同 key 不同內容拒絕。狀態變更與 audit／待送出事件在同一資料庫交易保存，避免資料成功但訊息遺失。
- **Optimistic concurrency／versioning**：更新 Ledger 實例或 Home State 時帶 expectedVersion，比對成功才提交；衝突重新讀取並確認意圖，不讓過期配置覆蓋新狀態。驗權與寫入不可分離成可被競態繞過的步驟。
- **Cache 與 pagination**：按版本失效或更新快取，權限及交易決策回查權威狀態；游標分頁、固定回傳上限、查詢配額。分析使用彙總／讀取模型，避免阻塞玩家交易。
- **Asynchronous blockchain settlement**：可靠佇列／outbox、去重消費、重試與對帳；回傳 operation ID 和 pending 狀態，而非長時間阻塞一般遊戲操作。轉移完成後，使舊持有人的裝備／家園引用失效或停用，Runtime 仍須檢查有效所有權。

AI 不載入玩家完整資產庫，只透過授權工具取得當前任務所需欄位，例如下列**未實作的查詢契約**：

| 查詢 | 最小回傳範圍 |
| :--- | :--- |
| GetOwnedAssets(playerId, filters, cursor) | 符合條件的有限筆資產摘要、版本與下一頁 cursor。 |
| GetHomeState(playerId) | 家園版本與所需配置摘要；大型布局依區域或選定物件分頁取得，避免完整塞入 context。 |
| GetAssetHistory(assetInstanceId) | 有權查看的來源／異動摘要；長歷史以後續游標或限定期間取得。 |

查詢工具在服務端驗權，對結果刪除不必要欄位；推薦長期未用家具應由服務先篩選候選，不把整個帳號或其他玩家資料交給模型。

### 9.6 可替換的 Blockchain Adapter

**IBlockchainAssetAdapter：僅設計名稱與邊界，未實作介面或服務。** 未來隔離「提交符合條件的證明／轉移操作、查詢結算狀態、核驗證明」等能力，讓不同鏈可替換。Adapter 不決定遊戲獎勵或授權；它只接受已通過服務驗證的操作，回傳結算狀態／證明，不直接繞過 Ledger 更新玩家資產。

本階段不選定 Polygon／Base／Solana 或其他鏈，不部署 Smart Contract、不建立玩家錢包。**沒有區塊鏈時，Chicken Rush 核心遊戲仍能完整運作。** Adapter 未配置或不可用時，本地玩法、許願、穿戴與家園基礎功能照常運作；需外部證明的可選功能顯示不可用／待處理，不偽造成功或阻擋普通遊戲。

### 9.7 O2O Redeemable Asset 與防重複兌換

**熱門虛擬商品 → AI 分析 → 人類批准 → O2O 商品 → Redeemable Asset → 玩家兌換 → Audit Log。**

實體周邊兌換權可定義為特殊 AssetInstance，連結商品與適用條件；它不是第二種貨幣，也不表示所有普通飾品都可兌換。若由收藏品衍生權利，規則須明確界定唯一權利來源，不讓原件與衍生權同時重複履約。

O2O Redeem Ledger 與同一 Ownership Ledger 關聯：驗證 owner、權利有效性及未使用狀態，以 assetInstanceId 的唯一兌換約束與 idempotency key 讓 available → reserved → redeemed 的每次狀態轉移在單筆交易記錄並保存 audit。重試回傳原兌換／履約識別，不能再次出貨；reserved／redeemed 權利不可轉贈。失敗取消須先確認尚未履約，再記錄補償，不能因網路超時直接恢復可用。

實際出貨可非同步，使用同一履約識別防重複；兌換與出貨狀態分開保存。地址等履約資料限授權服務使用並鏈下保存，不交給不需要它的 AI，也不公開上鏈。鏈上證明為可選，防重複兌換的服務端規則不能依賴 AI 或單次客戶端提示。

### 9.8 隱私、安全與現有系統相容性

AI 採最小資料存取；私人玩家資料不公開上鏈，鏈上內容避免直接含姓名、email、地址或內部帳號識別。公開證明使用必要的非直接識別資訊，帳號對應鏈下受控保存；不將去識別化等同無法被關聯。所有 ownership 變動必須有 audit trail，高價值轉移需明確玩家確認，AI 建議與實際交易執行保持分離。

Phase 8C 的 InventoryManager、HomeNestData、PlacedHomeItemData、ItemDataSO 保持可用，Phase 8D 設計更新未更動 Runtime、存檔 schema、許願費用或獎池；Phase 8E 仍沿用此資料模型。未來 Server Ledger 導入須使用版本化遷移、備份與可重試的映射，為經驗證的本地持有紀錄建立實例，並保存 local itemId 到 assetInstanceId 的對應；不能把客戶端 PlayerPrefs 宣告直接當作高價值所有權證明。

切換至 Server Ledger 後，本地 Inventory 是服務端資料的快取／離線使用投影，不能與服務端並列為兩個可獨立發放的真相。離線取得／同步策略須在該階段另行定義並防重複匯入；目前仍維持既有本地存檔模式，不提前建立帳號或實例遷移程式。

### 9.9 Phase 8D 狀態

| 範圍 | 狀態 |
| :--- | :--- |
| Web2.5 Asset Ledger & AI Asset Operations Architecture | **已規劃**；設計文件交付。 |
| Local Inventory／HomeNest foundation | **已完成**；沿用 Phase 8C。 |
| Server Ownership Ledger | **未實作**。 |
| AI Asset Operations Service | **未實作**。 |
| Blockchain Adapter | **未實作**。 |
| Smart Contract | **未實作**。 |
| Wallet | **未實作**。 |
| O2O Redeem Ledger | **未實作**。 |

Phase 8D 僅修改 WHITEPAPER.md。Phase 8E 的本地 3D 原型見第 8.7 節；本章 Server／AI／Blockchain 架構與 LLM、Santa Live Journey／Global Live Event 並未因此實作。未改變遊戲經濟，未開始任何區塊鏈實作。
