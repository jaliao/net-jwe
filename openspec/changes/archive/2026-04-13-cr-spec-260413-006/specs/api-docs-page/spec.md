## ADDED Requirements

### Requirement: Demo 頁面顯示 API 說明區塊
現有 Demo 頁面 SHALL 在解密表單下方新增 API 說明區塊，內容包含端點規格、request/response JSON 範例、curl 測試指令。

說明區塊需包含：
- 端點：`POST /api/decrypt`
- Content-Type：`application/json`
- Request body 欄位說明（jweToken、secretKey、iv）
- 成功 response 範例（200，含 filename、zipBase64）
- 失敗 response 範例（400，含 error）
- 可直接複製使用的 curl 指令範例

#### Scenario: 頁面載入時顯示 API 說明
- **WHEN** 使用者開啟 Demo 頁面
- **THEN** 頁面包含 API 說明區塊，可見端點 URL 與 curl 範例

#### Scenario: curl 指令使用 Demo 預設參數
- **WHEN** 說明區塊中的 curl 範例
- **THEN** 預填與頁面 Demo Data 相同的 secret_key 與 IV，jweToken 使用省略顯示提示使用者替換
