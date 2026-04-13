## Context

`NetJwe.Api` 目前為 class library（`Microsoft.NET.Sdk`），需升級為可獨立執行的 ASP.NET Core Razor Pages 應用程式。`NetJwe.Core` 已實作 `IJweService`，透過 `AddNetJwe()` DI 擴充方法注入。

## Goals / Non-Goals

**Goals:**
- 單一 Razor Page（`/`）提供 JWE 解密測試介面
- 預填 demo data（官方範例 token、secret_key、IV），一鍵測試
- 解密成功：顯示 filename，提供 zip 檔案下載（`application/zip`）
- 解密失敗：顯示具體錯誤訊息（如 IV 不符、金鑰錯誤）

**Non-Goals:**
- 不做使用者驗證或權限控管
- 不儲存任何解密資料至資料庫
- 不部署至正式環境（純開發/驗收用途）

## Decisions

### 決策一：使用 Razor Pages，不用 MVC 或 Blazor
單一頁面需求，Razor Pages 最精簡，無需額外 Controller 路由設定。

### 決策二：表單 POST，不用 AJAX
最簡單的實作方式；解密結果（包含 zip 下載）直接在同一頁面顯示或觸發下載。成功時返回 zip 檔案（FileContentResult），失敗時回到頁面顯示錯誤。

### 決策三：zip 下載以獨立 POST endpoint 處理
頁面 POST 執行解密並顯示結果（filename、成功/失敗訊息），另提供「下載 zip」按鈕觸發第二次 POST 取得檔案內容，避免在 PageModel 中混用 Page() 與 FileResult。

### 決策四：demo data 預填於 PageModel 預設值
`[BindProperty]` 欄位預設值設為官方範例資料，首次載入頁面即可直接按「開始解密」測試（需真實 token 時再覆蓋）。

## Risks / Trade-offs

- **JWE token 預填值（PDF 擷取）長度不足**：目前官方文件的 encrypted_key 段有字元遺失，預填後解密會失敗並顯示錯誤訊息，符合預期行為，不需特別處理。
- **zip 存入 TempData**：zip bytes 暫存於 `TempData`（Session 後端），資料量大時有記憶體壓力 → 驗收用途資料量小，可接受。
