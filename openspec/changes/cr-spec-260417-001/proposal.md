## Why

現行 `IJweService.Decrypt` 要求呼叫端自行傳入**已解密的** `secret_key`（32 bytes），
但 myData SP-API 實際傳給 SP 的是以 AES/CBC/PKCS5Padding 加密後的密文（base64，64 字元）。
SP 需先用 `client_secret × 2`（32 bytes key）搭配後台 `cbc_iv` 解出明文 secret_key，
才能進行第二步的 JWE 解密。目前缺少這個「Step 1」能力，
導致呼叫端必須在函式庫外自行實作 AES/CBC 解密，與職責不符。

## What Changes

- 新增 `SecretKeyDecryptor` 服務，封裝 Step 1：AES/CBC/PKCS5Padding 解密 encrypted_secret_key
- 在 `IJweService` / `JweService` 新增 `DecryptWithEncryptedKey` 方法，整合 Step 1 + Step 2 的完整流程
- 原有 `Decrypt(token, secretKey, expectedIv)` 保留，不做 **BREAKING** 變更
- 補充對應單元測試（以本次真實測試資料作為測試向量）

## Capabilities

### New Capabilities
- `secret-key-decryption`: 以 client_secret 與 cbc_iv 解密 myData SP-API 傳來的加密 secret_key（AES/CBC/PKCS5Padding）

### Modified Capabilities
- `jwe-decryption`: IJweService 新增 `DecryptWithEncryptedKey` 方法（擴充既有規格，非破壞性）

## Impact

- `src/NetJwe.Core/Services/JweDecryptor.cs` — 不改，職責明確
- `src/NetJwe.Core/Services/SecretKeyDecryptor.cs` — 新增
- `src/NetJwe.Core/Services/JweService.cs` — 新增方法、注入 SecretKeyDecryptor
- `src/NetJwe.Core/Interfaces/IJweService.cs` — 新增方法簽章
- `src/NetJwe.Core/ServiceCollectionExtensions.cs` — 更新 DI 註冊
- `tests/NetJwe.Core.Tests/JweDecryptorTests.cs` — 補充 SecretKeyDecryptor 測試
- `tests/NetJwe.Core.Tests/JweServiceTests.cs` — 新增整合測試（使用本次真實資料）
