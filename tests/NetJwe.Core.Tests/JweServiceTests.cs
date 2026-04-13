/*
 * ----------------------------------------------
 * JweServiceTests — JweService 整合測試
 * 2026-04-13
 * tests/NetJwe.Core.Tests/JweServiceTests.cs
 * ----------------------------------------------
 */

using NetJwe.Core.Services;

namespace NetJwe.Core.Tests;

/// <summary>
/// JweService 整合測試。
/// 注意：數發部技術文件 v4.8 §39 的 JWE 範例字串為 PDF 擷取文字，
/// encrypted_key 段有字元遺失（70 bytes，應為 72 bytes），
/// 無法直接使用。整合驗收應使用 Mark 提供的真實 myData JWE token。
/// </summary>
public class JweServiceTests
{
    private readonly JweService _service = new(
        new JweParser(),
        new JweValidator(),
        new JweDecryptor(),
        new MyDataPayloadParser());

    [Fact]
    public void Placeholder_測試環境正常()
    {
        // 確認測試框架與服務建構正常
        Assert.NotNull(_service);
    }

    // TODO: 取得 Mark 提供的真實 myData JWE token 後，加入以下整合測試：
    // [Fact]
    // public void Decrypt_真實myData_成功解密()
    // {
    //     const string token = "<真實 JWE token>";
    //     const string secretKey = "<myData 管理後臺的 secret_key>";
    //     const string expectedIv = "<myData 管理後臺的 IV>";
    //
    //     var result = _service.Decrypt(token, secretKey, expectedIv);
    //
    //     Assert.False(string.IsNullOrEmpty(result.FileName));
    //     Assert.EndsWith(".zip", result.FileName, StringComparison.OrdinalIgnoreCase);
    //     Assert.NotEmpty(result.FileBytes);
    // }
}
