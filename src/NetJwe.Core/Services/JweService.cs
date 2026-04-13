/*
 * ----------------------------------------------
 * JweService — JWE 解密服務（門面）
 * 2026-04-13
 * src/NetJwe.Core/Services/JweService.cs
 * ----------------------------------------------
 */

using System.Text;
using NetJwe.Core.Interfaces;
using NetJwe.Core.Models;

namespace NetJwe.Core.Services;

/// <summary>
/// JWE 解密門面服務，協調 JweParser、JweValidator、JweDecryptor、MyDataPayloadParser 完成完整解密流程
/// </summary>
public class JweService : IJweService
{
    private readonly JweParser _parser;
    private readonly JweValidator _validator;
    private readonly JweDecryptor _decryptor;
    private readonly MyDataPayloadParser _payloadParser;

    internal JweService(
        JweParser parser,
        JweValidator validator,
        JweDecryptor decryptor,
        MyDataPayloadParser payloadParser)
    {
        _parser = parser;
        _validator = validator;
        _decryptor = decryptor;
        _payloadParser = payloadParser;
    }

    /// <inheritdoc />
    public JweDecryptResult Decrypt(string token, string secretKey, string expectedIv)
    {
        // 步驟一：拆解 JWE 五段結構，取得 IV 與 header 進行預先驗證
        var components = _parser.Parse(token);

        // 步驟二：解析 header，判斷演算法模式
        var mode = _parser.ParseHeader(components.Header);

        // 步驟三：驗證 IV 與 myData 管理後臺一致
        _validator.ValidateIv(components.Iv, expectedIv);

        // 步驟四：以 jose-jwt 執行完整解密（含 CEK 解封裝、authentication tag 驗證、ciphertext 解密）
        var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
        var plaintext = mode == JweAlgorithmMode.CbcHs512
            ? _decryptor.DecryptCbc(token, secretKeyBytes)
            : _decryptor.DecryptGcm(token, secretKeyBytes);

        // 步驟五：解析 myData JSON payload
        var payload = _payloadParser.Parse(plaintext);

        // 步驟六：解碼 data 欄位取得 zip 二進位
        var fileBytes = _payloadParser.DecodeData(payload.Data!);

        return new JweDecryptResult(payload.FileName!, fileBytes);
    }
}
