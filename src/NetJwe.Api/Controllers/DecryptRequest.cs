/*
 * ----------------------------------------------
 * DecryptRequest — POST /api/decrypt 請求 Model
 * 2026-04-13 (Updated: 2026-04-17)
 * src/NetJwe.Api/Controllers/DecryptRequest.cs
 * ----------------------------------------------
 */

namespace NetJwe.Api.Controllers;

public class DecryptRequest
{
    public string? JweToken { get; set; }

    /// <summary>
    /// 兩步驟模式：SP-API 傳來的 encrypted_secret_key（base64，64 字元）。
    /// 單步模式：直接提供明文 secret_key（32 字元）。
    /// </summary>
    public string? SecretKey { get; set; }

    public string? Iv { get; set; }

    /// <summary>
    /// 選填。提供時啟用兩步驟模式：以 client_secret + cbc_iv 先解密 SecretKey，再解密 JWE。
    /// </summary>
    public string? ClientSecret { get; set; }
}
