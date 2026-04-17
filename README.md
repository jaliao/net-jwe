# net-jwe — 連江縣政府 myData 加解密

## 專案簡介

本專案為連江縣政府線上申辦平台（[eservice.matsu.gov.tw](https://eservice.matsu.gov.tw)）與數位發展部 myData 中間平台串接所需的 JWE（JSON Web Encryption）加解密函式庫。

使用 .NET 8 開發，以 class library 形式提供，供主系統直接引用。

---

## 背景說明

數發部 myData 平台扮演資料中介角色，彙整各政府機關的資料集（如戶口謄本、國民身分證等），讓民眾在辦理申請業務時，無需自行備齊紙本證明，由服務單位透過 myData 平台向各機關索取所需資料。

連江縣政府作為「服務提供者」，向 myData 平台發出資料請求後，平台回傳的資料內容以 JWE 格式加密。本專案的任務是將這份加密資料正確解密，取出實際內容。

---

## 完整解密流程

myData 的解密分兩步驟（依據數發部服務提供者技術文件 v4.8 §33–42）：

```
Step 1：取得明文 secret_key
  SP-API 通知 → SP 收到 encrypted_secret_key（base64，64 字元）
  AES/CBC/PKCS5Padding 解密：
    key = (client_secret + client_secret).UTF8  ← 32 bytes / 256 bits
    iv  = cbc_iv（管理後臺查詢，16 字元）
  → 明文 secret_key（32 bytes，英數字含大小寫）

Step 2：解密 JWE
  MyData-API 回傳 JWE Compact Serialization
  使用 secret_key + A256KW 解封裝 CEK（64 bytes）
  使用 CEK（後 32 bytes）+ IV，以 AES-256-CBC 解密 ciphertext
  → JSON { "filename": "xxx.zip", "data": "application/zip;data:<Base64>" }
  → Base64 解碼後得 zip 檔案
```

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
| `encrypted_key` | 以 `secret_key` 透過 A256KW（AESWrap）封裝後的 CEK |
| `initialization_vector` | AES-CBC 初始向量，**必須與 myData 管理後臺取得的 IV 相同** |
| `ciphertext` | 加密後的內容，驗證 `authentication_tag` 後才進行解密 |
| `authentication_tag` | 依 JWE 規範生成，用於確認 JWE 未被篡改 |

**CEK 結構（A256CBC-HS512 模式）：** 512 bits（64 bytes），前 256 bits 為 MAC key，後 256 bits 為 AES key。

---

## API 使用說明

### POST /api/decrypt

**兩步驟模式**（推薦，適用 SP-API 原始參數）：

```json
{
  "jweToken":     "<JWE Compact Serialization>",
  "secretKey":    "<encrypted_secret_key，SP-API 傳來的 base64，64 字元>",
  "clientSecret": "<管理後臺的 client_secret，16 字元>",
  "iv":           "<管理後臺的 cbc iv，16 字元>"
}
```

**單步模式**（向下相容，適用已取得明文 secret_key 的情境）：

```json
{
  "jweToken":  "<JWE Compact Serialization>",
  "secretKey": "<明文 secret_key，32 字元>",
  "iv":        "<管理後臺的 cbc iv，16 字元>"
}
```

**成功回應（200 OK）：**

```json
{
  "filename": "xxx.zip",
  "zipBase64": "<Base64 編碼的 zip 內容>"
}
```

**失敗回應（400 Bad Request）：**

```json
{
  "error": "<錯誤說明>"
}
```

### `IJweService` 介面（函式庫直接引用）

```csharp
// 兩步驟模式（SP-API 原始參數）
JweDecryptResult DecryptWithEncryptedKey(
    string token,
    string encryptedSecretKey,  // SP-API 傳來的 base64 密文
    string clientSecret,         // 管理後臺 16 字元
    string cbcIv                 // 管理後臺 16 字元
);

// 單步模式（已有明文 secret_key）
JweDecryptResult Decrypt(
    string token,
    string secretKey,    // 32 字元明文
    string expectedIv    // 16 字元
);
```

---

## 測試工具頁面（Demo UI）

提供一個獨立的測試頁面，供開發與驗收階段手動驗證解密功能：

### 頁面輸入欄位

| 欄位 | 說明 |
|---|---|
| JWE Token | 完整 JWE Compact Serialization（自動載入 `data1.txt`，若存在） |
| Secret Key | SP-API 傳來的 `encrypted_secret_key`（base64，64 字元）；或明文 secret_key（32 字元，Client Secret 留空時） |
| Client Secret | myData 管理後臺的 `client_secret`（16 字元，提供時啟用兩步驟模式） |
| IV | myData 管理後臺的 `cbc iv`（16 字元） |

### 操作流程

1. 填入上述欄位（預設值已帶入測試環境資料）
2. 按下「**開始解密**」按鈕
3. 頁面顯示解密結果：
   - 成功：顯示 `filename` 與「下載 zip」按鈕
   - 失敗：顯示具體錯誤訊息

---

## 技術規格

| 項目 | 規格 |
|---|---|
| 執行環境 | .NET 8 |
| 專案類型 | Class Library + Web API |
| 加密標準 | JWE（RFC 7516）、AES/CBC/PKCS5Padding |
| 參考文件 | 數發部服務提供者技術文件 v4.8 |

---

## 專案結構

```
src/
  NetJwe.Core/          # 核心加解密邏輯
    Interfaces/         # 服務介面
    Services/           # 服務實作
      SecretKeyDecryptor.cs   # Step 1：解密 encrypted_secret_key
      JweDecryptor.cs         # Step 2：JWE 解密
      JweService.cs           # 門面服務（整合兩步驟流程）
    Models/             # 資料模型
    Exceptions/         # 自訂例外
  NetJwe.Api/           # Web API + Demo UI
    Controllers/        # POST /api/decrypt
    Pages/              # Demo UI（Razor Pages）
tests/
  NetJwe.Core.Tests/    # xUnit 單元測試
```

---

## 快速開始

### 1. 建置

```bash
dotnet build
```

### 2. 設定 Demo UI 憑證

Demo 頁面的預設值透過設定系統注入，不寫死於程式碼。本機開發時需建立
`appsettings.Development.json`（已在 `.gitignore`，不會入版控）：

```bash
cp src/NetJwe.Api/appsettings.Development.json.example \
   src/NetJwe.Api/appsettings.Development.json
```

開啟 `appsettings.Development.json`，填入實際的測試環境憑證：

```json
{
  "Demo": {
    "EncryptedSecretKey": "<SP-API 傳來的 encrypted_secret_key，base64，64 字元>",
    "ClientSecret": "<myData 管理後臺的 client_secret，16 字元>",
    "Iv": "<myData 管理後臺的 cbc iv，16 字元>"
  }
}
```

> 生產環境改用環境變數（.NET Core 自動對應）：
> ```bash
> Demo__ClientSecret=xxxx
> Demo__EncryptedSecretKey=xxxx
> Demo__Iv=xxxx
> ```

### 3. 啟動 Demo UI

```bash
dotnet run --project src/NetJwe.Api
# 開啟 https://localhost:5001
```

### 4. 執行測試

```bash
dotnet test
```

需要憑證的整合測試（成功路徑）在環境變數未設定時會自動跳過。
如需執行完整測試，請先設定環境變數：

```bash
# 複製範本並填入實際值
cp tests/NetJwe.Core.Tests/testsecrets.sh.example \
   tests/NetJwe.Core.Tests/testsecrets.sh

# 編輯 testsecrets.sh，填入：
# NETJWE_TEST_ENCRYPTED_SECRET_KEY
# NETJWE_TEST_CLIENT_SECRET
# NETJWE_TEST_CBC_IV
# NETJWE_TEST_EXPECTED_SECRET_KEY

source tests/NetJwe.Core.Tests/testsecrets.sh
dotnet test
```
