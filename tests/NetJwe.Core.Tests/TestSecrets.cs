/*
 * ----------------------------------------------
 * TestSecrets — 測試用機敏參數（環境變數讀取）
 * 2026-04-17
 * tests/NetJwe.Core.Tests/TestSecrets.cs
 * ----------------------------------------------
 */

namespace NetJwe.Core.Tests;

/// <summary>
/// 從環境變數讀取測試用機敏參數。
/// 本機開發時請複製 testsecrets.sh.example 為 testsecrets.sh，填入實際值後執行。
/// CI/CD 環境請在 pipeline secrets 中設定對應環境變數。
/// </summary>
internal static class TestSecrets
{
    /// <summary>SP-API 傳來的 encrypted_secret_key（base64，64 字元）</summary>
    public static string EncryptedSecretKey =>
        Environment.GetEnvironmentVariable("NETJWE_TEST_ENCRYPTED_SECRET_KEY") ?? string.Empty;

    /// <summary>myData 管理後臺的 client_secret（16 字元）</summary>
    public static string ClientSecret =>
        Environment.GetEnvironmentVariable("NETJWE_TEST_CLIENT_SECRET") ?? string.Empty;

    /// <summary>myData 管理後臺的 cbc_iv（16 字元）</summary>
    public static string CbcIv =>
        Environment.GetEnvironmentVariable("NETJWE_TEST_CBC_IV") ?? string.Empty;

    /// <summary>解密後的明文 secret_key（32 字元）</summary>
    public static string ExpectedSecretKey =>
        Environment.GetEnvironmentVariable("NETJWE_TEST_EXPECTED_SECRET_KEY") ?? string.Empty;

    /// <summary>環境變數是否已完整設定</summary>
    public static bool IsConfigured =>
        !string.IsNullOrEmpty(EncryptedSecretKey) &&
        !string.IsNullOrEmpty(ClientSecret) &&
        !string.IsNullOrEmpty(CbcIv) &&
        !string.IsNullOrEmpty(ExpectedSecretKey);
}
