/*
 * ----------------------------------------------
 * IndexModel — JWE 解密 Demo 頁面 PageModel
 * 2026-04-13
 * src/NetJwe.Api/Pages/Index.cshtml.cs
 * ----------------------------------------------
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetJwe.Core.Exceptions;
using NetJwe.Core.Interfaces;

namespace NetJwe.Api.Pages;

public class IndexModel : PageModel
{
    private const string ZipBytesKey = "ZipBytes";
    private const string FileNameKey = "FileName";

    // demo data — 官方範例參數（數發部技術文件 v4.8 §39）
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

    private readonly IJweService _jweService;

    public IndexModel(IJweService jweService)
    {
        _jweService = jweService;
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
    /// 執行 JWE 解密
    /// </summary>
    public IActionResult OnPostDecrypt()
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

        // 移除 JWE token 中可能存在的空白與換行（來自 PDF 複製貼上）
        var cleanToken = JweToken.Trim().Replace("\n", "").Replace("\r", "").Replace(" ", "");

        try
        {
            var result = _jweService.Decrypt(cleanToken, SecretKey.Trim(), Iv.Trim());

            FileName = result.FileName;
            IsDecrypted = true;

            // TempData 僅支援基本型別，byte[] 需轉為 Base64 字串儲存
            TempData[ZipBytesKey] = Convert.ToBase64String(result.FileBytes);
            TempData[FileNameKey] = result.FileName;
            TempData.Keep(ZipBytesKey);
            TempData.Keep(FileNameKey);
        }
        catch (JweException ex)
        {
            ErrorMessage = ex.Message;
            if (ex.InnerException != null)
                ErrorMessage += $"\n詳細：{ex.InnerException.Message}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"未預期的錯誤：{ex.Message}";
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
}
