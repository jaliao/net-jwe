## 1. 資料模型與共用類別

- [x] 1.1 新增 `JweComponents` record（`Header`、`EncryptedKey`、`Iv`、`Ciphertext`、`AuthenticationTag` 各為 `byte[]`）
- [x] 1.2 新增 `JweAlgorithmMode` enum（`CbcHs512`、`Gcm`）
- [x] 1.3 新增 `JweDecryptResult` record（`FileName`、`FileBytes`）
- [x] 1.4 新增 `MyDataPayload` 內部 DTO（`filename`、`data` 欄位，用於 JSON 反序列化）

## 2. JweParser — 解析與拆解

- [x] 2.1 實作 `JweParser.Parse(string token)` → 以 `.` 拆分為五段，驗證段數為 5
- [x] 2.2 實作 Base64Url 解碼各段，填入 `JweComponents`；任一段失敗拋出 `JweException`（標明失敗段名）
- [x] 2.3 實作 `JweParser.ParseHeader(byte[] headerBytes)` → JSON 解析取得 `alg`/`enc`，回傳 `JweAlgorithmMode`
- [x] 2.4 不支援的 `alg`/`enc` 組合時拋出 `JweException`

## 3. JweValidator — 驗證

- [x] 3.1 實作 `JweValidator.ValidateIv(byte[] jweIv, string expectedIv)` → 比對兩者，不符時拋出 `JweException`
- [x] 3.2 實作 `JweValidator.ValidateAuthTag(JweComponents components, byte[] cek)` → 依 RFC 7516 重新計算 HMAC-SHA512 authentication tag 並比對（CBC 模式）
- [x] 3.3 GCM 模式的 authentication tag 驗證整合至 `AesGcm.Decrypt` 呼叫（內建驗證）

## 4. JweDecryptor — 解封裝與解密

- [x] 4.1 實作 `JweDecryptor.UnwrapCekCbc(byte[] encryptedKey, byte[] secretKey)` → AES Key Wrap（`System.Security.Cryptography.AesKeyWrap`）解封裝，驗證 `secretKey` 長度為 32 bytes
- [x] 4.2 實作 `JweDecryptor.DecryptCbc(byte[] ciphertext, byte[] cek, byte[] iv)` → AES-CBC（PKCS7）解密，使用 CEK 後 256 bits 為 AES key
- [x] 4.3 實作 `JweDecryptor.UnwrapCekGcm(byte[] encryptedKey, byte[] secretKey, byte[] keyIv, byte[] keyTag)` → AES-GCM 解封裝 CEK
- [x] 4.4 實作 `JweDecryptor.DecryptGcm(byte[] ciphertext, byte[] cek, byte[] iv, byte[] tag)` → AES-GCM 解密（`System.Security.Cryptography.AesGcm`）

## 5. MyDataPayloadParser — Payload 解析

- [x] 5.1 實作 `MyDataPayloadParser.Parse(string json)` → JSON 反序列化為 `MyDataPayload`，缺少欄位時拋出 `JweException`
- [x] 5.2 實作 `MyDataPayloadParser.DecodeData(string data)` → 去除前置碼 `application/zip;data:` 後進行 Base64Url 解碼，回傳 `byte[]`
- [x] 5.3 前置碼格式不符或 Base64Url 解碼失敗時拋出 `JweException`

## 6. JweService — 門面整合

- [x] 6.1 實作 `JweService : IJweService`，注入 `JweParser`、`JweValidator`、`JweDecryptor`、`MyDataPayloadParser`
- [x] 6.2 實作 `Decrypt(string token, string secretKey, string expectedIv)` → 協調完整流程，回傳 `JweDecryptResult`
- [x] 6.3 更新 `IJweService` 介面方法簽名（加入 `secretKey`、`expectedIv` 參數）
- [x] 6.4 在 `NetJwe.Core` 新增 `ServiceCollectionExtensions`，提供 `AddNetJwe()` DI 擴充方法

## 7. 單元測試

- [x] 7.1 `JweParserTests`：正常拆解、段數錯誤、Base64Url 失敗、header 模式判斷
- [x] 7.2 `JweValidatorTests`：IV 驗證通過、IV 不符
- [x] 7.3 `JweDecryptorTests`：secretKey 長度錯誤、錯誤金鑰解密失敗
- [x] 7.4 `MyDataPayloadParserTests`：正常解析、缺少欄位、前置碼錯誤、Base64Url 失敗
- [x] 7.5 `JweServiceTests`（整合）：佔位測試已建立；需 Mark 提供真實 myData JWE token 後補上驗收測試（PDF 擷取的範例 token encrypted_key 段有字元遺失）
