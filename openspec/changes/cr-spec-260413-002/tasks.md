## 1. 升級 NetJwe.Api 專案

- [x] 1.1 將 `NetJwe.Api.csproj` 的 SDK 由 `Microsoft.NET.Sdk` 改為 `Microsoft.NET.Sdk.Web`
- [x] 1.2 新增 `Program.cs`，設定 Razor Pages、DI（`AddNetJwe()`）、TempData（Cookie）
- [x] 1.3 新增 `appsettings.json`（空白設定檔）

## 2. Demo 頁面

- [x] 2.1 新增 `Pages/Index.cshtml.cs`（PageModel）：定義 `[BindProperty]` 欄位（JweToken、SecretKey、Iv），預設值填入 demo data
- [x] 2.2 實作 `OnPostDecrypt()`：呼叫 `IJweService.Decrypt()`，結果存入 PageModel 屬性（FileName、ErrorMessage、zip bytes 存 TempData）
- [x] 2.3 新增 `Pages/Index.cshtml`（Razor 頁面）：輸入表單、結果顯示區、「下載 zip」按鈕
- [x] 2.4 實作 `OnPostDownload()`：從 TempData 取出 zip bytes，回傳 `FileContentResult`

## 3. 樣式與使用體驗

- [x] 3.1 引入 Bootstrap 5 CDN，設定基本版面（`Pages/Shared/_Layout.cshtml`）
- [x] 3.2 JWE Token 欄位使用 `<textarea>`（多行），其餘欄位使用 `<input>`
- [x] 3.3 結果區塊：成功顯示綠色提示框（含 filename）、失敗顯示紅色提示框（含錯誤訊息）

## 4. 驗證

- [x] 4.1 `dotnet build` 成功，無 warning 或 error
- [x] 4.2 修正 DemoToken：encrypted_key 補上 PDF 換行遺失的 "1-" 首兩字元（96 chars = 72 bytes = 正確 A256KW 長度）
- [x] 4.3 確認空白欄位送出時顯示提示訊息
