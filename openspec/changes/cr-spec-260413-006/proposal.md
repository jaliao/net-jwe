## Why

目前解密功能僅透過 Demo 網頁操作，Mark 的系統需要能以程式方式呼叫解密服務。需要提供 REST API 端點並在網頁上說明使用方式，讓外部系統可整合。

## What Changes

- 新增 `POST /api/decrypt` 端點，接受 JWE token、secret_key、IV，回傳解密後的 zip 檔案（binary）或錯誤訊息
- 新增 API 說明頁面（或在現有 Demo 頁面新增區塊），說明 API 規格、request/response 範例、curl 測試指令

## Capabilities

### New Capabilities
- `decrypt-api`: REST API 端點，接受解密參數並回傳 zip 二進位檔案
- `api-docs-page`: 網頁上的 API 說明區塊，包含端點規格、範例 curl 指令、request/response 格式說明

### Modified Capabilities

## Impact

- `src/NetJwe.Api/Controllers/`：新增 `DecryptController.cs`
- `src/NetJwe.Api/Program.cs`：新增 `AddControllers()` / `MapControllers()`
- `src/NetJwe.Api/Pages/Index.cshtml`：新增 API 說明區塊
