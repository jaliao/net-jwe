/*
 * ----------------------------------------------
 * JweValidatorTests — JweValidator 單元測試
 * 2026-04-13
 * tests/NetJwe.Core.Tests/JweValidatorTests.cs
 * ----------------------------------------------
 */

using System.Text;
using NetJwe.Core.Exceptions;
using NetJwe.Core.Services;

namespace NetJwe.Core.Tests;

/// <summary>
/// JweValidator 的單元測試：IV 驗證、authentication tag 驗證
/// </summary>
public class JweValidatorTests
{
    private readonly JweValidator _validator = new();

    [Fact]
    public void ValidateIv_一致_不拋出例外()
    {
        var expectedIv = "HtzGY7g1hLy5bl9R";
        var jweIv = Encoding.UTF8.GetBytes(expectedIv);

        // 不應拋出例外
        _validator.ValidateIv(jweIv, expectedIv);
    }

    [Fact]
    public void ValidateIv_不一致_拋出JweException()
    {
        var jweIv = Encoding.UTF8.GetBytes("HtzGY7g1hLy5bl9R");
        var expectedIv = "DifferentIvValue!";

        var ex = Assert.Throws<JweException>(() => _validator.ValidateIv(jweIv, expectedIv));
        Assert.Contains("IV", ex.Message);
    }
}
