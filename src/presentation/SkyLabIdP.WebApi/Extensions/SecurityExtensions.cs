namespace SkyLabIdP.WebApi.Extensions
{
    /// <summary>
    /// 安全性相關的擴展方法
    /// </summary>
    public static class SecurityExtensions
    {
        /// <summary>
        /// 添加 CORS 政策
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <param name="configuration">IConfiguration</param>
        /// <returns>更新後的服務集合</returns>
        public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            var allowCorsWebSites = configuration.GetSection("AllowCorsWebSites").Get<string[]>();
            
            // 生產環境安全驗證：不允許 localhost
            if (!environment.IsDevelopment() && allowCorsWebSites?.Any(origin => 
                origin.Contains("localhost", StringComparison.OrdinalIgnoreCase)) == true)
            {
                throw new InvalidOperationException(
                    "生產環境的 CORS 配置不可包含 localhost。請在 appsettings.Production.json 中設定正確的來源。");
            }
            
            services.AddCors(options =>
            {
                options.AddPolicy("AllowCorsWebSites",
                    builder =>
                    {
                        if (allowCorsWebSites != null && allowCorsWebSites.Length > 0)
                        {
                            builder.WithOrigins(allowCorsWebSites)
                                   // 限制允許的 Headers，不使用 AllowAnyHeader()
                                   .WithHeaders(
                                       "Content-Type", 
                                       "Authorization", 
                                       "X-Api-Key", 
                                       "X-Tenant-Id",
                                       "Accept",
                                       "Origin")
                                   // 限制允許的 Methods，不使用 AllowAnyMethod()
                                   .WithMethods(
                                       "GET", 
                                       "POST", 
                                       "PUT", 
                                       "DELETE", 
                                       "OPTIONS")
                                   .AllowCredentials() // 允許請求攜帶身份驗證信息，如 cookie
                                   .SetPreflightMaxAge(TimeSpan.FromHours(24)); // 預檢請求快取 24 小時
                        }
                    });
            });

            return services;
        }

        /// <summary>
        /// 添加 Cookie 政策配置
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <param name="environment">IWebHostEnvironment</param>
        /// <returns>更新後的服務集合</returns>
        public static IServiceCollection AddCookiePolicy(this IServiceCollection services, IWebHostEnvironment environment)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // Cookie 安全性設定：始終使用最安全的配置
                options.Secure = CookieSecurePolicy.Always; // 所有環境都強制 HTTPS

                // 設置 HttpOnly 以增加 cookie 的保護，防止跨站腳本攻擊(XSS)
                options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;

                // 使用 Strict SameSite 策略以最大程度防止 CSRF 攻擊
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
                
                // 在添加 Cookie 時確保安全屬性
                options.OnAppendCookie = cookieContext =>
                {
                    cookieContext.CookieOptions.HttpOnly = true;
                    cookieContext.CookieOptions.Secure = true;
                    cookieContext.CookieOptions.SameSite = SameSiteMode.Strict;
                };
            });

            return services;
        }
    }
}
