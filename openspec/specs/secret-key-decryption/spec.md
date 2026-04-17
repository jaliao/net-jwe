## Requirements

### Requirement: 解密 encrypted_secret_key
系統 SHALL 接受 SP-API 傳來的 `encrypted_secret_key`（AES/CBC/PKCS5Padding 加密後的 base64 字串），
使用 `client_secret × 2`（32 bytes）為金鑰、`cbc_iv`（16 bytes）為向量，
解密後取得明文 `secret_key`（32 bytes，英數字含大小寫）。

背景：myData 以 AES/CBC/PKCS5Padding 加密真正的 secret_key，金鑰為 `client_secret`（16 字元）重複兩次，
結果為 32 bytes（256 bits）。加密後的 secret_key 以 base64 編碼，長度為 64 字元。

#### Scenario: 解密成功
- **WHEN** 傳入合法的 `encryptedSecretKeyBase64`（64 字元）、`clientSecret`（16 字元）、`cbcIv`（16 字元）
- **THEN** 系統回傳 32 bytes 的明文 `secret_key`

#### Scenario: clientSecret 長度錯誤
- **WHEN** `clientSecret` 長度不等於 16 字元
- **THEN** 系統拋出 `JweException`，說明 client_secret 必須為 16 字元

#### Scenario: cbcIv 長度錯誤
- **WHEN** `cbcIv` 長度不等於 16 字元
- **THEN** 系統拋出 `JweException`，說明 cbc_iv 必須為 16 字元

#### Scenario: clientSecret 錯誤（padding 驗證失敗）
- **WHEN** `clientSecret` 不正確，導致 AES 解密後 PKCS7 padding 無效
- **THEN** 系統拋出 `JweException`，說明解密失敗，client_secret 或 cbc_iv 不正確

### Requirement: 整合兩步驟解密流程
系統 SHALL 提供 `IJweService.DecryptWithEncryptedKey()` 方法，
自動串接 Step 1（解密 encrypted_secret_key）與 Step 2（JWE 解密），
呼叫端僅需傳入 SP-API 原始參數。

#### Scenario: 完整兩步驟流程
- **WHEN** 傳入 JWE token、encrypted_secret_key、client_secret、cbc_iv
- **THEN** 系統先解密 encrypted_secret_key 取得明文 secret_key，再完整解密 JWE，回傳 `JweDecryptResult`

#### Scenario: 向下相容
- **WHEN** 呼叫既有的 `IJweService.Decrypt(token, secretKey, expectedIv)`
- **THEN** 行為不變，直接以明文 secretKey 解密 JWE
