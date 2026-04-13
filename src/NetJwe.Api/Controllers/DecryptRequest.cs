/*
 * ----------------------------------------------
 * DecryptRequest — POST /api/decrypt 請求 Model
 * 2026-04-13
 * src/NetJwe.Api/Controllers/DecryptRequest.cs
 * ----------------------------------------------
 */

namespace NetJwe.Api.Controllers;

public class DecryptRequest
{
    public string? JweToken { get; set; }
    public string? SecretKey { get; set; }
    public string? Iv { get; set; }
}
