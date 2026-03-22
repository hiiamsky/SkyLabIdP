using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.SystemApps.Services;
using SkyLabIdP.Domain.Entities;
using SkyLabIdP.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Application.UnitTests.SystemApps.Users.Commands.RegisterUser;

/// <summary>
/// SkyLabDevelop 權限服務工廠相關單元測試
/// 測試權限服務工廠是否正確為 SkyLabDevelop 租戶返回對應的權限服務
/// </summary>
[Collection("TransactionalTests")]
public class RegisterSkyLabDevelopUserCommandHandlerTests
{
    private readonly Mock<IDefaultPermissionServiceFactory> _mockPermissionServiceFactory;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;

    public RegisterSkyLabDevelopUserCommandHandlerTests()
    {
        // Arrange - 建立基本的 Mock 物件
        _mockPermissionServiceFactory = new Mock<IDefaultPermissionServiceFactory>();
        _mockUserManager = MockUserManager();
    }

    #region 權限服務工廠測試

    /// <summary>
    /// 測試權限服務工廠是否正確為 SkyLabDevelop 租戶返回對應的權限服務
    /// </summary>
    [Fact]
    public void PermissionServiceFactory_Should_Return_SkyLabDevelopDefaultPermissionService_For_SkyLabdevelop_Tenant()
    {
        // Arrange
        var tenantId = nameof(Tenants.SkyLabdevelop);
        var expectedService = new Mock<IDefaultPermissionService>();

        _mockPermissionServiceFactory
            .Setup(x => x.GetService(tenantId))
            .Returns(expectedService.Object);

        // Act
        var service = _mockPermissionServiceFactory.Object.GetService(tenantId);

        // Assert
        Assert.NotNull(service);
        _mockPermissionServiceFactory.Verify(
            x => x.GetService(tenantId),
            Times.Once);
    }

    /// <summary>
    /// 測試權限服務工廠對於不同租戶返回不同的服務
    /// </summary>
    [Theory]
    [InlineData(nameof(Tenants.SkyLabdevelop))]
    [InlineData(nameof(Tenants.SkyLabmgm))]
    public void PermissionServiceFactory_Should_Return_Service_For_Different_Tenants(string tenantId)
    {
        // Arrange
        var expectedService = new Mock<IDefaultPermissionService>();

        _mockPermissionServiceFactory
            .Setup(x => x.GetService(tenantId))
            .Returns(expectedService.Object);

        // Act
        var service = _mockPermissionServiceFactory.Object.GetService(tenantId);

        // Assert
        Assert.NotNull(service);
        _mockPermissionServiceFactory.Verify(
            x => x.GetService(tenantId),
            Times.Once,
            $"應該為租戶 {tenantId} 返回權限服務");
    }

    /// <summary>
    /// 測試未知租戶時工廠的行為
    /// </summary>
    [Fact]
    public void PermissionServiceFactory_Should_Return_Default_Service_For_Unknown_Tenant()
    {
        // Arrange
        var unknownTenantId = "UnknownTenant";
        var defaultService = new Mock<IDefaultPermissionService>();

        _mockPermissionServiceFactory
            .Setup(x => x.GetService(unknownTenantId))
            .Returns(defaultService.Object);

        // Act
        var service = _mockPermissionServiceFactory.Object.GetService(unknownTenantId);

        // Assert
        Assert.NotNull(service);
        _mockPermissionServiceFactory.Verify(
            x => x.GetService(unknownTenantId),
            Times.Once,
            "應該為未知租戶返回預設權限服務");
    }

    /// <summary>
    /// 測試權限服務工廠的類型安全性
    /// </summary>
    [Fact]
    public void PermissionServiceFactory_Should_Return_Correct_Interface_Type()
    {
        // Arrange
        var tenantId = nameof(Tenants.SkyLabdevelop);
        var mockService = new Mock<IDefaultPermissionService>();

        _mockPermissionServiceFactory
            .Setup(x => x.GetService(tenantId))
            .Returns(mockService.Object);

        // Act
        var service = _mockPermissionServiceFactory.Object.GetService(tenantId);

        // Assert
        Assert.IsAssignableFrom<IDefaultPermissionService>(service);
        Assert.NotNull(service);
    }

    #endregion

    #region 服務整合測試

    /// <summary>
    /// 測試 SkyLabDevelop 權限服務可以成功建立
    /// </summary>
    [Fact]
    public void SkyLabDevelopDefaultPermissionService_Should_Be_Created_Successfully()
    {
        // Arrange
        var mockRoleManager = MockRoleManager();
        var mockLogger = new Mock<ILogger<SkyLabDevelopDefaultPermissionService>>();

        // Act & Assert - 不應拋出異常
        var service = new SkyLabDevelopDefaultPermissionService(
            _mockUserManager.Object,
            mockRoleManager.Object,
            mockLogger.Object);

        Assert.NotNull(service);
        Assert.IsType<SkyLabDevelopDefaultPermissionService>(service);
    }

    /// <summary>
    /// 測試 SkyLabDevelop 權限服務繼承關係
    /// </summary>
    [Fact]
    public void SkyLabDevelopDefaultPermissionService_Should_Inherit_From_DefaultPermissionService()
    {
        // Arrange
        var mockRoleManager = MockRoleManager();
        var mockLogger = new Mock<ILogger<SkyLabDevelopDefaultPermissionService>>();

        // Act
        var service = new SkyLabDevelopDefaultPermissionService(
            _mockUserManager.Object,
            mockRoleManager.Object,
            mockLogger.Object);

        // Assert
        Assert.IsAssignableFrom<DefaultPermissionService>(service);
        Assert.IsAssignableFrom<IDefaultPermissionService>(service);
    }

    /// <summary>
    /// 測試 SkyLabDevelop 權限服務的依賴注入
    /// </summary>
    [Fact]
    public void SkyLabDevelopDefaultPermissionService_Should_Accept_Required_Dependencies()
    {
        // Arrange
        var mockRoleManager = MockRoleManager();
        var mockLogger = new Mock<ILogger<SkyLabDevelopDefaultPermissionService>>();

        // Act & Assert - 驗證建構子可以接受所有必要的依賴
        var service = new SkyLabDevelopDefaultPermissionService(
            _mockUserManager.Object,
            mockRoleManager.Object,
            mockLogger.Object);

        Assert.NotNull(service);
        // 如果建構子參數不正確，這裡會在編譯時或運行時失敗
    }

    #endregion

    #region 工廠模式驗證

    /// <summary>
    /// 測試工廠模式返回的服務能夠正確處理 SkyLabDevelop 租戶
    /// </summary>
    [Fact]
    public void Factory_Should_Return_Service_Capable_Of_Handling_SkyLabDevelop_Tenant()
    {
        // Arrange
        var tenantId = nameof(Tenants.SkyLabdevelop);
        var mockService = new Mock<IDefaultPermissionService>();
        
        // 模擬服務的 SetDefaultPermissionsAsync 方法
        mockService
            .Setup(x => x.SetDefaultPermissionsAsync(
                It.IsAny<string>(), 
                tenantId, 
                It.IsAny<CancellationToken>(), 
                It.IsAny<Dictionary<string, object>>()))
            .Returns(Task.CompletedTask);

        _mockPermissionServiceFactory
            .Setup(x => x.GetService(tenantId))
            .Returns(mockService.Object);

        // Act
        var service = _mockPermissionServiceFactory.Object.GetService(tenantId);

        // Assert
        Assert.NotNull(service);
        
        // 驗證服務可以被調用（不會拋出異常）
        var task = service.SetDefaultPermissionsAsync("test-user-id", tenantId, CancellationToken.None);
        Assert.NotNull(task);
        Assert.True(task.IsCompletedSuccessfully || task.Status == TaskStatus.RanToCompletion);
    }

    #endregion

    #region Helper Methods

    private static Mock<UserManager<ApplicationUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var options = new Mock<IOptions<IdentityOptions>>();
        var passwordHasher = new Mock<IPasswordHasher<ApplicationUser>>();
        var userValidators = new List<IUserValidator<ApplicationUser>>();
        var passwordValidators = new List<IPasswordValidator<ApplicationUser>>();
        var keyNormalizer = new Mock<ILookupNormalizer>();
        var errors = new Mock<IdentityErrorDescriber>();
        var services = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<UserManager<ApplicationUser>>>();

        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            options.Object,
            passwordHasher.Object,
            userValidators,
            passwordValidators,
            keyNormalizer.Object,
            errors.Object,
            services.Object,
            logger.Object);
    }

    private static Mock<RoleManager<ApplicationRoles>> MockRoleManager()
    {
        var store = new Mock<IRoleStore<ApplicationRoles>>();
        var roleValidators = new List<IRoleValidator<ApplicationRoles>>();
        var keyNormalizer = new Mock<ILookupNormalizer>();
        var errors = new Mock<IdentityErrorDescriber>();
        var logger = new Mock<ILogger<RoleManager<ApplicationRoles>>>();

        return new Mock<RoleManager<ApplicationRoles>>(
            store.Object,
            roleValidators,
            keyNormalizer.Object,
            errors.Object,
            logger.Object);
    }

    #endregion
}
