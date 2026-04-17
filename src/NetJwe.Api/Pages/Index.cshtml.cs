/*
 * ----------------------------------------------
 * IndexModel — JWE 解密 Demo 頁面 PageModel
 * 2026-04-13 (Updated: 2026-04-17)
 * src/NetJwe.Api/Pages/Index.cshtml.cs
 * ----------------------------------------------
 */

using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace NetJwe.Api.Pages;

public class IndexModel : PageModel
{
    private const string ZipBytesKey = "ZipBytes";
    private const string FileNameKey = "FileName";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;

    public IndexModel(
        IHttpClientFactory httpClientFactory,
        IWebHostEnvironment env,
        IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _env = env;
        _config = config;
    }

    [BindProperty]
    public string JweToken { get; set; } = string.Empty;

    [BindProperty]
    public string SecretKey { get; set; } = string.Empty;

    [BindProperty]
    public string ClientSecret { get; set; } = string.Empty;

    [BindProperty]
    public string Iv { get; set; } = string.Empty;

    // 解密結果
    public string? FileName { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsDecrypted { get; set; }

    public void OnGet()
    {
        // 預填 Demo 預設值（來自設定系統，機敏資料不寫死於程式碼）
        SecretKey    = _config["Demo:EncryptedSecretKey"] ?? string.Empty;
        ClientSecret = _config["Demo:ClientSecret"] ?? string.Empty;
        Iv           = _config["Demo:Iv"] ?? string.Empty;

        // 嘗試從 solution 根目錄的 data1.txt 載入 Demo JWE Token
        var data1Path = Path.GetFullPath(
            Path.Combine(_env.ContentRootPath, "..", "..", "data1.txt"));

        if (System.IO.File.Exists(data1Path))
            JweToken = System.IO.File.ReadAllText(data1Path).Trim();
    }

    /// <summary>
    /// 透過 POST /api/decrypt 執行 JWE 解密
    /// </summary>
    public async Task<IActionResult> OnPostDecryptAsync()
    {
        // 清除上次結果
        TempData.Remove(ZipBytesKey);
        TempData.Remove(FileNameKey);

        if (string.IsNullOrWhiteSpace(JweToken) ||
            string.IsNullOrWhiteSpace(SecretKey) ||
            string.IsNullOrWhiteSpace(Iv))
        {
            ErrorMessage = "請填寫所有欄位（JWE Token、Secret Key、IV）";
            return Page();
        }

        var apiUrl = $"{Request.Scheme}://{Request.Host}/api/decrypt";
        var client = _httpClientFactory.CreateClient();

        try
        {
            var response = await client.PostAsJsonAsync(apiUrl, new
            {
                jweToken = JweToken,
                secretKey = SecretKey,
                clientSecret = string.IsNullOrWhiteSpace(ClientSecret) ? null : ClientSecret,
                iv = Iv
            });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<DecryptApiResponse>();
                if (result is null)
                {
                    ErrorMessage = "API 回傳格式異常";
                    return Page();
                }

                FileName = result.Filename;
                IsDecrypted = true;

                // TempData 儲存 Base64 字串（API 已回傳 Base64，直接存入）
                TempData[ZipBytesKey] = result.ZipBase64;
                TempData[FileNameKey] = result.Filename;
                TempData.Keep(ZipBytesKey);
                TempData.Keep(FileNameKey);
            }
            else
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorApiResponse>();
                ErrorMessage = error?.Error ?? "解密失敗（未知錯誤）";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"呼叫 API 失敗：{ex.Message}";
        }

        return Page();
    }

    /// <summary>
    /// 下載解密後的 zip 檔案
    /// </summary>
    public IActionResult OnPostDownload()
    {
        TempData.Keep(ZipBytesKey);
        TempData.Keep(FileNameKey);

        var zipBase64 = TempData[ZipBytesKey] as string;
        var fileName = TempData[FileNameKey] as string ?? "download.zip";

        if (string.IsNullOrEmpty(zipBase64))
        {
            ErrorMessage = "找不到可下載的檔案，請重新執行解密";
            return Page();
        }

        var zipBytes = Convert.FromBase64String(zipBase64);
        if (zipBytes.Length == 0)
        {
            ErrorMessage = "找不到可下載的檔案，請重新執行解密";
            return Page();
        }

        return File(zipBytes, "application/zip", fileName);
    }

    private record DecryptApiResponse(string Filename, string ZipBase64);
    private record ErrorApiResponse(string Error);
}
