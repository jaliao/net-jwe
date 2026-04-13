/*
 * ----------------------------------------------
 * MyDataPayload — myData 解密後的 JSON payload
 * 2026-04-13
 * src/NetJwe.Core/Models/MyDataPayload.cs
 * ----------------------------------------------
 */

using System.Text.Json.Serialization;

namespace NetJwe.Core.Models;

/// <summary>
/// JWE 解密後的 JSON 內容，對應 myData 平台回傳的 payload 格式
/// </summary>
internal record MyDataPayload(
    [property: JsonPropertyName("filename")] string? FileName,
    [property: JsonPropertyName("data")] string? Data
);
