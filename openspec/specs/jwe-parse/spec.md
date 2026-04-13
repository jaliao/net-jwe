## ADDED Requirements

### Requirement: 解析 JWE Compact Serialization 字串
系統 SHALL 將 JWE Compact Serialization 字串以 `.` 拆分為五段，並對每段進行 Base64Url 解碼，產生結構化的 `JweComponents` 物件，包含 `Header`、`EncryptedKey`、`Iv`、`Ciphertext`、`AuthenticationTag` 五個 byte array 欄位。

#### Scenario: 格式正確的 JWE 字串
- **WHEN** 傳入符合 RFC 7516 格式的 JWE Compact Serialization 字串（五段以 `.` 分隔）
- **THEN** 系統成功拆解並回傳 `JweComponents`，五個欄位均為非空的 byte array

#### Scenario: 段數不足或過多
- **WHEN** 傳入的字串分隔後段數不等於 5
- **THEN** 系統拋出 `JweException`，訊息說明格式錯誤

#### Scenario: Base64Url 解碼失敗
- **WHEN** 任一段的內容無法進行合法的 Base64Url 解碼
- **THEN** 系統拋出 `JweException`，並標明是哪一段解碼失敗

### Requirement: 解析 JWE Header
系統 SHALL 將解碼後的 header byte array 解析為 JSON，取得 `alg` 與 `enc` 欄位值，用以判斷後續解密所需的演算法模式。

#### Scenario: 預設模式 header
- **WHEN** header 中 `alg` 為 `A256KW`、`enc` 為 `A256CBC-HS512`
- **THEN** 系統識別為 CBC 模式，後續流程使用 AES Key Wrap + AES-CBC 演算法

#### Scenario: GCM 模式 header
- **WHEN** header 中 `alg` 為 `A256GCMKW`、`enc` 為 `A256GCM`
- **THEN** 系統識別為 GCM 模式，後續流程使用 AES-GCM Key Wrap + AES-GCM 演算法

#### Scenario: 不支援的演算法
- **WHEN** header 中的 `alg` 或 `enc` 值不屬於上述兩種模式
- **THEN** 系統拋出 `JweException`，說明不支援的演算法名稱
