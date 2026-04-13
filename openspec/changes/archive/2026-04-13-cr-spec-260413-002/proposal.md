## Why

開發人員與客戶需要一個簡單的測試介面，能夠直接貼入 myData 回傳的 JWE token 並執行解密，驗證 `NetJwe.Core` 函式庫的正確性，以及在取得真實 myData 憑證後進行端對端驗收。

## What Changes

- 將 `NetJwe.Api` 專案由 class library 升級為 ASP.NET Core Razor Pages 應用程式
- 新增單一 Demo 頁面，包含 JWE token、secret_key、IV 輸入欄位
- 預填官方範例資料（demo data），方便快速測試
- 按下「開始解密」後顯示結果或錯誤訊息，成功時提供 zip 檔案下載

## Capabilities

### New Capabilities

- `decrypt-ui`：Razor Pages 測試頁面，輸入 JWE 三要素後執行解密，顯示 `filename`、提供 zip 下載

### Modified Capabilities

（無）

## Impact

- **`NetJwe.Api`**：由 classlib 改為 `Microsoft.NET.Sdk.Web`，新增 Razor Pages 與 Program.cs
- **相依**：引用 `NetJwe.Core`（已存在）、`Microsoft.Extensions.DependencyInjection`
- **不影響**：`NetJwe.Core` 函式庫與測試專案無需修改
