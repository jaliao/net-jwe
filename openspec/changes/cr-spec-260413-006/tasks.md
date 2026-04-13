## 1. 啟用 Controllers

- [x] 1.1 `Program.cs` 新增 `builder.Services.AddControllers()`
- [x] 1.2 `Program.cs` 新增 `app.MapControllers()`

## 2. 新增 DecryptController

- [x] 2.1 新增 `Controllers/DecryptRequest.cs`：定義 request model（JweToken、SecretKey、Iv）
- [x] 2.2 新增 `Controllers/DecryptController.cs`（`[ApiController]`、route `api/decrypt`）
- [x] 2.3 實作 `POST` action：清除 token 空白換行、呼叫 `IJweService.Decrypt()`、回傳 `{ filename, zipBase64 }`
- [x] 2.4 錯誤處理：空白欄位回傳 400 + `{ error }`；`JweException` 回傳 400 + `{ error }`；未預期例外回傳 400 + `{ error }`

## 3. Demo 頁面改用 API 串接

- [x] 3.1 `IndexModel` 移除直接注入 `IJweService`，改以 `IHttpClientFactory` 呼叫 `POST /api/decrypt`
- [x] 3.2 `Program.cs` 新增 `builder.Services.AddHttpClient()`
- [x] 3.3 `OnPostDecrypt()`：組裝 JSON request body，呼叫 API，解析回傳的 `{ filename, zipBase64 }`，Base64 解碼後存入 TempData
- [x] 3.4 `OnPostDownload()`：邏輯不變，從 TempData 取出 Base64 字串，解碼後回傳 zip

## 4. Demo 頁面新增 API 說明區塊

- [x] 4.1 `Pages/Index.cshtml` 新增 API 說明 card，包含端點（`POST /api/decrypt`）、Content-Type、欄位說明
- [x] 4.2 新增成功 response 範例（200 JSON）與失敗 response 範例（400 JSON）
- [x] 4.3 新增可複製的 curl 指令範例（預填 Demo 參數，jweToken 留提示文字）

## 5. 驗證

- [x] 5.1 `dotnet build` 成功，無 warning 或 error
- [ ] 5.2 Demo 頁面按「開始解密」確認成功（透過 API 串接）
- [ ] 5.3 curl 呼叫 `POST /api/decrypt` 使用 Demo token，確認回傳 200 含 zipBase64
- [ ] 5.4 傳入空白欄位確認回傳 400 + error 訊息
