## Context

連江縣政府線上申辦平台已建立 .NET 8 solution（`NetJwe.sln`），需在 `NetJwe.Core` 實作 JWE 解密功能。myData 平台依照數發部服務提供者技術文件 v4.8 §39–42，以 JWE Compact Serialization 格式回傳加密資料，SP 系統需以 myData 核發的 `secret_key` 進行解密。

現有骨架：`IJweService` 介面、`JweException` 自訂例外，尚無實作。

## Goals / Non-Goals

**Goals:**
- 實作完整的 JWE Compact Serialization 解密流程（解析→驗證→解封裝→解密→解析 payload）
- 支援 `A256KW + A256CBC-HS512`（預設模式）與 `A256GCMKW + A256GCM`（GCM 模式）
- IV 驗證：確認 JWE 中的 IV 與 myData 管理後臺提供的 IV 一致
- Authentication tag 驗證：解密前確認 JWE 未被篡改
- 以 `IJweService` 介面對外暴露，支援 DI 注入

**Non-Goals:**
- 不實作 JWE 加密（僅需解密）
- 不處理網路請求（下載檔案由呼叫端負責）
- 不實作 UI 或 API 層（屬 `cr-spec-260413-002` 範疇）

## Decisions

### 決策一：優先使用 `System.Security.Cryptography`，搭配 `jose-jwt`

**選項：**
- **A. 純 `System.Security.Cryptography`**：.NET 內建，無外部相依，但 AES Key Wrap（RFC 3394）在 .NET 8 已內建支援（`AesKeyWrap`），AES-CBC 與 AES-GCM 亦有對應 API。
- **B. `jose-jwt` NuGet 套件**：高階抽象，幾行程式即可完成解密，但黑盒子難以精確控制 IV 驗證邏輯。
- **C. `Microsoft.IdentityModel.Tokens`**：主要針對 JWT，JWE 支援較有限。

**決定：選 A。** .NET 8 的 `System.Security.Cryptography` 已完整支援所需演算法（`AesKeyWrap`、`Aes`、`AesGcm`），不引入外部套件可降低相依複雜度，也便於精確實作 myData 規範要求的 IV 驗證步驟。

### 決策二：`JweService` 單一服務類別，內部依 header 自動切換模式

解密時先解析 header 的 `alg` 欄位，自動判斷使用 CBC 或 GCM 路徑，呼叫端無需傳入模式參數，降低使用複雜度。

### 決策三：解密流程分三個內部步驟，各自可獨立測試

```
JweParser      → 拆解 Compact Serialization、Base64Url 解碼
JweValidator   → IV 比對、authentication tag 驗證
JweDecryptor   → AES Key Unwrap + AES-CBC/GCM 解密
```

`JweService` 作為門面（Facade）協調上述三者，`IJweService` 對外只暴露 `Decrypt()`。

### 決策四：回傳 `JweDecryptResult` 值物件，不直接回傳 byte array

```csharp
public record JweDecryptResult(string FileName, byte[] FileBytes);
```

封裝 `filename` 與解碼後的 zip 二進位，方便後續擴充（如加入 `ContentType`）。

## Risks / Trade-offs

- **AES-GCM 在 .NET 8 的限制**：`AesGcm` 在部分平台（如 Android）有限制，但本專案部署於 Windows Server，無影響。
- **IV 驗證依賴外部傳入值**：`Decrypt()` 需接收 myData 管理後臺的 `expectedIv` 參數，若呼叫端未正確傳入將導致驗證永遠失敗 → 文件中明確說明此參數為必填。
- **Base64Url vs Base64**：myData 使用 Base64Url（無 padding），需確認使用 `Base64UrlEncoder` 而非標準 `Convert.FromBase64String`，否則解碼會失敗。

## 官方測試參數（數發部技術文件 v4.8 §39 範例）

單元測試與驗收測試使用以下官方範例參數：

| 參數 | 值 |
|---|---|
| `secret_key` | `dgFpgO7FhNF15UJsOB1xmCjwwWw3SO6D` |
| `IV` | `HtzGY7g1hLy5bl9R` |
| 演算法模式 | `A256KW + A256CBC-HS512`（預設） |

對應的完整 JWE 範例字串（跳行符號需移除後使用）：

```
eyJhbGciOiJBMjU2S1ciLCJlbmMiOiJBMjU2Q0JDLUhTNTEyIn0.mJQI42l08E3mz6Zac4OlHsNDXxz7g6DoAmJqayHmmEVIUIiNhLMYS5kjWAKPl7LrsFZ0pmdFVqfC77688Mdfni0Xgu4PST.SHR6R1k3ZzFoTHk1Ymw5Ug.LMz7XIhl2p6FPQwXfHAhb0yZ7YjgjPsLXzR6J96Lxzcz0G3dR5P5_MB_NBQmumD7exefh2GpXjCvwkI277CD5htL7XzJodZLIqOwp1Ymhg.C7iWNo6BVCpamm3KlpuPxJYgCkcCh1QcTc8BzDKD3Sw
```

## Open Questions

- `A256GCMKW` 模式下，header 中包含 `iv` 與 `tag` 欄位（Key Wrap 時使用），需確認數發部文件是否有額外驗證要求。
- `secret_key` 儲存方式（純字串 vs `byte[]`）由呼叫端決定，服務介面接受 `byte[]` 或 `string`？→ 待與 Mark 確認主系統的傳入形式。
