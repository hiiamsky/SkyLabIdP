using SkyLabIdP.WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 配置環境變數和 Serilog
builder.AddEnvironmentConfiguration()
       .AddSerilogConfiguration();

// 配置核心服務
builder.Services.AddCoreServices(builder.Configuration)
                .AddControllerServices()
                .AddApiDocumentationServices();

// 配置快取服務
builder.Services.AddRedisCache()
                .AddOutputCacheServices();

// 配置健康檢查服務
builder.Services.AddHealthCheckServices();

// 配置可觀測性服務
builder.Services.AddOpenTelemetryObservability();

// 配置安全性服務
builder.Services.AddCorsPolicy(builder.Configuration, builder.Environment)
                .AddCookiePolicy(builder.Environment);

// 配置速率限制服務
builder.Services.AddRateLimitingServices(builder.Configuration);

var app = builder.Build();

// 執行資料庫遷移（DbUp）
app.ApplyDatabaseMigrations();

// 配置中間件管道並運行應用程式
await app.ConfigureMiddlewarePipelineAsync();


// Make the implicit Program class accessible for WebApplicationFactory<Program>
public partial class Program { }
