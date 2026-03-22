using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace SkyLabIdP.Application.SystemApps.Services
{
    /// <summary>
    /// 預設權限服務工廠介面
    /// </summary>
    public interface IDefaultPermissionServiceFactory
    {
        /// <summary>
        /// 根據租戶 ID 獲取對應的預設權限服務
        /// </summary>
        /// <param name="tenantId">租戶 ID</param>
        /// <returns>預設權限服務實例</returns>
        IDefaultPermissionService GetService(string tenantId);
    }

    /// <summary>
    /// 預設權限服務工廠實作
    /// </summary>
    public class DefaultPermissionServiceFactory : IDefaultPermissionServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public DefaultPermissionServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 根據租戶 ID 獲取對應的預設權限服務
        /// </summary>
        /// <param name="tenantId">租戶 ID</param>
        /// <returns>預設權限服務實例</returns>
        public IDefaultPermissionService GetService(string tenantId)
        {
            var service = _serviceProvider.GetKeyedService<IDefaultPermissionService>(tenantId);

            if (service == null)
            {
                throw new BadHttpRequestException(
                    $"Tenant '{tenantId}' does not have a registered default permission service.",
                    StatusCodes.Status400BadRequest);
            }

            return service;
        }
    }
}
