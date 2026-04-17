## Why

Demo UI 的預設測試憑證（`encrypted_secret_key`、`client_secret`、`iv`）目前以 `private const string` 硬編碼在 `Index.cshtml.cs`，
會隨原始碼一同提交，違反機敏資料不入版控的原則。打包前需改以 .NET Core 設定系統管理，確保正式環境不暴露測試憑證。

## What Changes

- 將 Demo 預設值從程式碼常數移至 `appsettings.json`（空值佔位）+ `appsettings.Development.json`（本機填值，已在 `.gitignore`）
- `IndexModel` 改以 `IConfiguration` 注入讀取設定值，移除所有 `private const string Demo*` 常數
- 新增 `appsettings.Development.json` 範本說明（`appsettings.Development.json.example`），供新進成員參考

## Capabilities

### New Capabilities
- （無）

### Modified Capabilities
- `decrypt-ui`：Demo 預設值來源由硬編碼改為設定系統（IConfiguration），行為規格不變

## Impact

- `src/NetJwe.Api/appsettings.json` — 新增 `Demo` 設定節（空值）
- `src/NetJwe.Api/appsettings.Development.json` — 新建（gitignored，存放本機測試憑證）
- `src/NetJwe.Api/appsettings.Development.json.example` — 新建（committed，供參考）
- `src/NetJwe.Api/Pages/Index.cshtml.cs` — 移除常數、改注入 `IConfiguration`
