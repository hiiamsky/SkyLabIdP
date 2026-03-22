using System;
using System.Collections.Generic;
using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos.User.Registration;
using SkyLabIdP.Application.SystemApps.Users.Commands.Writers;
using SkyLabIdP.Domain.Entities;
using SkyLabIdP.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace SkyLabIdP.Application.SystemApps.Users.Commands.RegisterUser;

public class UserDetailWriterFactory : IUserDetailWriterFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, Type> _writerTypesMap;

    public UserDetailWriterFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _writerTypesMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            { Tenants.SkyLabmgm.ToString(), typeof(SkyLabDocUserDetailWriter) },
            { Tenants.SkyLabdevelop.ToString(), typeof(SkyLabDevelopUserDetailWriter) },
        };
    }

    public IUserDetailWriter<TRequest, TResponse> GetWriter<TRequest, TResponse>(string tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
        {
            throw new ArgumentNullException(nameof(tenantId), "租戶ID不能為空");
        }

        // 使用不區分大小寫的方式比對租戶 ID
        var normalizedTenantId = tenantId.Trim().ToUpperInvariant();

        if (!_writerTypesMap.TryGetValue(normalizedTenantId, out var writerType))
        {
            throw new NotSupportedException($"找不到租戶 '{tenantId}' 的寫入器");
        }

        // 直接獲取具體的 Writer 實例
        var writer = _serviceProvider.GetRequiredService(writerType);
        
        if (writer is IUserDetailWriter<TRequest, TResponse> typedWriter)
        {
            return typedWriter;
        }
        
        throw new InvalidOperationException($"無法將租戶 '{tenantId}' 的寫入器轉換為 IUserDetailWriter<{typeof(TRequest).Name}, {typeof(TResponse).Name}>");
    }
}
