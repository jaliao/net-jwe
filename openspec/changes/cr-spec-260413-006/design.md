## Context

NetJwe.Api 已具備 Razor Pages Demo 介面與核心 `IJweService`。Mark 的系統需要以程式方式呼叫解密，目前只能透過網頁人工操作。需在現有專案中新增 REST API 端點，並在 Demo 頁面補充說明。

## Goals / Non-Goals

**Goals:**
- 提供 `POST /api/decrypt` 端點，回傳 Base64 JSON（含 filename 與 zipBase64）
- 在現有 Demo 頁面新增 API 說明區塊（端點規格、request/response 格式、curl 範例）

**Non-Goals:**
- 驗證機制（API key、JWT auth）
- Swagger / OpenAPI 文件自動產生
- 分頁或批次解密

## Decisions

**回傳格式：Base64 JSON**
回傳 `{"filename":"...", "zipBase64":"..."}` 而非 binary stream。
理由：Mark 的系統呼叫後可直接解析 JSON，不需處理 Content-Disposition；binary 回傳需額外處理 HTTP content type 與 stream reading。

**Controller 沿用現有 IJweService DI**
`DecryptController` 透過建構子注入 `IJweService`，與 Demo 頁面共用同一個服務實例，不重複實作邏輯。

**錯誤回傳：HTTP 400 + JSON 錯誤訊息**
解密失敗（參數錯誤、token 損毀）回傳 `400 Bad Request`，body 為 `{"error":"..."}` 說明原因，讓 Mark 的系統可程式化處理。

**API 說明區塊位置：現有 Demo 頁面底部**
不另開新頁面，減少維護成本；說明區塊包含 curl 指令與 request/response JSON 範例。

## Risks / Trade-offs

- [無驗證機制] 端點公開，任何人知道 URL 皆可呼叫 → 接受（內部使用環境，非公開服務）
- [Base64 傳輸大小] zip 轉 Base64 增加約 33% 傳輸量 → 接受（zip 檔案預期不大）

## Migration Plan

1. `Program.cs` 新增 `builder.Services.AddControllers()` 與 `app.MapControllers()`
2. 新增 `Controllers/DecryptController.cs`
3. `Pages/Index.cshtml` 新增 API 說明區塊
4. `dotnet build` 確認無錯誤
