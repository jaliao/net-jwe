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
/// SecretKeyDecryptor 的單元測試，使用 2026-04-17 真實測試資料作為測試向量
/// </summary>
public class SecretKeyDecryptorTests
{
    private readonly SecretKeyDecryptor _decryptor = new();

    // 真實測試向量（2026-04-17 連江縣政府測試資料）
    private const string EncryptedSecretKey = "oWgZzJHnJ8Vkty+YlkldC7TT8aQr7jcQUXlpmEvCESSQqDHoEA+ueZXF6Bm8L8tn";
    private const string ClientSecret       = "8rtR3mtWlTynOigI";
    private const string CbcIv              = "RjiCdd8OgJcTYgZr";
    private const string ExpectedSecretKey  = "edf4933bc95e483ebdf29d30c4c751f7";

    [Fact]
    public void Decrypt_真實測試向量_回傳正確secretKey()
    {
        var result = _decryptor.Decrypt(EncryptedSecretKey, ClientSecret, CbcIv);

        Assert.Equal(32, result.Length);
        Assert.Equal(ExpectedSecretKey, Encoding.UTF8.GetString(result));
    }

    [Fact]
    public void Decrypt_clientSecret長度錯誤_拋出JweException()
    {
        var ex = Assert.Throws<JweException>(() =>
            _decryptor.Decrypt(EncryptedSecretKey, "tooshort", CbcIv));
        Assert.Contains("client_secret", ex.Message);
    }

    [Fact]
    public void Decrypt_cbcIv長度錯誤_拋出JweException()
    {
        var ex = Assert.Throws<JweException>(() =>
            _decryptor.Decrypt(EncryptedSecretKey, ClientSecret, "badiv"));
        Assert.Contains("cbc_iv", ex.Message);
    }

    [Fact]
    public void Decrypt_clientSecret錯誤_拋出JweException()
    {
        var ex = Assert.Throws<JweException>(() =>
            _decryptor.Decrypt(EncryptedSecretKey, "WrongSecret1234X", CbcIv));
        Assert.Contains("解密失敗", ex.Message);
    }
}
