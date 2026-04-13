/*
 * ----------------------------------------------
 * MyDataPayloadParserTests — MyDataPayloadParser 單元測試
 * 2026-04-13
 * tests/NetJwe.Core.Tests/MyDataPayloadParserTests.cs
 * ----------------------------------------------
 */

using NetJwe.Core.Exceptions;
using NetJwe.Core.Services;

namespace NetJwe.Core.Tests;

/// <summary>
/// MyDataPayloadParser 的單元測試：JSON 解析、前置碼移除、Base64Url 解碼
/// </summary>
public class MyDataPayloadParserTests
{
    private readonly MyDataPayloadParser _parser = new();

    [Fact]
    public void Parse_合法Payload_成功解析()
    {
        var json = """{"filename":"test.zip","data":"application/zip;data:abc"}""";

        var result = _parser.Parse(json);

        Assert.Equal("test.zip", result.FileName);
        Assert.Equal("application/zip;data:abc", result.Data);
    }

    [Fact]
    public void Parse_缺少filename_拋出JweException()
    {
        var json = """{"data":"application/zip;data:abc"}""";

        var ex = Assert.Throws<JweException>(() => _parser.Parse(json));
        Assert.Contains("filename", ex.Message);
    }

    [Fact]
    public void Parse_非合法JSON_拋出JweException()
    {
        var ex = Assert.Throws<JweException>(() => _parser.Parse("not-json"));
        Assert.NotNull(ex);
    }

    [Fact]
    public void DecodeData_前置碼正確_成功解碼()
    {
        // "hello" 的 Base64Url 編碼
        var data = "application/zip;data:aGVsbG8=";

        var bytes = _parser.DecodeData(data);

        Assert.Equal("hello"u8.ToArray(), bytes);
    }

    [Fact]
    public void DecodeData_前置碼錯誤_拋出JweException()
    {
        var data = "wrong-prefix:abc";

        var ex = Assert.Throws<JweException>(() => _parser.DecodeData(data));
        Assert.Contains("前置碼", ex.Message);
    }

    [Fact]
    public void DecodeData_非合法Base64Url_拋出JweException()
    {
        var data = "application/zip;data:!!!invalid!!!";

        var ex = Assert.Throws<JweException>(() => _parser.DecodeData(data));
        Assert.Contains("解碼失敗", ex.Message);
    }
}
