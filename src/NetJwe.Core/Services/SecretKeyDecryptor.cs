/*
 * ----------------------------------------------
 * SecretKeyDecryptor — AES/CBC/PKCS5Padding 解密 encrypted_secret_key
 * 2026-04-17
 * src/NetJwe.Core/Services/SecretKeyDecryptor.cs
 * ----------------------------------------------
 */

using System.Security.Cryptography;
using System.Text;
using NetJwe.Core.Exceptions;

namespace NetJwe.Core.Services;

/// <summary>
/// 解密 myData SP-API 傳來的 encrypted_secret_key。
/// myData 以 AES/CBC/PKCS5Padding 加密真正的 secret_key，
/// 使用 client_secret 重複兩次（32 bytes）作為金鑰，cbc_iv 作為初始向量。
/// </summary>
internal class SecretKeyDecryptor
{
    /// <summary>
    /// 將 base64 編碼的 encrypted_secret_key 解密，取回 32 bytes 的明文 secret_key。
    /// </summary>
    /// <param name="encryptedSecretKeyBase64">myData SP-API 傳來的 secret_key 欄位（base64，64 字元）</param>
    /// <param name="clientSecret">管理後臺的 client_secret（16 字元）</param>
    /// <param name="cbcIv">管理後臺的 cbc iv（16 字元）</param>
    /// <returns>解密後的 secret_key（32 bytes）</returns>
    public byte[] Decrypt(string encryptedSecretKeyBase64, string clientSecret, string cbcIv)
    {
        if (string.IsNullOrEmpty(clientSecret) || clientSecret.Length != 16)
            throw new JweException($"client_secret 長度錯誤：必須為 16 字元，實際為 {clientSecret?.Length ?? 0} 字元");

        if (string.IsNullOrEmpty(cbcIv) || cbcIv.Length != 16)
            throw new JweException($"cbc_iv 長度錯誤：必須為 16 字元，實際為 {cbcIv?.Length ?? 0} 字元");

        byte[] ciphertext;
        try
        {
            ciphertext = Convert.FromBase64String(encryptedSecretKeyBase64);
        }
        catch (FormatException ex)
        {
            throw new JweException("encrypted_secret_key 不是合法的 base64 字串", ex);
        }

        // AES key = client_secret 連接兩次，取 UTF-8 bytes（32 bytes = 256 bits）
        var aesKey = Encoding.UTF8.GetBytes(clientSecret + clientSecret);
        var iv     = Encoding.UTF8.GetBytes(cbcIv);

        try
        {
            using var aes = Aes.Create();
            aes.Key     = aesKey;
            aes.IV      = iv;
            aes.Mode    = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            var plaintext = decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
            return plaintext;
        }
        catch (CryptographicException ex)
        {
            throw new JweException("encrypted_secret_key 解密失敗：client_secret 或 cbc_iv 不正確", ex);
        }
    }
}
