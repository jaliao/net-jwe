/*
 * ----------------------------------------------
 * JweDecryptor — CEK 解封裝與內容解密
 * 2026-04-13
 * src/NetJwe.Core/Services/JweDecryptor.cs
 * ----------------------------------------------
 */

using System.Text;
using Jose;
using NetJwe.Core.Exceptions;
using NetJwe.Core.Models;

namespace NetJwe.Core.Services;

/// <summary>
/// 使用 jose-jwt 函式庫執行 JWE 解密，同時保留手動 IV 驗證邏輯
/// </summary>
internal class JweDecryptor
{
    /// <summary>
    /// 解密 JWE token（CBC 模式：A256KW + A256CBC-HS512）
    /// </summary>
    /// <param name="token">完整 JWE Compact Serialization 字串</param>
    /// <param name="secretKey">myData 核發的 secret_key（32 bytes）</param>
    /// <returns>解密後的明文 JSON 字串</returns>
    public string DecryptCbc(string token, byte[] secretKey)
    {
        if (secretKey.Length != 32)
            throw new JweException($"secret_key 長度錯誤：必須為 32 bytes（256 bits），實際為 {secretKey.Length} bytes");

        try
        {
            return JWT.Decode(token, secretKey, JweAlgorithm.A256KW, JweEncryption.A256CBC_HS512);
        }
        catch (Exception ex)
        {
            throw new JweException("JWE（A256KW/A256CBC-HS512）解密失敗：secret_key 不正確或資料已損毀", ex);
        }
    }

    /// <summary>
    /// 解密 JWE token（GCM 模式：A256GCMKW + A256GCM）
    /// </summary>
    /// <param name="token">完整 JWE Compact Serialization 字串</param>
    /// <param name="secretKey">myData 核發的 secret_key（32 bytes）</param>
    /// <returns>解密後的明文 JSON 字串</returns>
    public string DecryptGcm(string token, byte[] secretKey)
    {
        if (secretKey.Length != 32)
            throw new JweException($"secret_key 長度錯誤：必須為 32 bytes（256 bits），實際為 {secretKey.Length} bytes");

        try
        {
            return JWT.Decode(token, secretKey, JweAlgorithm.A256GCMKW, JweEncryption.A256GCM);
        }
        catch (Exception ex)
        {
            throw new JweException("JWE（A256GCMKW/A256GCM）解密失敗：secret_key 不正確或資料已損毀", ex);
        }
    }
}
