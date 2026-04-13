/*
 * ----------------------------------------------
 * IJweService — JWE 加解密服務介面
 * 2026-04-13
 * src/NetJwe.Core/Interfaces/IJweService.cs
 * ----------------------------------------------
 */

namespace NetJwe.Core.Interfaces;

/// <summary>
/// 定義 JWE（JSON Web Encryption）加解密操作的標準介面
/// </summary>
public interface IJweService
{
    /// <summary>
    /// 將明文資料加密為 JWE 緊湊序列化字串
    /// </summary>
    /// <param name="plaintext">待加密的明文字串</param>
    /// <returns>JWE 緊湊序列化字串</returns>
    string Encrypt(string plaintext);

    /// <summary>
    /// 將 JWE 緊湊序列化字串解密為明文
    /// </summary>
    /// <param name="token">JWE 緊湊序列化字串</param>
    /// <returns>解密後的明文字串</returns>
    string Decrypt(string token);
}
