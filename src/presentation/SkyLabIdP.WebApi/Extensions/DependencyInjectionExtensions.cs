using SkyLabIdP.Application;
using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Data;
using SkyLabIdP.Identity;
using SkyLabIdP.Shared;
using SkyLabIdP.WebApi.Filters;
using SkyLabIdP.WebApi.Helpers;
using SkyLabIdP.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SkyLabIdP.WebApi.Extensions
{
    /// <summary>
    /// 依賴注入相關的擴展方法
    /// </summary>
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// 添加核心服務依賴注入
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <param name="configuration">IConfiguration</param>
        /// <returns>更新後的服務集合</returns>
        public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();
            
            // 添加應用層服務
            services.AddApplication();

            // 添加基礎設施層服務
            services.AddInfrastructureData(configuration);
            services.AddInfrastructureShared(configuration);
            services.AddInfrastructureIdentity(configuration);

            // 註冊 Serilog 審計日誌服務
            services.AddScoped<IAuditLogService, SerilogAuditLogService>();

            return services;
        }

        /// <summary>
        /// 添加控制器相關服務
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <returns>更新後的服務集合</returns>
        public static IServiceCollection AddControllerServices(this IServiceCollection services)
        {
            services.AddControllers();

            // 修改這一行，使用全局過濾器註冊的方式，讓依賴注入系統處理參數
            services.AddControllersWithViews(options =>
            {
                // 使用類型而不是實例，讓 DI 容器處理依賴注入
                options.Filters.Add<ApiExceptionFilterAttribute>();
            });

            services.Configure<ApiBehaviorOptions>(options =>
                options.SuppressModelStateInvalidFilter = true
            );

            return services;
        }

        /// <summary>
        /// 添加 API 文檔相關服務
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <returns>更新後的服務集合</returns>
        public static IServiceCollection AddApiDocumentationServices(this IServiceCollection services)
        {
            services.AddApiVersioningExtension();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGenExtension();
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

            return services;
        }
    }
}
