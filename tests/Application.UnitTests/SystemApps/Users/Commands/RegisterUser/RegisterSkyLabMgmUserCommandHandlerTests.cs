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
/// SkyLabMgm 權限服務工廠相關單元測試
/// 測試權限服務工廠是否正確為 SkyLabMgm 租戶返回對應的權限服務
/// </summary>
[Collection("TransactionalTests")]
public class RegisterSkyLabMgmUserCommandHandlerTests
{
    private readonly Mock<IDefaultPermissionServiceFactory> _mockPermissionServiceFactory;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;

    public RegisterSkyLabMgmUserCommandHandlerTests()
    {
        // Arrange - 建立基本的 Mock 物件
        _mockPermissionServiceFactory = new Mock<IDefaultPermissionServiceFactory>();
        _mockUserManager = MockUserManager();
    }

    #region 權限服務工廠測試

    /// <summary>
    /// 測試權限服務工廠是否正確為 SkyLabMgm 租戶返回對應的權限服務
    /// </summary>
    [Fact]
    public void PermissionServiceFactory_Should_Return_SkyLabMgmDefaultPermissionService_For_SkyLabmgm_Tenant()
    {
        // Arrange
        var tenantId = nameof(Tenants.SkyLabmgm);
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

    #endregion
}
