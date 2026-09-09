# 🐣《小雞衝衝衝 (Chicken Rush)》遊戲設計與營運白皮書
**Version:** 1.5.0 (Phase 8C My Nest Foundation)
**Engine:** Unity 6 (6000.3.23f1)  
**Architecture:** Data-Driven (ScriptableObject) + Web2.5 Invisible Economy + AI-Led Operations

> **文件狀態說明**：「已完成」以第 6 章 Phase 1～8C 的交付紀錄為準；「已規劃」表示已納入設計，「未實作」表示尚無對應執行功能。本文經濟擴充、AI 營運與全球活動願景不代表已上線。Phase 8A 已將 Phase 5 玩家入口改為本地許願；Phase 8B-1 已加入規則式企鵝 NPC。全球活動、真正 LLM 與 AI 分析仍屬規劃。

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

### 4.1 現有四類資料與規劃用途
現有 ItemType 為 Title、Costume、Furniture、Butler；分類存在不代表下列所有用途均已實作。家園擺放、離線收益與安撫技能仍為規劃。Exterior／Decoration 的未來擴充分工見第 8.3 節。

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
* **操作流程**：開發者只需點擊 `[👍 一鍵批准上架]` 或 `[👎 退回銷毀]`，AI 隨即透過 Addressables 雲端熱更新推送到遊戲，實現極低負擔的全球化營運。


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
| 真正 3D Home Nest、3D Furniture Placement UI、3D Models | 已規劃，見第 8 章 | 未實作 |

Phase 8A 交付本地許願交易與 UI Theme；Phase 8B-1 已補上規則式企鵝 NPC 與訊息來源介面。本次 Phase 8C 完成 My Nest 入口、個人家園資料及本地存檔骨架。Santa Live Journey／Global Live Event 為已規劃／未實作，不包含世界地圖、Santa Tracker 介接、全球後端或 AI Agent 執行服務。未新增貨幣系統。

---

## 7. 專案核心願景 (Core Vision)

* **玩家端的極致減法**：保持 100% 的 Hyper-Casual 純粹樂趣——無門檻、極速上手、Q 彈解壓、隨玩隨關。不讓任何複雜的機制（如交易、錢包、任務系統）干擾玩家體驗。
* **營運端的極致加法（願景）**：由可互動的企鵝 AI 管家連接玩家與幕後 AI 數位團隊，規劃實現「24 小時時事跟風、安全護欄審查、資產無感轉化」。
* **終極目標**：打造一款**「玩起來極度簡單舒壓，幕後由 AI 驅動且具備永續迷因生命力」**的全球化爆款遊戲。
---

## 8. Personal Chicken Nest / My Nest

**個人雞窩家園系統｜Phase 8C Foundation 已完成；真正 3D 家園已規劃／未實作**

核心定位：**每一個玩家帳號都擁有一個自己的雞窩家園。** 這是未來帳號家園設計；目前仍以既有本地 PlayerData／InventoryManager 存檔為基礎，尚未實作帳號綁定家園、雲端同步或 3D 家園。

### 8.1 兩個互補的遊戲層

| 層級 | 定位與內容 | 狀態 |
| :--- | :--- | :--- |
| **2D Gameplay Layer** | 保留目前 2D 物理玩法：掉落、雞窩、Combo、障礙、怒火、難度與救援。主要功能是「玩」與「產生資源」，延續現有金幣／羽毛經濟。 | 已完成部分功能，既有交付見第 6 章；後續資源來源與平衡另行規劃。 |
| **3D Home Nest Layer** | 玩家自己的 3D 雞窩／小屋，可旋轉查看與裝潢，擺放家具、燈具、牆面、地板及節慶裝飾。玩家擁有的小雞可在家園活動，Penguin Butler／AI Agent 作為家園 NPC，未來承接 Santa／Global Live Event。 | 已規劃／未實作；家園小雞的持有、生成與活動規則尚非目前關卡小雞的既有能力。 |

2D 關卡中的接雞用 Nest 與持久化的個人家園用途不同；家園不替換原本關卡、不改變現有 Collider／物理玩法。家園是可選擇進入的裝扮與互動空間，維持隨玩隨關的核心體驗。企鵝規則式 NPC 已在許願池實作，但 3D 家園中的 NPC 呈現與活動整合仍待實作。

### 8.2 同一資產、多種 Representation

**同一 Item ID／ItemDataSO → 多種 Representation；玩家只擁有一次資產 ownership。** 不建立「2D 資產一套、3D 資產另一套」的重複庫存或經濟系統。

以規劃範例 `costume.santa_hat` 為例（不代表現有資產已上架）：

| Representation | 用途 |
| :--- | :--- |
| 2D Sprite Representation | 套用既有小雞 Costume Overlay。 |
| 3D Model／Mesh Representation | 在家園小雞模型上呈現同一頂帽子。 |
| Icon／Preview Representation | 許願揭曉、背包與預覽展示。 |

共同 Item ID 是所有權依據，2D 穿戴、3D 顯示與圖示都是該資產的不同表現，不要求重複解鎖。家園擺放位置／旋轉等配置資料與 ownership 分開保存；配置引用同一 ID，不另發一份資產。缺少 3D Representation 時保留既有所有權及 2D 使用能力，在家園標示尚不支援，不因未有模型而扣款或重新許願。

目前 ItemDataSO 保留 icon 與 Costume Sprite／Overlay，Phase 8C 新增可為空的 model3DPrefab 參照；尚無模型不影響有效性或所有權。本地家園配置存檔骨架已完成；真正 3D 模型、呈現及擺放 UI 仍未實作，ItemType 未擴充。

### 8.3 資產分類與擴充界線

| 分類 | 統一資產定位 | 現有資料模型／後續界線 |
| :--- | :--- | :--- |
| **Costume** | 小雞穿戴，2D／3D 共用所有權。 | 現有 ItemType；2D Overlay 已完成，3D 穿戴待實作。 |
| **Furniture** | 3D 家園家具、燈具等可擺放物件。 | 現有 ItemType；3D 擺放與使用功能待實作。 |
| **Exterior** | 雞窩外觀、建築造型。 | 未來 ItemType／資料模型擴充，目前 enum 未包含。 |
| **Decoration** | 節慶與裝飾物，可包含牆面、地板等裝飾配置。 | 未來 ItemType／資料模型擴充，目前 enum 未包含。 |
| **Butler** | AI 管家／NPC。 | 現有 ItemType；許願池規則式企鵝 NPC 已完成，不代表已實作管家道具解鎖或家園 NPC。 |
| **Title** | 玩家稱號。 | 現有 ItemType；沿用既有所有權模型。 |

未來加入新分類須保留現有 enum 數值、Item ID 與存檔相容性。目前 Wishing Well 僅抽取未擁有 Costume，不因本章規劃而自動開放 Furniture／Exterior／Decoration 獎池；各分類解鎖與擺放數量規則須於後續階段明確定義。

### 8.4 玩家核心循環

**玩 2D 關卡 → 獲得金幣／羽毛 → 許願 → 解鎖虛擬資產 → 穿戴小雞／裝潢 3D 雞窩 → AI 管家互動 → 節慶活動 → 再回到遊戲。**

這是完整目標循環。已完成的本地許願、2D 穿戴及規則式企鵝互動可持續使用；3D 裝潢、更多類別的許願與全球節慶活動仍為規劃。沿用同一金幣／羽毛與 ownership，不新增家園貨幣或第二套 Inventory。家園偏好訊號連接第 5.4 節的 O2O／Meme 分析。

### 8.5 狀態與本次交付範圍

| 功能 | 狀態 |
| :--- | :--- |
| 2D Gameplay | **已完成部分功能**；保留 Phase 1～7 的既有交付。 |
| 2D Costume Overlay | **已完成**。 |
| Wishing Well | **已完成**；目前為本地 Costume 許願。 |
| Penguin Butler Agent NPC 基礎版 | **已完成**；許願池規則式互動，非真正 LLM。 |
| Personal 3D Home Nest | **已規劃／未實作**。 |
| 3D Furniture Placement UI | **已規劃／未實作**；僅資料驗證／存檔 API 已完成。 |
| 2D／3D Shared Representation | 共用 itemId 與可空 3D 參照骨架已完成；3D 呈現 **已規劃／未實作**。 |
| 3D Models | **未實作**。 |
| Phase 8C My Nest Foundation | **已完成**；入口、摘要、資料與存檔安全測試通過。 |

本次 Phase 8C 未新增 Scene、3D 模型、家具拖曳或素材。真正 3D Home Nest、3D Furniture Placement UI、Santa Live Journey、Global Live Event、LLM 與雲端帳號仍未實作。

### 8.6 Phase 8C：My Nest Foundation（已完成）

主選單新增「🏡 我的雞窩」，開啟 runtime MyNestCanvas，顯示已擁有的 Costume／Furniture／Butler 數量、家園版本、有效家具紀錄及啟用管家，並可返回主選單。此畫面只有資產摘要，沒有 3D 擺放操作。

每份 PlayerData 內嵌一份 HomeNestData：version、placedFurniture、activeButlerId，以及預留的 exteriorItemId／decorations。PlacedHomeItemData 僅保存 itemId、position、Euler rotation 與 scale，不序列化完整 ItemDataSO。舊存檔缺少 homeNest 時建立空家園，原貨幣、所有權與裝備槽保持相容。

InventoryManager.TryPlaceHomeFurniture 驗證現有資產為已擁有 Furniture、位置／旋轉為有限值、scale 為正數及家園版本受支援，再透過深拷貝與同一 Commit 保存。每個 itemId 目前最多一筆家具紀錄，重放只更新配置，不增加 ownership 或扣款。TrySetHomeButler 只接受已擁有的 Butler，空字串可清除；不新增管家或任何道具。

載入時保留無法解析的 ID 與未來 Exterior／Decoration 資料，以便恢復；GetActiveHomeFurniture／GetActiveHomeButler 僅回傳仍可解析且具所有權的有效配置，不讓未知或未擁有資產生效。回傳資料為深拷貝，不可藉修改快照繞過交易。未支援的家園版本不允許新增配置。Exterior／Decoration 欄位僅預留，未加入 ItemType 或操作功能。

驗證涵蓋舊存檔 migration、UI 開關、家園存檔重讀、未知 ID 恢復保留、未擁有／錯誤類型／非法 Transform 拒絕，以及同一資產 2D／3D 參照不重複所有權。
