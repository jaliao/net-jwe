## Requirements

### Requirement: POST /api/decrypt 端點（兩步驟模式）
系統 SHALL 提供 `POST /api/decrypt` 端點，支援兩種解密模式：
- **兩步驟模式**：提供 `clientSecret` 時，先以 AES/CBC 解密 `secretKey`（encrypted_secret_key），再解密 JWE
- **單步模式**（向下相容）：未提供 `clientSecret` 時，直接以 `secretKey` 作為明文 secret_key 解密 JWE

Request body（application/json）：
```json
{
  "jweToken": "<JWE Compact Serialization 字串>",
  "secretKey": "<encrypted_secret_key (64 字元) 或明文 secret_key (32 字元)>",
  "clientSecret": "<client_secret, 16 字元，選填>",
  "iv": "<cbc iv, 16 字元>"
}
```

成功回應（200 OK）：
```json
{
  "filename": "xxx.zip",
  "zipBase64": "<Base64 編碼的 zip 內容>"
}
```

失敗回應（400 Bad Request）：
```json
{
  "error": "<錯誤說明>"
}
```

#### Scenario: 兩步驟模式，解密成功
- **WHEN** 傳入有效的 jweToken、encrypted_secret_key、clientSecret、iv
- **THEN** 回傳 HTTP 200，body 含 filename 與 zipBase64

#### Scenario: 單步模式，解密成功（向下相容）
- **WHEN** 傳入有效的 jweToken、明文 secretKey（32 字元）、iv，未提供 clientSecret
- **THEN** 回傳 HTTP 200，body 含 filename 與 zipBase64

#### Scenario: 缺少必填欄位
- **WHEN** jweToken、secretKey、iv 任一為空白或未傳入
- **THEN** 回傳 HTTP 400，body 含 error 說明缺少必填欄位

#### Scenario: token 損毀或金鑰不符
- **WHEN** 傳入格式正確但內容錯誤的 token 或金鑰
- **THEN** 回傳 HTTP 400，body 含 error 說明解密失敗原因

#### Scenario: jweToken 含換行或空白
- **WHEN** jweToken 字串中夾雜換行（\n）或空白（來自 PDF 複製貼上）
- **THEN** 系統自動清除後再解密，不回傳錯誤
