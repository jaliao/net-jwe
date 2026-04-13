/*
 * ----------------------------------------------
 * MyDataPayloadParser — myData JSON Payload 解析器
 * 2026-04-13
 * src/NetJwe.Core/Services/MyDataPayloadParser.cs
 * ----------------------------------------------
 */

using System.Text.Json;
using NetJwe.Core.Exceptions;
using NetJwe.Core.Models;

namespace NetJwe.Core.Services;

/// <summary>
/// 負責解析 JWE 解密後的 myData JSON payload，並對 data 欄位進行 Base64Url 解碼
/// </summary>
internal class MyDataPayloadParser
{
    private const string DataPrefix = "application/zip;data:";

    /// <summary>
    /// 將解密後的 JSON 字串解析為 MyDataPayload
    /// </summary>
    public MyDataPayload Parse(string json)
    {
        try
        {
            var payload = JsonSerializer.Deserialize<MyDataPayload>(json);

            if (payload is null || string.IsNullOrEmpty(payload.FileName) || string.IsNullOrEmpty(payload.Data))
                throw new JweException("payload 格式不符合 myData 規範：缺少 filename 或 data 欄位");

            return payload;
        }
        catch (JweException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new JweException("解密內容非預期的 JSON 格式", ex);
        }
    }

    /// <summary>
    /// 去除 data 欄位前置碼並進行 Base64Url 解碼，回傳 zip 二進位內容
    /// </summary>
    public byte[] DecodeData(string data)
    {
        if (!data.StartsWith(DataPrefix, StringComparison.Ordinal))
            throw new JweException($"data 前置碼格式不符預期：必須以 \"{DataPrefix}\" 開頭");

        var base64Url = data[DataPrefix.Length..];

        try
        {
            return Base64UrlDecode(base64Url);
        }
        catch (Exception ex)
        {
            throw new JweException("data 欄位 Base64Url 解碼失敗", ex);
        }
    }

    private static byte[] Base64UrlDecode(string base64Url)
    {
        var base64 = base64Url
            .Replace('-', '+')
            .Replace('_', '/');

        var padding = base64.Length % 4;
        if (padding > 0)
            base64 += new string('=', 4 - padding);

        return Convert.FromBase64String(base64);
    }
}
