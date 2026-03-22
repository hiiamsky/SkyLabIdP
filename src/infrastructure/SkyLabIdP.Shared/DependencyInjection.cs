using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Domain.Settings;
using SkyLabIdP.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SkyLabIdP.Shared
{
    public static class DependencyInjection
    {
        /// <summary>
        /// Adds the shared infrastructure services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="config">The <see cref="IConfiguration"/> instance used to configure the services.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddInfrastructureShared(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<MailSettings>(config.GetSection("MailSettings"));
            services.Configure<LoginNotificationSettings>(config.GetSection("LoginNotification"));

            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<ILoginNotificationService, LoginNotificationService>();

            services.AddScoped<FileService>();
            services.AddScoped<SaltGenerator>();
            services.AddScoped<CaptchaService>();

            services.AddScoped<IFileService>(provider =>
                new Lazy<IFileService>(() => provider.GetRequiredService<FileService>()).Value);

            services.AddScoped<ISaltGenerator>(provider =>
                new Lazy<ISaltGenerator>(() => provider.GetRequiredService<SaltGenerator>()).Value);
               
            services.AddScoped<ICaptchaService>(provider =>
                new Lazy<ICaptchaService>(() => provider.GetRequiredService<CaptchaService>()).Value);
            services.AddSingleton<IDateService, DateService>();
            
            // Register the Redis token storage service
            services.AddScoped<ITokenStorageService, RedisTokenStorageService>();

            // 添加以下代碼
            services.AddHttpClient<ITikaService, TikaService>();

            // 註冊資料保護服務
            services.AddScoped<IDataProtectionService, DataProtectionService>();


            return services;
        }
    }
}