# Chicken Rush：GitHub 上傳與結構檢查

## 目前檔案盤點（2026-09-06）

```text
小雞衝衝衝遊戲測試/
├── .gitignore
├── GITHUB_SETUP.md
├── README.md
├── Assets/
│   └── Scripts/
│       ├── ChickenSpawner.cs
│       ├── ChickenController.cs
│       ├── NestController.cs
│       ├── GameManager.cs
│       └── HoldSpawnInput.cs
└── Tests/
    └── Test-HoldSpawnInput.ps1
```

以上是原始碼起點，尚非可直接在 Unity Hub 開啟的完整專案。目前缺少 Packages/manifest.json、Packages/packages-lock.json、ProjectSettings/ProjectVersion.txt 與其他專案設定，以及場景、Prefab 和資產 .meta。

請用預定 Unity 版本建立 2D 專案，匯入這些腳本，依 README 建立場景／Prefab 並儲存。將 Unity 產生的 Assets（含 .meta）、Packages 與 ProjectSettings 一併提交；不要手動杜撰套件版本或 .meta GUID。Library、Temp、Logs 等快取則不提交。

## 建立本機版本庫與首次提交

以下為 PowerShell 指令；逐步執行，遇到錯誤先停止。已完成的步驟可略過。

```powershell
Set-Location -LiteralPath 'C:\Users\alluser\Documents\ChatGPT\小雞衝衝衝遊戲測試'
git init -b main
git config --get user.name
git config --get user.email
```

若尚未設定提交者身分，填入自己的資料；已有正確設定就略過：

```powershell
git config user.name '你的 Git 提交名稱'
git config user.email '你的已驗證信箱或 GitHub noreply 信箱'
```

確認檔案後提交（不要在已有相同提交時重複提交）：

```powershell
git add .gitignore Assets Tests README.md GITHUB_SETUP.md
git diff --cached --stat
git diff --cached --check
git commit -m "Initial Chicken Rush MVP source"
```

## 建立 GitHub 遠端並 Push

在 https://github.com/new 建立儲存庫，選擇自己的帳號／組織與公開或私有。新庫保持空白，不勾選 README、.gitignore 或 License，避免產生另一份初始歷史。

以下 OWNER 與 REPOSITORY 是佔位字，請換成實際名稱：

```powershell
git remote add origin https://github.com/OWNER/REPOSITORY.git
git remote -v
git push -u origin main
```

HTTPS 認證若出現 Git Credential Manager 登入視窗，依提示在瀏覽器登入；不要把 Token 寫進遠端網址。若 origin 已存在，先核對 `git remote -v`，不要再次 add 或未確認就覆蓋。若遠端不是空庫，先 fetch 並檢查歷史，不要 force push。

可選替代方式：若日後安裝 GitHub CLI，可用下列指令取代「網頁建庫、remote add、push」三步；本機目前未安裝 gh。`--private` 表示私有，如確定公開才改成 `--public`。

```powershell
gh auth login
gh repo create OWNER/REPOSITORY --private --source=. --remote=origin --push
```

## 上傳後驗證

```powershell
git fetch origin
git status --short --branch
git rev-parse HEAD
git ls-remote origin refs/heads/main
git diff --exit-code HEAD origin/main
git ls-tree -r --name-only origin/main
```

HEAD 與遠端 main 的 SHA 應相同，diff 無差異，且遠端清單應包含上方檔案。此檢查證明原始碼同步，不等於 Unity 專案可執行。

NestController.cs 已存在，可繼續編寫容量、滿窩事件與切換流程；實際驗收需雞窩 Trigger Collider2D、小雞 Prefab、GameManager 引用、下一窩 Prefab 與音效事件，詳細接線見 README。尚未完成 Unity 編譯與 Play Mode 驗證。

參考：[GitHub 官方上傳步驟](https://docs.github.com/en/migrations/importing-source-code/using-the-command-line-to-import-source-code/adding-locally-hosted-code-to-github)、[Unity .gitignore 範本](https://github.com/github/gitignore/blob/main/Unity.gitignore)。
