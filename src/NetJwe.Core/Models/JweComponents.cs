/*
 * ----------------------------------------------
 * JweComponents — JWE 解析後的五段結構
 * 2026-04-13
 * src/NetJwe.Core/Models/JweComponents.cs
 * ----------------------------------------------
 */

namespace NetJwe.Core.Models;

/// <summary>
/// JWE Compact Serialization 拆解後的五段結構，每段均已 Base64Url 解碼
/// </summary>
public record JweComponents(
    byte[] Header,
    byte[] EncryptedKey,
    byte[] Iv,
    byte[] Ciphertext,
    byte[] AuthenticationTag
);
