/*
 * ----------------------------------------------
 * DecryptController — JWE 解密 REST API
 * 2026-04-13
 * src/NetJwe.Api/Controllers/DecryptController.cs
 * ----------------------------------------------
 */

using Microsoft.AspNetCore.Mvc;
using NetJwe.Core.Exceptions;
using NetJwe.Core.Interfaces;

namespace NetJwe.Api.Controllers;

[ApiController]
[Route("api/decrypt")]
public class DecryptController : ControllerBase
{
    private readonly IJweService _jweService;

    public DecryptController(IJweService jweService)
    {
        _jweService = jweService;
    }

    /// <summary>
    /// 解密 JWE token，回傳 filename 與 Base64 編碼的 zip 內容。
    /// 若提供 clientSecret，啟用兩步驟模式（先解密 encrypted_secret_key，再解密 JWE）；
    /// 否則以 secretKey 直接解密（單步模式，向下相容）。
    /// </summary>
    [HttpPost]
    public IActionResult Decrypt([FromBody] DecryptRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.JweToken) ||
            string.IsNullOrWhiteSpace(request.SecretKey) ||
            string.IsNullOrWhiteSpace(request.Iv))
        {
            return BadRequest(new { error = "請提供所有必填欄位：jweToken、secretKey、iv" });
        }

        // 清除 token 中可能因 PDF 複製產生的空白與換行
        var cleanToken = request.JweToken.Trim()
            .Replace("\n", "").Replace("\r", "").Replace(" ", "");

        try
        {
            var useFullFlow = !string.IsNullOrWhiteSpace(request.ClientSecret);

            var result = useFullFlow
                ? _jweService.DecryptWithEncryptedKey(
                    cleanToken,
                    request.SecretKey.Trim(),
                    request.ClientSecret!.Trim(),
                    request.Iv.Trim())
                : _jweService.Decrypt(cleanToken, request.SecretKey.Trim(), request.Iv.Trim());

            return Ok(new
            {
                filename = result.FileName,
                zipBase64 = Convert.ToBase64String(result.FileBytes)
            });
        }
        catch (JweException ex)
        {
            var message = ex.InnerException != null
                ? $"{ex.Message}\n詳細：{ex.InnerException.Message}"
                : ex.Message;
            return BadRequest(new { error = message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = $"未預期的錯誤：{ex.Message}" });
        }
    }
}
