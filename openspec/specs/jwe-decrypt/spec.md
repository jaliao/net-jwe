## ADDED Requirements

### Requirement: 驗證 IV 與管理後臺一致
系統 SHALL 比對 JWE 中解碼後的 IV 與呼叫端傳入的 `expectedIv`（來自 myData 管理後臺），兩者 MUST 完全相同，否則拒絕繼續解密。

#### Scenario: IV 驗證通過
- **WHEN** JWE 內的 IV 與 `expectedIv` 完全一致
- **THEN** 系統繼續執行後續解封裝與解密步驟

#### Scenario: IV 驗證失敗
- **WHEN** JWE 內的 IV 與 `expectedIv` 不一致
- **THEN** 系統拋出 `JweException`，訊息說明 IV 不符，停止所有後續操作

### Requirement: 解封裝 CEK（CBC 模式）
系統 SHALL 使用呼叫端提供的 `secretKey`（32 bytes，256 bits），以 AES Key Wrap（RFC 3394）演算法解封裝 JWE 中的 `encryptedKey`，取得 512 bits 的 CEK，其中前 256 bits 為 MAC key，後 256 bits 為 AES key。

#### Scenario: 成功解封裝
- **WHEN** `secretKey` 正確且 `encryptedKey` 未遭篡改
- **THEN** 系統取得 64 bytes 的 CEK

#### Scenario: secretKey 長度錯誤
- **WHEN** 傳入的 `secretKey` 長度不等於 32 bytes
- **THEN** 系統拋出 `JweException`，說明金鑰長度必須為 256 bits

#### Scenario: 解封裝失敗（金鑰錯誤）
- **WHEN** `secretKey` 不正確，導致 AESWrap 解封裝失敗
- **THEN** 系統拋出 `JweException`，說明 CEK 解封裝失敗

### Requirement: 驗證 Authentication Tag 完整性
系統 SHALL 在解密 ciphertext 前，依 JWE 規範重新計算 authentication tag，並與 JWE 中的 `authenticationTag` 比對，確保資料未被篡改。

#### Scenario: Tag 驗證通過
- **WHEN** 重新計算的 authentication tag 與 JWE 中的 tag 完全一致
- **THEN** 系統繼續執行 ciphertext 解密

#### Scenario: Tag 驗證失敗
- **WHEN** 重新計算的 tag 與 JWE 中的 tag 不一致
- **THEN** 系統拋出 `JweException`，說明 JWE 完整性驗證失敗，資料可能遭篡改

### Requirement: 解密 ciphertext（CBC 模式）
系統 SHALL 使用 CEK 中的 AES key（後 256 bits）與 IV，以 AES-CBC（PKCS7 padding）演算法解密 ciphertext，取得明文 JSON 字串。

#### Scenario: 解密成功
- **WHEN** CEK 與 IV 正確，ciphertext 合法
- **THEN** 系統回傳 UTF-8 解碼後的 JSON 明文字串

#### Scenario: 使用官方範例參數解密
- **WHEN** 以 `secret_key = dgFpgO7FhNF15UJsOB1xmCjwwWw3SO6D`、`IV = HtzGY7g1hLy5bl9R` 及官方 JWE 範例字串執行解密
- **THEN** 解密成功，回傳合法的 JSON 字串，包含 `filename` 與 `data` 欄位

### Requirement: 解密 ciphertext（GCM 模式）
系統 SHALL 在 `alg = A256GCMKW` 模式下，使用 AES-GCM 解封裝 CEK，並以 CEK 與 IV 執行 AES-GCM 解密，同時利用 authentication tag 完成完整性驗證。

#### Scenario: GCM 解密成功
- **WHEN** 傳入 GCM 模式的 JWE 字串與對應的 `secretKey`
- **THEN** 系統正確解密並回傳明文 JSON 字串
