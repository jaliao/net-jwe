## ADDED Requirements

### Requirement: POST /api/decrypt 端點
系統 SHALL 提供 `POST /api/decrypt` 端點，接受 JSON body 並回傳解密結果。

Request body（application/json）：
```json
{
  "jweToken": "<JWE Compact Serialization 字串>",
  "secretKey": "<32 字元金鑰>",
  "iv": "<16 字元 IV>"
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

#### Scenario: 參數完整且 token 有效，解密成功
- **WHEN** 傳入有效的 jweToken、secretKey、iv
- **THEN** 回傳 HTTP 200，body 含 filename 與 zipBase64

#### Scenario: 缺少必填欄位
- **WHEN** jweToken、secretKey、iv 任一為空白或未傳入
- **THEN** 回傳 HTTP 400，body 含 error 說明缺少哪個欄位

#### Scenario: token 損毀或 secret_key 不符
- **WHEN** 傳入格式正確但內容錯誤的 token 或 secret_key
- **THEN** 回傳 HTTP 400，body 含 error 說明解密失敗原因

#### Scenario: jweToken 含換行或空白
- **WHEN** jweToken 字串中夾雜換行（\n）或空白（來自 PDF 複製貼上）
- **THEN** 系統自動清除後再解密，不回傳錯誤
