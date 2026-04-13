/*
 * ----------------------------------------------
 * JweAlgorithmMode — JWE 演算法模式列舉
 * 2026-04-13
 * src/NetJwe.Core/Models/JweAlgorithmMode.cs
 * ----------------------------------------------
 */

namespace NetJwe.Core.Models;

/// <summary>
/// myData 支援的 JWE 演算法模式
/// </summary>
public enum JweAlgorithmMode
{
    /// <summary>
    /// A256KW + A256CBC-HS512（預設模式）
    /// </summary>
    CbcHs512,

    /// <summary>
    /// A256GCMKW + A256GCM（GCM 模式，管理介面選「AES/GCM」時使用）
    /// </summary>
    Gcm
}
