## ADDED Requirements

### Requirement: 解析 myData JSON Payload
系統 SHALL 將解密後的 JSON 字串解析為結構化物件，取得 `filename`（字串）與 `data`（字串）兩個欄位。

#### Scenario: 合法 JSON payload
- **WHEN** 解密結果為包含 `filename` 與 `data` 欄位的合法 JSON 字串
- **THEN** 系統成功解析並回傳對應的值

#### Scenario: 缺少必要欄位
- **WHEN** JSON 中缺少 `filename` 或 `data` 欄位
- **THEN** 系統拋出 `JweException`，說明 payload 格式不符合 myData 規範

#### Scenario: 非合法 JSON
- **WHEN** 解密結果無法解析為 JSON
- **THEN** 系統拋出 `JweException`，說明解密內容非預期的 JSON 格式

### Requirement: 去除 data 欄位前置碼並 Base64Url 解碼
系統 SHALL 移除 `data` 欄位的前置碼 `application/zip;data:`，再對剩餘內容進行 Base64Url 解碼，取得 zip 檔案的 byte array。

#### Scenario: 前置碼正確且解碼成功
- **WHEN** `data` 欄位以 `application/zip;data:` 開頭，後接合法 Base64Url 字串
- **THEN** 系統去除前置碼後解碼，回傳 zip 二進位內容（byte array）

#### Scenario: 前置碼格式不符
- **WHEN** `data` 欄位不以 `application/zip;data:` 開頭
- **THEN** 系統拋出 `JweException`，說明 data 前置碼格式不符預期

#### Scenario: Base64Url 解碼失敗
- **WHEN** 去除前置碼後的內容無法合法解碼
- **THEN** 系統拋出 `JweException`，說明 data 欄位解碼失敗

### Requirement: 回傳 JweDecryptResult
系統 SHALL 將 `filename` 字串與解碼後的 zip byte array 封裝為 `JweDecryptResult` 回傳，供呼叫端儲存或後續處理。

#### Scenario: 完整解密流程成功
- **WHEN** JWE 解密、payload 解析、Base64Url 解碼均成功
- **THEN** 系統回傳 `JweDecryptResult`，`FileName` 為 `{client_id}.zip`，`FileBytes` 為非空的 byte array
