/*
 * ----------------------------------------------
 * Program — NetJwe.Api 啟動程式
 * 2026-04-13
 * src/NetJwe.Api/Program.cs
 * ----------------------------------------------
 */

using NetJwe.Core;

var builder = WebApplication.CreateBuilder(args);

// 註冊 Razor Pages
builder.Services.AddRazorPages();

// 註冊 JWE 解密服務
builder.Services.AddNetJwe();

// TempData 使用 Cookie（儲存 zip bytes）
builder.Services.AddRazorPages().AddSessionStateTempDataProvider();
builder.Services.AddSession();

var app = builder.Build();

app.UseSession();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

app.Run();
