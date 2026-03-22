using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.SystemApps.Services;
using SkyLabIdP.Application.SystemApps.Users.Commands.LoginUser.Services;
using SkyLabIdP.Domain.Enums;

namespace Application.UnitTests.SystemApps.Services;

/// <summary>
/// 測試 TenantUserServiceFactory 及 DefaultPermissionServiceFactory
/// 未知租戶 → BadHttpRequestException (HTTP 400)
/// </summary>
public class TenantServiceFactoryTests
{
    // ─── TenantUserServiceFactory ───────────────────────────────────────────

    [Theory]
    [InlineData(nameof(Tenants.SkyLabmgm))]
    [InlineData(nameof(Tenants.SkyLabdevelop))]
    public void GetServiceByTenantId_KnownTenant_ReturnsService(string tenantId)
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        var services = new ServiceCollection();
        services.AddKeyedScoped<IUserService>(tenantId, (_, _) => mockUserService.Object);
        var serviceProvider = services.BuildServiceProvider();

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        var factory = new TenantUserServiceFactory(serviceProvider, httpContextAccessor.Object);

        // Act
        var service = factory.GetServiceByTenantId(tenantId);

        // Assert
        Assert.NotNull(service);
    }

    [Theory]
    [InlineData("UnknownTenant")]
    [InlineData("invalid-tenant")]
    [InlineData("__garbage__")]
    public void GetServiceByTenantId_UnknownTenant_ThrowsBadHttpRequestException(string unknownTenantId)
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        var factory = new TenantUserServiceFactory(serviceProvider, httpContextAccessor.Object);

        // Act & Assert
        var ex = Assert.Throws<BadHttpRequestException>(() => factory.GetServiceByTenantId(unknownTenantId));
        Assert.Equal(StatusCodes.Status400BadRequest, ex.StatusCode);
    }

    [Fact]
    public void GetCurrentTenantService_MissingTenantIdHeader_ThrowsBadHttpRequestException()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.Items).Returns(new Dictionary<object, object?>());
        mockHttpContext.Setup(c => c.Request.Headers).Returns(new HeaderDictionary());

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

        var factory = new TenantUserServiceFactory(serviceProvider, httpContextAccessor.Object);

        // Act & Assert
        var ex = Assert.Throws<BadHttpRequestException>(() => factory.GetCurrentTenantService());
        Assert.Equal(StatusCodes.Status400BadRequest, ex.StatusCode);
    }

    // ─── DefaultPermissionServiceFactory ────────────────────────────────────

    [Theory]
    [InlineData(nameof(Tenants.SkyLabmgm))]
    [InlineData(nameof(Tenants.SkyLabdevelop))]
    public void DefaultPermissionServiceFactory_KnownTenant_ReturnsService(string tenantId)
    {
        // Arrange
        var mockPermService = new Mock<IDefaultPermissionService>();
        var services = new ServiceCollection();
        services.AddKeyedScoped<IDefaultPermissionService>(tenantId, (_, _) => mockPermService.Object);
        var serviceProvider = services.BuildServiceProvider();

        var factory = new DefaultPermissionServiceFactory(serviceProvider);

        // Act
        var service = factory.GetService(tenantId);

        // Assert
        Assert.NotNull(service);
    }

    [Theory]
    [InlineData("UnknownTenant")]
    [InlineData("bogus-tenant-xyz")]
    public void DefaultPermissionServiceFactory_UnknownTenant_ThrowsBadHttpRequestException(string unknownTenantId)
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var factory = new DefaultPermissionServiceFactory(serviceProvider);

        // Act & Assert
        var ex = Assert.Throws<BadHttpRequestException>(() => factory.GetService(unknownTenantId));
        Assert.Equal(StatusCodes.Status400BadRequest, ex.StatusCode);
    }
}
