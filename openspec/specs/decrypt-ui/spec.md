## ADDED Requirements

### Requirement: 頁面預填 demo data
系統 SHALL 在首次載入頁面時，自動將官方範例 JWE token、secret_key、IV 填入對應輸入欄位。

#### Scenario: 首次載入
- **WHEN** 使用者開啟 Demo 頁面（GET /）
- **THEN** JWE Token 欄位預填官方範例 token，Secret Key 預填 `dgFpgO7FhNF15UJsOB1xmCjwwWw3SO6D`，IV 預填 `HtzGY7g1hLy5bl9R`

### Requirement: 執行解密
系統 SHALL 在使用者按下「開始解密」後，以輸入欄位的值呼叫 `IJweService.Decrypt()`，並在同一頁面顯示結果。

#### Scenario: 解密成功
- **WHEN** 使用者填入合法的 JWE token、secret_key、IV 並送出表單
- **THEN** 頁面顯示 `filename` 與「下載 zip」按鈕

#### Scenario: 解密失敗
- **WHEN** 使用者送出表單，但解密過程拋出 `JweException`
- **THEN** 頁面顯示具體的錯誤訊息文字，不顯示下載按鈕

### Requirement: 下載 zip 檔案
系統 SHALL 在使用者點擊「下載 zip」後，將解密所得的 zip 二進位內容以 `application/zip` 回傳，觸發瀏覽器下載。

#### Scenario: 下載成功
- **WHEN** 使用者點擊「下載 zip」按鈕
- **THEN** 瀏覽器下載以 `filename` 命名的 zip 檔案

### Requirement: 輸入驗證
系統 SHALL 在三個欄位任一為空時，不執行解密並顯示提示訊息。

#### Scenario: 欄位為空
- **WHEN** 使用者未填寫 JWE Token、Secret Key 或 IV 其中之一即送出表單
- **THEN** 頁面提示「請填寫所有欄位」，不呼叫解密服務
