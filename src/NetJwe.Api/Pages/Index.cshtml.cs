/*
 * ----------------------------------------------
 * IndexModel — JWE 解密 Demo 頁面 PageModel
 * 2026-04-13 (Updated: 2026-04-13)
 * src/NetJwe.Api/Pages/Index.cshtml.cs
 * ----------------------------------------------
 */

using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NetJwe.Api.Pages;

public class IndexModel : PageModel
{
    private const string ZipBytesKey = "ZipBytes";
    private const string FileNameKey = "FileName";

    // Demo token 使用官方範例參數（secret_key + IV）在本地產生，payload 為空 zip（demo.zip）
    private const string DemoToken =
        "eyJhbGciOiJBMjU2S1ciLCJlbmMiOiJBMjU2Q0JDLUhTNTEyIn0" +
        ".EY4fcoqReDN9TXc43YHZQzmbSICst8mIFeSmsPlVnDH3sYgFDscNZzoTTvRUJi622ELCSAJrFl8uAYId6wbiJFW4LLadwUbO" +
        ".SHR6R1k3ZzFoTHk1Ymw5Ug" +
        ".b_2NwSsqRIHUC-IYQqh-x_ZMcVen_fVDhRNXlYFc5FBCsSFVdQd9Dh2TR0GZ5Ey4azzljD0" +
        "V6E3qwUIli77OmNLDAuLmSV4KEKGhNQz6WqU1SxgmCIUlbBE-uofLV1pD" +
        ".VzN5r3a7VIC5Gw4YUmbaasgw6PKeJIf_JhZlfGOEi9I";

    private const string DemoSecretKey = "dgFpgO7FhNF15UJsOB1xmCjwwWw3SO6D";
    private const string DemoIv = "HtzGY7g1hLy5bl9R";

    private readonly IHttpClientFactory _httpClientFactory;

    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public string JweToken { get; set; } = DemoToken;

    [BindProperty]
    public string SecretKey { get; set; } = DemoSecretKey;

    [BindProperty]
    public string Iv { get; set; } = DemoIv;

    // 解密結果
    public string? FileName { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsDecrypted { get; set; }

    public void OnGet() { }

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
