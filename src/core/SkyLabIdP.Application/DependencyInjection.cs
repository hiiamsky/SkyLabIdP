﻿using SkyLabIdP.Application.Common.Behaviors;
using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Common.Security;
using SkyLabIdP.Application.SystemApps.Services;

using SkyLabIdP.Application.SystemApps.Services.ServiceSettings;
using SkyLabIdP.Domain.Entities;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using SkyLabIdP.Application.SystemApps.Users.Commands.LoginUser.Services;
using SkyLabIdP.Application.SystemApps.Users.Commands.RegisterUser;
using SkyLabIdP.Application.SystemApps.Users.Commands.Writers;
using SkyLabIdP.Application.Dtos.User.Registration;
using SkyLabIdP.Application.Dtos.SkyLabDocUserDetail;
using SkyLabIdP.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace SkyLabIdP.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddSingleton(SkyLabIdPMapper.Instance);
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            services.AddMediator(options =>
            {
                options.ServiceLifetime = ServiceLifetime.Transient;
            });

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));

            // Add Data Protection Services
            services.AddDataProtection();

            // 🔒 註冊 URL 白名單驗證器 (SSRF 防護)
            services.AddSingleton<IUrlWhitelistValidator, UrlWhitelistValidator>();

            services.AddScoped<LoginUserInfoServiceSettings>(provider => new LoginUserInfoServiceSettings
            {
                UnitOfWork = provider.GetRequiredService<IUnitOfWork>(),
                UserManager = provider.GetRequiredService<UserManager<ApplicationUser>>(),
                Configuration = provider.GetRequiredService<IConfiguration>(),
                Dataprotectionservice = provider.GetRequiredService<IDataProtectionService>(),
                JwtService = provider.GetRequiredService<IJwtService>(),
                Logger = provider.GetRequiredService<ILogger<AbstractLoginUserInfoService>>(),
                EmailService = provider.GetRequiredService<IEmailService>(),
                LoginNotificationService = provider.GetRequiredService<ILoginNotificationService>(),
                SaltGenerator = provider.GetRequiredService<ISaltGenerator>(),
                Mapper = provider.GetRequiredService<SkyLabIdPMapper>(),
                CaptchaService = provider.GetRequiredService<ICaptchaService>(),
                Cache = provider.GetRequiredService<IDistributedCache>(),
                TokenStorageService = provider.GetRequiredService<ITokenStorageService>()
            });

            services.AddScoped<ExternalLoginServiceSettings>(provider => new ExternalLoginServiceSettings
            {
                UserManager = provider.GetRequiredService<UserManager<ApplicationUser>>(),
                Context = provider.GetRequiredService<IApplicationDbContext>(),
                JwtService = provider.GetRequiredService<IJwtService>(),
                TenantUserServiceFactory = provider.GetRequiredService<ITenantUserServiceFactory>(),
                DataProtectionService = provider.GetRequiredService<IDataProtectionService>(),
                LoginNotificationService = provider.GetRequiredService<ILoginNotificationService>(),
                TokenStorageService = provider.GetRequiredService<ITokenStorageService>(),
                Logger = provider.GetRequiredService<ILogger<ExternalLoginServiceSettings>>()
            });

            // 註冊 ITenantUserServiceFactory
            services.AddScoped<ITenantUserServiceFactory, TenantUserServiceFactory>();

            // 註冊所有租戶的 Keyed Service
            services.AddKeyedScoped<IUserService, SkyLabMgmLoginUserInfoService>(nameof(Tenants.SkyLabmgm));
            services.AddKeyedScoped<IUserService, SkyLabDevelopLoginUserInfoService>(nameof(Tenants.SkyLabdevelop));
            // 註冊 UserDetailWriter 相關服務
            services.AddScoped<IUserDetailWriterFactory, UserDetailWriterFactory>();

            // 註冊具體實現類而非泛型接口
            services.AddScoped<SkyLabDocUserDetailWriter>();
            services.AddScoped<SkyLabDevelopUserDetailWriter>();
            // 註冊預設權限服務工廠
            services.AddScoped<IDefaultPermissionServiceFactory, DefaultPermissionServiceFactory>();
            
            // 註冊所有租戶的專用權限服務實作
            services.AddScoped<DefaultPermissionService>();
            services.AddScoped<SkyLabMgmDefaultPermissionService>();
            services.AddScoped<SkyLabDevelopDefaultPermissionService>();
                        // 註册 Keyed IDefaultPermissionService（每個租戶對應一個實作）
            services.AddKeyedScoped<IDefaultPermissionService, SkyLabMgmDefaultPermissionService>(nameof(Tenants.SkyLabmgm));
            services.AddKeyedScoped<IDefaultPermissionService, SkyLabDevelopDefaultPermissionService>(nameof(Tenants.SkyLabdevelop));
                        // 註冊預設權限服務（用於向後相容）
            services.AddScoped<IDefaultPermissionService, DefaultPermissionService>();


            return services;
        }
    }
}
