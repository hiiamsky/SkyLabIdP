using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StackExchange.Redis;

namespace SkyLabIdP.WebApi.Extensions;

/// <summary>
/// OpenTelemetry 可觀測性配置擴展方法
/// </summary>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// 添加 OpenTelemetry Tracing 和 Metrics，使用 OTLP Exporter 匯出
    /// </summary>
    public static IServiceCollection AddOpenTelemetryObservability(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: "SkyLabIdP"))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSqlClientInstrumentation(options =>
                {
                    options.SetDbStatementForText = true;
                })
                .AddRedisInstrumentation()
                .AddOtlpExporter())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter());

        return services;
    }
}
