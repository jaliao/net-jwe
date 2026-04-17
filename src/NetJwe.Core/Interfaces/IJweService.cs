/*
 * ----------------------------------------------
 * IJweService — JWE 解密服務介面
 * 2026-04-13 (Updated: 2026-04-13)
 * src/NetJwe.Core/Interfaces/IJweService.cs
 * ----------------------------------------------
 */

using NetJwe.Core.Models;

namespace NetJwe.Core.Interfaces;

/// <summary>
/// 定義 JWE（JSON Web Encryption）解密操作的標準介面
/// </summary>
public interface IJweService
{
    /// <summary>
    /// 將 myData 平台回傳的 JWE Compact Serialization 字串解密，取得 zip 檔案內容。
    /// 適用於已自行取得明文 secret_key 的情境。
    /// </summary>
    /// <param name="token">JWE Compact Serialization 字串</param>
    /// <param name="secretKey">myData 核發的 secret_key（32 字元 / 32 bytes）</param>
    /// <param name="expectedIv">myData 管理後臺取得的 IV，用於驗證 JWE 中的 IV 一致性</param>
    /// <returns>解密結果，包含 FileName 與 zip 檔案的二進位內容</returns>
    JweDecryptResult Decrypt(string token, string secretKey, string expectedIv);

    /// <summary>
    /// 完整兩步驟解密：先以 client_secret 解密 encrypted_secret_key，再解密 JWE。
    /// 適用於直接使用 myData SP-API 傳來參數的情境。
    /// </summary>
    /// <param name="token">JWE Compact Serialization 字串（MyData-API 回傳）</param>
    /// <param name="encryptedSecretKey">SP-API 傳來的加密 secret_key（base64，64 字元）</param>
    /// <param name="clientSecret">myData 管理後臺的 client_secret（16 字元）</param>
    /// <param name="cbcIv">myData 管理後臺的 cbc iv（16 字元）</param>
    /// <returns>解密結果，包含 FileName 與 zip 檔案的二進位內容</returns>
    JweDecryptResult DecryptWithEncryptedKey(string token, string encryptedSecretKey, string clientSecret, string cbcIv);
}
