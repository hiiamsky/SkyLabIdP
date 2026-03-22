using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Domain.Enums;
using System;

namespace SkyLabIdP.Application.SystemApps.Users.Commands.LoginUser.Services;

public class TenantUserServiceFactory : ITenantUserServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantUserServiceFactory(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor)
    {
        _serviceProvider = serviceProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    public IUserService GetCurrentTenantService()
    {
        var tenantId = GetCurrentTenantId();
        return GetServiceByTenantId(tenantId);
    }

    public IUserService GetServiceByTenantId(string tenantId)
    {
        var normalizedTenantId = NormalizeTenantId(tenantId);
        
        var service = _serviceProvider.GetKeyedService<IUserService>(normalizedTenantId);
        
        if (service == null)
        {
            throw new BadHttpRequestException($"Tenant '{normalizedTenantId}' is not a registered tenant.", StatusCodes.Status400BadRequest);
        }
        
        return service;
    }

    private string GetCurrentTenantId()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext?.Items.TryGetValue("Tenant", out var tenantObj) == true && tenantObj is string tenantId)
        {
            return tenantId;
        }

        if (httpContext?.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantHeader) == true && !string.IsNullOrEmpty(tenantHeader))
        {
            return tenantHeader.ToString();
        }

        throw new BadHttpRequestException("Missing or empty X-Tenant-Id.", StatusCodes.Status400BadRequest);
    }

    private string NormalizeTenantId(string tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
        {
            throw new BadHttpRequestException("TenantId cannot be empty.", StatusCodes.Status400BadRequest);
        }

        if (Enum.TryParse<Tenants>(tenantId, true, out var tenantEnum))
        {
            return tenantEnum.ToString();
        }

        throw new BadHttpRequestException($"'{tenantId}' is not a recognized tenant.", StatusCodes.Status400BadRequest);
    }
}
