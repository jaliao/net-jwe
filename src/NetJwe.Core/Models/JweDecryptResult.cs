/*
 * ----------------------------------------------
 * JweDecryptResult — JWE 解密結果
 * 2026-04-13
 * src/NetJwe.Core/Models/JweDecryptResult.cs
 * ----------------------------------------------
 */

namespace NetJwe.Core.Models;

/// <summary>
/// JWE 解密完成後的回傳結果，包含檔案名稱與 zip 二進位內容
/// </summary>
public record JweDecryptResult(
    /// <summary>檔案名稱，格式為 {client_id}.zip</summary>
    string FileName,

    /// <summary>zip 檔案的二進位內容（已完成 Base64Url 解碼）</summary>
    byte[] FileBytes
);
