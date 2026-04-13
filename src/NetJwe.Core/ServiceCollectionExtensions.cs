/*
 * ----------------------------------------------
 * ServiceCollectionExtensions — DI 擴充方法
 * 2026-04-13
 * src/NetJwe.Core/ServiceCollectionExtensions.cs
 * ----------------------------------------------
 */

using Microsoft.Extensions.DependencyInjection;
using NetJwe.Core.Interfaces;
using NetJwe.Core.Services;

namespace NetJwe.Core;

/// <summary>
/// IServiceCollection 擴充方法，提供 NetJwe.Core 的 DI 註冊
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 註冊 JWE 解密服務至 DI 容器
    /// </summary>
    public static IServiceCollection AddNetJwe(this IServiceCollection services)
    {
        services.AddSingleton<IJweService>(_ =>
            new JweService(
                new JweParser(),
                new JweValidator(),
                new JweDecryptor(),
                new MyDataPayloadParser()));
        return services;
    }
}
