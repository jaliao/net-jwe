/*
 * ----------------------------------------------
 * JweDecryptorTests — JweDecryptor 單元測試
 * 2026-04-13
 * tests/NetJwe.Core.Tests/JweDecryptorTests.cs
 * ----------------------------------------------
 */

using NetJwe.Core.Exceptions;
using NetJwe.Core.Services;

namespace NetJwe.Core.Tests;

/// <summary>
/// JweDecryptor 的單元測試：secretKey 長度驗證、錯誤金鑰解密失敗
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
