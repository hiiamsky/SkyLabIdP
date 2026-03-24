using Asp.Versioning.ApiExplorer;
using SkyLabIdP.Identity.Helpers;
using SkyLabIdP.WebApi.Helpers.Middleware;
using Serilog;

namespace SkyLabIdP.WebApi.Extensions
{
    /// <summary>
    /// 中間件相關的擴展方法
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// 配置開發環境中間件
        /// </summary>
        /// <param name="app">IApplicationBuilder</param>
        /// <param name="apiVersionDescriptionProvider">IApiVersionDescriptionProvider</param>
        /// <returns>更新後的應用程式建構器</returns>
        public static IApplicationBuilder UseDevelopmentMiddleware(this IApplicationBuilder app, IApiVersionDescriptionProvider apiVersionDescriptionProvider)
        {
            app.UseDeveloperExceptionPage();
            app.UseSwaggerExtension(apiVersionDescriptionProvider);
            return app;
        }

        /// <summary>
        /// 配置生產環境安全中間件
        /// </summary>
        /// <param name="app">IApplicationBuilder</param>
        /// <returns>更新後的應用程式建構器</returns>
        public static IApplicationBuilder UseProductionSecurityMiddleware(this IApplicationBuilder app)
        {
            app.UseHsts();
            return app;
        }

        /// <summary>
        /// 配置基本中間件管道
        /// </summary>
        /// <param name="app">IApplicationBuilder</param>
        /// <returns>更新後的應用程式建構器</returns>
        public static IApplicationBuilder UseBasicMiddleware(this IApplicationBuilder app)
        {
            app.UseHttpsRedirection();
            app.UseRouting();
            return app;
        }

        /// <summary>
        /// 配置安全性和快取中間件
        /// </summary>
        /// <param name="app">IApplicationBuilder</param>
        /// <returns>更新後的應用程式建構器</returns>
        public static IApplicationBuilder UseSecurityAndCacheMiddleware(this IApplicationBuilder app)
        {
            app.UseRateLimiter();
            app.UseCors("AllowCorsWebSites");
            app.UseOutputCache();
            app.UseResponseCaching();
            return app;
        }

        /// <summary>
        /// 配置自定義中間件
        /// </summary>
        /// <param name="app">IApplicationBuilder</param>
        /// <returns>更新後的應用程式建構器</returns>
        public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<ApiKeyMiddleware>();
            app.UseMiddleware<RequestLoggingMiddleware>();
            app.UseMiddleware<AuditLoggingMiddleware>(); // 添加審計日誌中間件
            return app;
        }

        /// <summary>
        /// 配置身份認證和授權中間件
        /// </summary>
        /// <param name="app">IApplicationBuilder</param>
        /// <returns>更新後的應用程式建構器</returns>
        public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder app)
        {
            app.UseCookiePolicy();
            app.UseMiddleware<TenantMiddleware>();
            // 順序很重要：先是身份驗證，然後是令牌驗證中間件
            app.UseAuthentication();
            app.UseMiddleware<TokenValidationMiddleware>();
            app.UseAuthorization();
            app.UseMiddleware<ForbiddenMiddleware>();
            return app;
        }

        /// <summary>
        /// 配置完整的中間件管道
        /// </summary>
        /// <param name="app">WebApplication</param>
        /// <returns>配置完成的應用程式</returns>
        public static async Task<WebApplication> ConfigureMiddlewarePipelineAsync(this WebApplication app)
        {
            try
            {
                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseDevelopmentMiddleware(app.Services.GetRequiredService<IApiVersionDescriptionProvider>());
                }

                if (!app.Environment.IsDevelopment())
                {
                    app.UseProductionSecurityMiddleware();
                }

                app.UseBasicMiddleware()
                   .UseSecurityAndCacheMiddleware()
                   .UseCustomMiddleware()
                   .UseAuthenticationMiddleware();

                app.MapHealthCheckEndpoints();
                app.MapControllers();  // 使用頂層路由註冊
                
                await app.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application failed to start");
                throw new InvalidOperationException("An error occurred while starting the application.", null);
            }
            finally
            {
                await Log.CloseAndFlushAsync();
            }

            return app;
        }
    }
}
