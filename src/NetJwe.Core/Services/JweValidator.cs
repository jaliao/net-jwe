/*
 * ----------------------------------------------
 * JweValidator — JWE IV 與 Authentication Tag 驗證
 * 2026-04-13
 * src/NetJwe.Core/Services/JweValidator.cs
 * ----------------------------------------------
 */

using System.Security.Cryptography;
using System.Text;
using NetJwe.Core.Exceptions;
using NetJwe.Core.Models;

namespace NetJwe.Core.Services;

/// <summary>
/// 負責驗證 JWE 的 IV 一致性與 authentication tag 完整性
/// </summary>
internal class JweValidator
{
    /// <summary>
    /// 驗證 JWE 中的 IV 與 myData 管理後臺提供的 expectedIv 完全一致
    /// </summary>
    public void ValidateIv(byte[] jweIv, string expectedIv)
    {
        // expectedIv 為字串（如 "HtzGY7g1hLy5bl9R"），直接比對 UTF-8 bytes
        var expectedIvBytes = Encoding.UTF8.GetBytes(expectedIv);

        if (!jweIv.SequenceEqual(expectedIvBytes))
            throw new JweException("IV 驗證失敗：JWE 中的 IV 與 myData 管理後臺提供的 IV 不一致");
    }

    /// <summary>
    /// 依 RFC 7516 §5.1 重新計算 authentication tag 並與 JWE 中的 tag 比對（CBC 模式）
    /// </summary>
    /// <remarks>
    /// RFC 7516 CBC 模式的 MAC 計算：
    /// AL = AAD 長度（bits）以 big-endian uint64 表示
    /// MAC_INPUT = AAD || IV || Ciphertext || AL
    /// MAC = HMAC-SHA512(MAC key, MAC_INPUT)
    /// authentication_tag = MAC 的前 256 bits（前 32 bytes）
    /// </remarks>
    public void ValidateAuthTag(JweComponents components, byte[] cek)
    {
        // CEK 前 256 bits 為 MAC key
        var macKey = cek[..32];

        // AAD = 原始 header 的 Base64Url 編碼字串的 ASCII bytes
        // 注意：JWE 規範中 AAD 為 JWE Protected Header 的 Base64Url 字串
        // 此處 components.Header 已解碼，需重新編碼回 Base64Url
        var headerBase64Url = Base64UrlEncode(components.Header);
        var aad = Encoding.ASCII.GetBytes(headerBase64Url);

        // AL = AAD 長度（bits），big-endian uint64
        var aadBitLength = (ulong)aad.Length * 8;
        var al = BitConverter.GetBytes(aadBitLength);
        if (BitConverter.IsLittleEndian) Array.Reverse(al);

        // MAC_INPUT = AAD || IV || Ciphertext || AL
        var macInput = new byte[aad.Length + components.Iv.Length + components.Ciphertext.Length + al.Length];
        var offset = 0;
        aad.CopyTo(macInput, offset); offset += aad.Length;
        components.Iv.CopyTo(macInput, offset); offset += components.Iv.Length;
        components.Ciphertext.CopyTo(macInput, offset); offset += components.Ciphertext.Length;
        al.CopyTo(macInput, offset);

        // 計算 HMAC-SHA512，取前 32 bytes 為 authentication tag
        var hmac = HMACSHA512.HashData(macKey, macInput);
        var computedTag = hmac[..32];

        if (!CryptographicOperations.FixedTimeEquals(computedTag, components.AuthenticationTag))
            throw new JweException("Authentication tag 驗證失敗：JWE 可能遭到篡改");
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
