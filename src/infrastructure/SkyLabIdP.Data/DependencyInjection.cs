using System.Data;
using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Data.Contexts;
using SkyLabIdP.Data.Extensions;
using SkyLabIdP.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace SkyLabIdP.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureData(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                x => x.UseNetTopologySuite()
            )
        );
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // 🔧 註冊 Dapper 用 IDbConnection
        services.AddScoped<IDbConnection>(_ =>
            new SqlConnection(configuration.GetConnectionString("DefaultConnection")));

        // 🔧 註冊權限提供者服務
        services.AddScoped<IPermissionProvider, PermissionProvider>();
        services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
        
        services.AddScoped<IAuditLogService, AuditLogService>(); // 註冊審計日誌服務
        services.AddScoped<IUnitOfWork, UnitOfWork>(); // 註冊 UnitOfWork
        services.AddIdentityServices();
        return services;

    }

}
