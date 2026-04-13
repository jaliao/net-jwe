# net-jwe — 連江縣政府 myData 加解密

## 專案簡介

本專案為連江縣政府線上申辦平台（[eservice.matsu.gov.tw](https://eservice.matsu.gov.tw)）與數位發展部 myData 中間平台串接所需的 JWE（JSON Web Encryption）加解密函式庫。

使用 .NET 8 開發，以 class library 形式提供，供主系統直接引用。

---

## 背景說明

數發部 myData 平台扮演資料中介角色，彙整各政府機關的資料集（如戶口謄本、國民身分證等），讓民眾在辦理申請業務時，無需自行備齊紙本證明，由服務單位透過 myData 平台向各機關索取所需資料。

連江縣政府作為「服務提供者」，向 myData 平台發出資料請求後，平台回傳的資料內容以 JWE 格式加密。本專案的任務是將這份加密資料正確解密，取出實際內容。

---

## 核心需求

接收 myData 平台回傳的 JWE 加密字串後，解密結果可能為以下兩種形式之一：

1. **檔案下載位址** — 解密後得到一組 URL，再向該位址下載檔案
2. **Base64 編碼的二進位資料** — 解密後直接得到 Base64 字串，解碼後即為檔案內容

### 需實作的功能

- JWE 解密（依據數發部服務提供者技術文件 v4.8，第 47 頁規範）
- 解析解密結果，判斷內容類型（URL / Base64）
- 依內容類型回傳對應的資料結構

---

## JWE 規格說明

> 參考：數發部服務提供者技術文件 v4.8 §39–42、RFC 7516

### JWE Compact Serialization 格式

```
header . encrypted_key . initialization_vector . ciphertext . authentication_tag
```

五段資料皆以 `.` 分隔，每段均為 **Base64Url** 編碼。

### 支援的演算法模式

| 模式 | 金鑰封裝（alg） | 內容加密（enc） | 備註 |
|---|---|---|---|
| 預設 | `A256KW` | `A256CBC-HS512` | 管理介面未特別設定時使用 |
| GCM | `A256GCMKW` | `A256GCM` | 管理介面選「AES/GCM」時使用 |

### 各欄位說明

| 欄位 | 說明 |
|---|---|
| `header` | 宣告 `alg` 與 `enc` 演算法 |
| `encrypted_key` | 以 `secret_key`（myData 核發）透過 A256KW（AESWrap）封裝後的 CEK |
| `initialization_vector` | AES-CBC 初始向量，**必須與 myData 管理後臺取得的 IV 相同** |
| `ciphertext` | 加密後的內容，驗證 `authentication_tag` 後才進行解密 |
| `authentication_tag` | 依 JWE 規範生成，用於確認 JWE 未被篡改 |

**CEK 結構（A256CBC-HS512 模式）：** 512 bits（64 bytes），前 256 bits 為 MAC key，後 256 bits 為 AES key。

### 解密流程

```
1. 以 "." 分割 JWE 字串，取得五段資料
2. Base64Url 解碼各段
3. 驗證 IV 與 myData 管理後臺一致
4. 使用 secret_key + A256KW(AESWrap) 解封裝 encrypted_key → 取得 CEK
5. 驗證 authentication_tag（確保資料未被篡改）
6. 使用 CEK 的 AES key + IV，以 AES-CBC 解密 ciphertext
7. 解密結果為 JSON：{ "filename": "{client_id}.zip", "data": "application/zip;data:<Base64Url>" }
8. 對 data 欄位去除前置碼後進行 Base64Url 解碼，儲存為 filename 指定的 .zip 檔
```

### 解密後的 JSON 結構

```json
{
  "filename": "{client_id}.zip",
  "data": "application/zip;data:<Base64UrlEncoded 二進位內容>"
}
```

- `filename`：下載檔案名稱，格式為 `{client_id}.zip`
- `data`：前置碼 `application/zip;data:` 之後為實際 Base64Url 編碼的 zip 檔內容

---

## 測試工具頁面（Demo UI）

提供一個獨立的測試頁面，供開發與驗收階段手動驗證解密功能：

### 頁面輸入欄位

| 欄位 | 說明 |
|---|---|
| JWE Token | 貼入完整的 JWE Compact Serialization 字串 |
| Secret Key | myData 核發的 256-bit 金鑰（32 字元） |
| IV | myData 管理後臺取得的初始向量（用於驗證） |
| 演算法模式 | 選擇 `A256KW / A256CBC-HS512`（預設）或 `A256GCMKW / A256GCM` |

### 操作流程

1. 填入上述欄位
2. 按下「**開始解密**」按鈕
3. 頁面顯示解密結果：
   - 成功：顯示 `filename` 與解碼後的檔案，提供下載
   - 失敗：顯示具體錯誤訊息（IV 不符、authentication_tag 驗證失敗等）

---

## 技術規格

| 項目 | 規格 |
|---|---|
| 執行環境 | .NET 8 |
| 專案類型 | Class Library + Demo UI |
| 加密標準 | JWE（RFC 7516） |
| 參考文件 | 數發部服務提供者技術文件 v4.8 §39–42 |

---

## 專案結構

```
src/
  NetJwe.Core/          # 核心加解密邏輯
    Interfaces/         # 服務介面
    Services/           # 服務實作
    Models/             # 資料模型
    Exceptions/         # 自訂例外
  NetJwe.Api/           # API 層（供主系統整合）
tests/
  NetJwe.Core.Tests/    # 單元測試
```

---

## 快速開始

```bash
# 建置
dotnet build

# 執行測試
dotnet test
```
