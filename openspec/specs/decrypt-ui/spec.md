## Requirements

### Requirement: 頁面預填 demo data
系統 SHALL 在首次載入頁面時，自動將 Demo 預設值填入對應輸入欄位。
JWE Token 欄位優先從 solution 根目錄的 `data1.txt` 動態載入；
其餘欄位使用測試環境預設值。

#### Scenario: 首次載入（data1.txt 存在）
- **WHEN** 使用者開啟 Demo 頁面（GET /），且 `data1.txt` 存在
- **THEN** JWE Token 預填 `data1.txt` 內容，Secret Key 預填 `oWgZzJHnJ8Vkty+YlkldC7TT8aQr7jcQUXlpmEvCESSQqDHoEA+ueZXF6Bm8L8tn`，Client Secret 預填 `8rtR3mtWlTynOigI`，IV 預填 `RjiCdd8OgJcTYgZr`

#### Scenario: 首次載入（data1.txt 不存在）
- **WHEN** 使用者開啟 Demo 頁面，且 `data1.txt` 不存在
- **THEN** JWE Token 欄位為空，其餘欄位填入預設值

### Requirement: 四欄位輸入
系統 SHALL 在表單中提供四個輸入欄位：JWE Token、Secret Key、Client Secret、IV。

#### Scenario: 欄位顯示
- **WHEN** 使用者開啟 Demo 頁面
- **THEN** 顯示 JWE Token（textarea）、Secret Key（input）、Client Secret（input）、IV（input）四個欄位

### Requirement: 執行解密
系統 SHALL 在使用者按下「開始解密」後，以輸入欄位的值呼叫 `POST /api/decrypt`，
若 Client Secret 有值則啟用兩步驟模式，否則以 Secret Key 直接解密。

#### Scenario: 兩步驟解密成功
- **WHEN** 使用者填入合法的 JWE token、encrypted_secret_key、client_secret、IV 並送出表單
- **THEN** 頁面顯示 `filename` 與「下載 zip」按鈕

#### Scenario: 解密失敗
- **WHEN** 使用者送出表單，但解密過程拋出錯誤
- **THEN** 頁面顯示具體的錯誤訊息文字，不顯示下載按鈕

### Requirement: 下載 zip 檔案
系統 SHALL 在使用者點擊「下載 zip」後，將解密所得的 zip 二進位內容以 `application/zip` 回傳，觸發瀏覽器下載。

#### Scenario: 下載成功
- **WHEN** 使用者點擊「下載 zip」按鈕
- **THEN** 瀏覽器下載以 `filename` 命名的 zip 檔案

### Requirement: 輸入驗證
系統 SHALL 在 JWE Token、Secret Key、IV 任一為空時，不執行解密並顯示提示訊息。

#### Scenario: 欄位為空
- **WHEN** 使用者未填寫 JWE Token、Secret Key 或 IV 其中之一即送出表單
- **THEN** 頁面提示「請填寫所有欄位」，不呼叫解密服務
