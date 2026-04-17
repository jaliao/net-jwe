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
    /// 完整兩步驟解密整合測試，使用 2026-04-17 真實測試資料。
    /// JWE token 以 data1.txt 載入，避免 inline 過長。
    /// </summary>
    [Fact]
    public void DecryptWithEncryptedKey_真實測試資料_成功解密zip()
    {
        var tokenPath = Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "data1.txt");

        if (!File.Exists(tokenPath))
            return; // 測試資料不在 CI 環境，跳過

        var token = File.ReadAllText(tokenPath).Trim();

        const string encryptedSecretKey = "oWgZzJHnJ8Vkty+YlkldC7TT8aQr7jcQUXlpmEvCESSQqDHoEA+ueZXF6Bm8L8tn";
        const string clientSecret       = "8rtR3mtWlTynOigI";
        const string cbcIv              = "RjiCdd8OgJcTYgZr";

        var result = _service.DecryptWithEncryptedKey(token, encryptedSecretKey, clientSecret, cbcIv);

        Assert.False(string.IsNullOrEmpty(result.FileName));
        Assert.EndsWith(".zip", result.FileName, StringComparison.OrdinalIgnoreCase);
        Assert.NotEmpty(result.FileBytes);
    }
}
