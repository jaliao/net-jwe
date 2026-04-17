/*
 * ----------------------------------------------
 * JweServiceTests — JweService 整合測試
 * 2026-04-13 (Updated: 2026-04-17)
 * tests/NetJwe.Core.Tests/JweServiceTests.cs
 * ----------------------------------------------
 */

using NetJwe.Core.Services;

namespace NetJwe.Core.Tests;

/// <summary>
/// JweService 整合測試。
/// </summary>
public class JweServiceTests
{
    private readonly JweService _service = new(
        new JweParser(),
        new JweValidator(),
        new JweDecryptor(),
        new SecretKeyDecryptor(),
        new MyDataPayloadParser());

    [Fact]
    public void Placeholder_測試環境正常()
    {
        Assert.NotNull(_service);
    }

    /// <summary>
    /// 完整兩步驟解密整合測試，使用真實測試資料。
    /// 需設定環境變數（見 testsecrets.sh.example）且 data1.txt 存在時才執行。
    /// </summary>
    [Fact]
    public void DecryptWithEncryptedKey_真實測試資料_成功解密zip()
    {
        if (!TestSecrets.IsConfigured)
            return; // 環境變數未設定，跳過

        var tokenPath = Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "data1.txt");

        if (!File.Exists(tokenPath))
            return; // JWE token 檔案不存在，跳過

        var token = File.ReadAllText(tokenPath).Trim();

        var result = _service.DecryptWithEncryptedKey(
            token,
            TestSecrets.EncryptedSecretKey,
            TestSecrets.ClientSecret,
            TestSecrets.CbcIv);

        Assert.False(string.IsNullOrEmpty(result.FileName));
        Assert.EndsWith(".zip", result.FileName, StringComparison.OrdinalIgnoreCase);
        Assert.NotEmpty(result.FileBytes);
    }
}
