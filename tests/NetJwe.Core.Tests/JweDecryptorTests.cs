/*
 * ----------------------------------------------
 * JweDecryptorTests — JweDecryptor / SecretKeyDecryptor 單元測試
 * 2026-04-13 (Updated: 2026-04-17)
 * tests/NetJwe.Core.Tests/JweDecryptorTests.cs
 * ----------------------------------------------
 */

using System.Text;
using NetJwe.Core.Exceptions;
using NetJwe.Core.Services;

namespace NetJwe.Core.Tests;

/// <summary>
/// JweDecryptor 與 SecretKeyDecryptor 的單元測試
/// </summary>
public class JweDecryptorTests
{
    private readonly JweDecryptor _decryptor = new();

    // 官方範例 JWE
    private const string OfficialJwe =
        "eyJhbGciOiJBMjU2S1ciLCJlbmMiOiJBMjU2Q0JDLUhTNTEyIn0" +
        ".mJQI42l08E3mz6Zac4OlHsNDXxz7g6DoAmJqayHmmEVIUIiNhLMYS5kjWAKPl7LrsFZ0pmdFVqfC77688Mdfni0Xgu4PST" +
        ".SHR6R1k3ZzFoTHk1Ymw5Ug" +
        ".LMz7XIhl2p6FPQwXfHAhb0yZ7YjgjPsLXzR6J96Lxzcz0G3dR5P5_MB_NBQmumD7exefh2GpXjCvwkI277CD5htL7XzJodZLIqOwp1Ymhg" +
        ".C7iWNo6BVCpamm3KlpuPxJYgCkcCh1QcTc8BzDKD3Sw";

    [Fact]
    public void DecryptCbc_secretKey長度錯誤_拋出JweException()
    {
        var shortKey = new byte[16]; // 16 bytes，不是 32

        var ex = Assert.Throws<JweException>(() => _decryptor.DecryptCbc(OfficialJwe, shortKey));
        Assert.Contains("32 bytes", ex.Message);
    }

    [Fact]
    public void DecryptCbc_金鑰錯誤_拋出JweException()
    {
        var wrongKey = new byte[32];
        Random.Shared.NextBytes(wrongKey);

        var ex = Assert.Throws<JweException>(() => _decryptor.DecryptCbc(OfficialJwe, wrongKey));
        Assert.Contains("解密失敗", ex.Message);
    }

    // 注意：OfficialJwe 為 PDF 擷取字串，encrypted_key 段有字元遺失（70 bytes，應為 72 bytes）。
    // 此測試需以完整的真實 myData JWE token 取代後才能啟用。
    // [Fact]
    // public void DecryptCbc_官方範例金鑰_成功解密()
    // {
    //     var key = System.Text.Encoding.UTF8.GetBytes("dgFpgO7FhNF15UJsOB1xmCjwwWw3SO6D");
    //     var result = _decryptor.DecryptCbc(OfficialJwe, key);
    //     Assert.False(string.IsNullOrEmpty(result));
    // }
}

/// <summary>
/// SecretKeyDecryptor 的單元測試。
/// 成功路徑需設定環境變數（見 testsecrets.sh.example），未設定時自動跳過。
/// 失敗路徑使用合成資料，不依賴真實憑證。
/// </summary>
public class SecretKeyDecryptorTests
{
    private readonly SecretKeyDecryptor _decryptor = new();

    // 合成用：任意合法的 AES/CBC 密文（48 bytes base64），供失敗路徑測試使用
    private static readonly string SyntheticEncryptedBlob =
        Convert.ToBase64String(new byte[48]);

    [Fact]
    public void Decrypt_真實測試向量_回傳正確secretKey()
    {
        if (!TestSecrets.IsConfigured)
            return; // 環境變數未設定，跳過（設定方式見 testsecrets.sh.example）

        var result = _decryptor.Decrypt(
            TestSecrets.EncryptedSecretKey,
            TestSecrets.ClientSecret,
            TestSecrets.CbcIv);

        Assert.Equal(32, result.Length);
        Assert.Equal(TestSecrets.ExpectedSecretKey, Encoding.UTF8.GetString(result));
    }

    [Fact]
    public void Decrypt_clientSecret長度錯誤_拋出JweException()
    {
        var ex = Assert.Throws<JweException>(() =>
            _decryptor.Decrypt(SyntheticEncryptedBlob, "tooshort", "1234567890123456"));
        Assert.Contains("client_secret", ex.Message);
    }

    [Fact]
    public void Decrypt_cbcIv長度錯誤_拋出JweException()
    {
        var ex = Assert.Throws<JweException>(() =>
            _decryptor.Decrypt(SyntheticEncryptedBlob, "ValidSecret1234X", "badiv"));
        Assert.Contains("cbc_iv", ex.Message);
    }

    [Fact]
    public void Decrypt_clientSecret錯誤_拋出JweException()
    {
        if (!TestSecrets.IsConfigured)
            return; // 需要真實 encrypted blob 才能觸發 padding 錯誤

        var ex = Assert.Throws<JweException>(() =>
            _decryptor.Decrypt(
                TestSecrets.EncryptedSecretKey,
                "WrongSecret1234X",
                TestSecrets.CbcIv));
        Assert.Contains("解密失敗", ex.Message);
    }
}
