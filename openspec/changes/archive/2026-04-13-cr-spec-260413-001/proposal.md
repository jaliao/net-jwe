## Why

連江縣政府線上申辦平台串接數發部 myData 平台後，收到的回傳資料以 JWE（RFC 7516）格式加密，目前缺乏可直接引用的 .NET 8 解密函式庫，需自行實作以取出實際資料內容。

## What Changes

- 新增 `NetJwe.Core` 核心函式庫，實作 JWE Compact Serialization 解密完整流程
- 支援兩種演算法模式：`A256KW + A256CBC-HS512`（預設）與 `A256GCMKW + A256GCM`
- 解密後自動解析 JSON 結果，回傳結構化資料（`filename` + Base64Url 解碼後的 zip 二進位內容）
- 提供 IV 驗證與 authentication tag 完整性檢查機制

## Capabilities

### New Capabilities

- `jwe-decrypt`：接收 JWE Compact Serialization 字串與 secret_key，執行完整解密流程，回傳解密後的 JSON 內容（`filename`、`data`）
- `jwe-parse`：將 JWE 字串拆解為五段結構（header、encrypted_key、IV、ciphertext、authentication_tag），並對各段進行 Base64Url 解碼與驗證
- `mydata-payload`：解析解密後的 JSON payload，對 `data` 欄位去除前置碼後進行 Base64Url 解碼，以 byte array 形式回傳 zip 檔案內容

### Modified Capabilities

（無，此為全新專案）

## Impact

- **新增套件相依**：需引入支援 AES Key Wrap（RFC 3394）及 AES-CBC / AES-GCM 的 .NET 加密套件（優先使用 `System.Security.Cryptography` 內建 API，不足部分評估 `jose-jwt` 或 `Microsoft.IdentityModel.Tokens`）
- **影響範圍**：`src/NetJwe.Core`（新增）、`tests/NetJwe.Core.Tests`（新增對應測試）
- **對外介面**：透過 `IJweService` 介面暴露功能，主系統透過 DI 注入使用
