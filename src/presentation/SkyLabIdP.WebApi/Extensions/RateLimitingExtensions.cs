using Serilog;
using System.Globalization;
using System.Net;
using System.Threading.RateLimiting;

namespace SkyLabIdP.WebApi.Extensions
{
    /// <summary>
    /// 速率限制相關的擴展方法
    /// </summary>
    public static class RateLimitingExtensions
    {
        /// <summary>
        /// 添加速率限制服務
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <param name="configuration">IConfiguration</param>
        /// <returns>更新後的服務集合</returns>
        public static IServiceCollection AddRateLimitingServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 從配置中讀取限流設定
            var rateLimitingConfig = configuration.GetSection("RateLimiting");
            var shortTermPermitLimit = rateLimitingConfig.GetValue<int>("ShortTerm:PermitLimit");
            var shortTermWindowSeconds = rateLimitingConfig.GetValue<int>("ShortTerm:WindowSeconds");
            var longTermPermitLimit = rateLimitingConfig.GetValue<int>("LongTerm:PermitLimit");
            var longTermWindowSeconds = rateLimitingConfig.GetValue<int>("LongTerm:WindowSeconds");
            var authPermitLimit = rateLimitingConfig.GetValue<int>("Authentication:PermitLimit", 5);
            var authWindowSeconds = rateLimitingConfig.GetValue<int>("Authentication:WindowSeconds", 60);
            var refreshTokenPermitLimit = rateLimitingConfig.GetValue<int>("RefreshToken:PermitLimit", 5);
            var refreshTokenWindowSeconds = rateLimitingConfig.GetValue<int>("RefreshToken:WindowSeconds", 60);
            var fileUploadPermitLimit = rateLimitingConfig.GetValue<int>("FileUpload:PermitLimit", 10);
            var fileUploadWindowSeconds = rateLimitingConfig.GetValue<int>("FileUpload:WindowSeconds", 60);

            services.AddRateLimiter(_ =>
            {
                // 設置當請求被拒絕時的處理方式
                _.OnRejected = (context, _) =>
                {
                    // 取得 IP 地址
                    var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString();
                    // 取得已登入的用戶名，如果沒有登入則使用 "Anonymous"
                    var loginUserName = context.HttpContext.User.Identity?.Name ?? "Anonymous";
                    // 設定動作名稱
                    var memberName = "RateLimit";

                    // 如果被拒絕的請求包含 RetryAfter 元數據，則在響應頭中添加 RetryAfter 標頭
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        context.HttpContext.Response.Headers.RetryAfter =
                            ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
                    }

                    // 設置響應狀態碼為 429 (Too Many Requests)
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    var rejectionMessage = "Too many requests. Please try again later.";
                    // 回應一條消息給使用者，告知請求過多
                    context.HttpContext.Response.WriteAsync(rejectionMessage);

                    // 記錄日誌，包含 IP 地址、創建時間、已登入用戶名和動作名稱等上下文信息
                    var log = Log.ForContext("IP", ipAddress)
                                 .ForContext("CreationTime", DateTime.UtcNow)
                                 .ForContext("LoggedInUser", loginUserName)
                                 .ForContext("Action", memberName);

                    // 記錄警告日誌，包含拒絕消息
                    log.Warning("Request was rate limited. {RejectionMessage}", rejectionMessage);

                    return new ValueTask();
                };

                // 設置全局速率限制器，使用鏈式限制器
                _.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                    // 第一部分：短期速率限制器
                    PartitionedRateLimiter.Create<HttpContext, IPAddress>(httpContext =>
                    {
                        var ipAddress = httpContext.Connection.RemoteIpAddress ?? IPAddress.None;
                        return RateLimitPartition.GetFixedWindowLimiter(ipAddress!, _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = shortTermPermitLimit,
                            Window = TimeSpan.FromSeconds(shortTermWindowSeconds)
                        });
                    }),
                    // 第二部分：長期速率限制器
                    PartitionedRateLimiter.Create<HttpContext, IPAddress>(httpContext =>
                    {
                        // 根據使用者的 User-Agent 創建限制器
                        var ipAddress = httpContext.Connection.RemoteIpAddress ?? IPAddress.None;
                        return RateLimitPartition.GetFixedWindowLimiter
                        (ipAddress!, _ =>
                            new FixedWindowRateLimiterOptions
                            {
                                AutoReplenishment = true, // 自動補充允許次數
                                PermitLimit = longTermPermitLimit, // 設定長期內允許的請求數量
                                Window = TimeSpan.FromSeconds(longTermWindowSeconds) // 設定長期窗口的時間長度
                            });
                    }));
                
                // 為身份驗證端點（登入/註冊）添加專門的速率限制策略
                _.AddPolicy("AuthenticationPolicy", httpContext =>
                {
                    var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
                    return RateLimitPartition.GetFixedWindowLimiter(ipAddress, _ => 
                        new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = authPermitLimit,
                            Window = TimeSpan.FromSeconds(authWindowSeconds)
                        });
                });
                
                // 為 Refresh Token 端點添加專門的速率限制策略
                _.AddPolicy("RefreshTokenPolicy", httpContext =>
                {
                    var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
                    return RateLimitPartition.GetFixedWindowLimiter(ipAddress, _ => 
                        new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = refreshTokenPermitLimit,
                            Window = TimeSpan.FromSeconds(refreshTokenWindowSeconds)
                        });
                });
                
                // 為文件上傳端點添加專門的速率限制策略
                _.AddPolicy("FileUploadPolicy", httpContext =>
                {
                    var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
                    return RateLimitPartition.GetFixedWindowLimiter(ipAddress, _ => 
                        new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = fileUploadPermitLimit,
                            Window = TimeSpan.FromSeconds(fileUploadWindowSeconds)
                        });
                });
            });

            return services;
        }
    }
}
