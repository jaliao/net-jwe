/*
 * ----------------------------------------------
 * JweParserTests — JweParser 單元測試
 * 2026-04-13
 * tests/NetJwe.Core.Tests/JweParserTests.cs
 * ----------------------------------------------
 */

using NetJwe.Core.Exceptions;
using NetJwe.Core.Models;
using NetJwe.Core.Services;

namespace NetJwe.Core.Tests;

/// <summary>
/// JweParser 的單元測試：拆解、Base64Url 解碼、header 演算法判斷
/// </summary>
public class JweParserTests
{
    // 官方範例 JWE（數發部技術文件 v4.8 §39）
    private const string OfficialJwe =
        "eyJhbGciOiJBMjU2S1ciLCJlbmMiOiJBMjU2Q0JDLUhTNTEyIn0" +
        ".mJQI42l08E3mz6Zac4OlHsNDXxz7g6DoAmJqayHmmEVIUIiNhLMYS5kjWAKPl7LrsFZ0pmdFVqfC77688Mdfni0Xgu4PST" +
        ".SHR6R1k3ZzFoTHk1Ymw5Ug" +
        ".LMz7XIhl2p6FPQwXfHAhb0yZ7YjgjPsLXzR6J96Lxzcz0G3dR5P5_MB_NBQmumD7exefh2GpXjCvwkI277CD5htL7XzJodZLIqOwp1Ymhg" +
        ".C7iWNo6BVCpamm3KlpuPxJYgCkcCh1QcTc8BzDKD3Sw";

    private readonly JweParser _parser = new();

    [Fact]
    public void Parse_官方範例_成功拆解五段()
    {
        var result = _parser.Parse(OfficialJwe);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Header);
        Assert.NotEmpty(result.EncryptedKey);
        Assert.NotEmpty(result.Iv);
        Assert.NotEmpty(result.Ciphertext);
        Assert.NotEmpty(result.AuthenticationTag);
    }

    [Fact]
    public void Parse_段數不足_拋出JweException()
    {
        var token = "aaa.bbb.ccc.ddd"; // 只有四段

        var ex = Assert.Throws<JweException>(() => _parser.Parse(token));
        Assert.Contains("五段", ex.Message);
    }

    [Fact]
    public void Parse_非合法Base64Url_拋出JweException並標明段名()
    {
        var token = "!!!.bbb.ccc.ddd.eee"; // header 段無效

        var ex = Assert.Throws<JweException>(() => _parser.Parse(token));
        Assert.Contains("header", ex.Message);
    }

    [Fact]
    public void ParseHeader_CBC模式_回傳CbcHs512()
    {
        var components = _parser.Parse(OfficialJwe);
        var mode = _parser.ParseHeader(components.Header);

        Assert.Equal(JweAlgorithmMode.CbcHs512, mode);
    }

    [Fact]
    public void ParseHeader_不支援的演算法_拋出JweException()
    {
        // 建立一個 alg=RS256 的 header（不支援）
        var headerJson = """{"alg":"RS256","enc":"A128CBC-HS256"}""";
        var headerBytes = System.Text.Encoding.UTF8.GetBytes(headerJson);

        var ex = Assert.Throws<JweException>(() => _parser.ParseHeader(headerBytes));
        Assert.Contains("不支援", ex.Message);
    }
}
