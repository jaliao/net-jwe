/*
 * ----------------------------------------------
 * JweParser — JWE Compact Serialization 解析器
 * 2026-04-13
 * src/NetJwe.Core/Services/JweParser.cs
 * ----------------------------------------------
 */

using System.Text;
using System.Text.Json;
using NetJwe.Core.Exceptions;
using NetJwe.Core.Models;

namespace NetJwe.Core.Services;

/// <summary>
/// 負責拆解 JWE Compact Serialization 字串並解析 header 演算法模式
/// </summary>
internal class JweParser
{
    private static readonly string[] SegmentNames = ["header", "encrypted_key", "iv", "ciphertext", "authentication_tag"];

    /// <summary>
    /// 將 JWE Compact Serialization 字串拆解為五段並 Base64Url 解碼
    /// </summary>
    public JweComponents Parse(string token)
    {
        var parts = token.Split('.');
        if (parts.Length != 5)
            throw new JweException($"JWE 格式錯誤：預期五段，實際為 {parts.Length} 段");

        var decoded = new byte[5][];
        for (int i = 0; i < 5; i++)
        {
            try
            {
                decoded[i] = Base64UrlDecode(parts[i]);
            }
            catch (Exception ex)
            {
                throw new JweException($"JWE 第 {i + 1} 段（{SegmentNames[i]}）Base64Url 解碼失敗", ex);
            }
        }

        return new JweComponents(decoded[0], decoded[1], decoded[2], decoded[3], decoded[4]);
    }

    /// <summary>
    /// 解析 header 取得演算法模式
    /// </summary>
    public JweAlgorithmMode ParseHeader(byte[] headerBytes)
    {
        var json = Encoding.UTF8.GetString(headerBytes);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var alg = root.TryGetProperty("alg", out var algProp) ? algProp.GetString() : null;
        var enc = root.TryGetProperty("enc", out var encProp) ? encProp.GetString() : null;

        return (alg, enc) switch
        {
            ("A256KW", "A256CBC-HS512") => JweAlgorithmMode.CbcHs512,
            ("A256GCMKW", "A256GCM") => JweAlgorithmMode.Gcm,
            _ => throw new JweException($"不支援的演算法組合：alg={alg}, enc={enc}")
        };
    }

    private static byte[] Base64UrlDecode(string base64Url)
    {
        // 將 Base64Url 轉換為標準 Base64
        var base64 = base64Url
            .Replace('-', '+')
            .Replace('_', '/');

        // 補齊 padding
        var padding = base64.Length % 4;
        if (padding > 0)
            base64 += new string('=', 4 - padding);

        return Convert.FromBase64String(base64);
    }
}
