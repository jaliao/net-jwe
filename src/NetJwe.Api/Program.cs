/*
 * ----------------------------------------------
 * Program — NetJwe.Api 啟動程式
 * 2026-04-13
 * src/NetJwe.Api/Program.cs
 * ----------------------------------------------
 */

using NetJwe.Core;

var builder = WebApplication.CreateBuilder(args);

// 註冊 Controllers（REST API）
builder.Services.AddControllers();

// 註冊 Razor Pages + TempData（Session）
builder.Services.AddRazorPages().AddSessionStateTempDataProvider();
builder.Services.AddSession();

// 註冊 JWE 解密服務
builder.Services.AddNetJwe();

// HttpClient 供 Demo 頁面串接自身 API
builder.Services.AddHttpClient();

var app = builder.Build();

app.UseSession();
app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.MapRazorPages();

app.Run();
