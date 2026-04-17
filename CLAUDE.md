# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

**連江縣政府 myData 加解密**（專案代號：`net-jwe`）

以 .NET 8 開發的 JWE（JSON Web Encryption）加解密函式庫，提供 myData 資料交換所需的加解密能力。

### 目錄結構

```
src/
  NetJwe.Core/          # 核心加解密邏輯 (class library)
    Interfaces/         # 服務介面定義
    Services/           # 服務實作
      SecretKeyDecryptor.cs   # Step 1：AES/CBC 解密 encrypted_secret_key
      JweDecryptor.cs         # Step 2：JWE 解封裝與解密
      JweService.cs           # 門面服務（整合兩步驟流程）
    Models/             # 資料模型
    Exceptions/         # 自訂例外
  NetJwe.Api/           # Web API + Demo UI
    Controllers/        # POST /api/decrypt
    Pages/              # Demo UI（Razor Pages）
tests/
  NetJwe.Core.Tests/    # xUnit 單元測試
```

### 常用指令

```bash
dotnet build                    # 建置整個 solution
dotnet test                     # 執行所有測試
dotnet test --filter "方法名稱"  # 執行單一測試
dotnet run --project src/NetJwe.Api
```

### 程式碼規範

**檔案開頭必須加入標準註解**，格式如下：

```csharp
/*
 * ----------------------------------------------
 * ComponentName — 元件說明
 * 2026-XX-XX (Updated: 2026-XX-XX)
 * path/to/file.cs
 * ----------------------------------------------
 */
```

- 所有註解使用**繁體中文**
- XML 文件註解（`<summary>`、`<param>`、`<returns>`）也使用繁體中文

## Development Workflow (OpenSpec)

This project uses the `openspec` CLI to manage changes through a spec-driven workflow. Changes live in `openspec/changes/<name>/` and follow artifact sequences defined by the schema in `openspec/config.yaml` (currently `spec-driven`).

**Key slash commands** (via `.claude/commands/opsx/`):

| Command | Purpose |
|---|---|
| `/opsx new` | Start a new change (creates scaffolding, shows first artifact) |
| `/opsx continue` | Create the next artifact for an in-progress change |
| `/opsx ff` | Fast-forward: create all artifacts for a change at once |
| `/opsx apply` | Implement tasks from a change |
| `/opsx verify` | Verify implementation matches change artifacts |
| `/opsx archive` | Archive a completed change |
| `/opsx explore` | Thinking-partner mode for exploring ideas before starting a change |

**Common openspec CLI commands:**

```bash
openspec new change "<kebab-case-name>"       # scaffold a new change
openspec status --change "<name>"             # see artifact progress
openspec instructions <artifact-id> --change "<name>"  # get artifact template
openspec schemas --json                        # list available workflow schemas
```

The `spec-driven` schema sequences artifacts from proposal → tasks → implementation → verification. Always check `openspec status` to know which artifact is next.
