using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos.Permission;
using SkyLabIdP.Application.SystemApps.Services;
using SkyLabIdP.Domain.Entities;
using SkyLabIdP.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;

namespace Application.UnitTests.SystemApps.Users.Commands.RegisterUser;

/// <summary>
/// SkyLabDevelopDefaultPermissionService 單元測試
/// 測試新增用戶時的權限分配邏輯
/// </summary>
[Collection("TransactionalTests")]
public class SkyLabDevelopDefaultPermissionServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<RoleManager<ApplicationRoles>> _mockRoleManager;
    private readonly Mock<ILogger<SkyLabDevelopDefaultPermissionService>> _mockLogger;
    private readonly SkyLabDevelopDefaultPermissionService _permissionService;
    private readonly ApplicationUser _testUser;
    private readonly ApplicationRoles _testRole;

    public SkyLabDevelopDefaultPermissionServiceTests()
    {
        // Arrange - 建立 Mock 物件
        _mockUserManager = MockUserManager();
        _mockRoleManager = MockRoleManager();
        _mockLogger = new Mock<ILogger<SkyLabDevelopDefaultPermissionService>>();

        // 建立測試服務實例
        _permissionService = new SkyLabDevelopDefaultPermissionService(
            _mockUserManager.Object,
            _mockRoleManager.Object,
            _mockLogger.Object);

        // 建立測試資料
        _testUser = new ApplicationUser
        {
            Id = "test-user-id",
            UserName = "testuser@test.com",
            Email = "testuser@test.com",
            IsActive = true,
            IsApproved = true
        };

        _testRole = new ApplicationRoles
        {
            Id = "developer-role-id",
            Name = Roles.SkyLabDeveloper.GetName()
        };
    }

    #region SkyLabDevelop 租戶權限分配測試案例

    [Fact]
    public async Task SetDefaultPermissionsAsync_Should_Assign_SkyLabDeveloper_Role_When_Is_SkyLabdevelop_Tenant()
    {
        // Arrange
        var tenantId = nameof(Tenants.SkyLabdevelop);
        SetupUserManagerFindUser();
        SetupUserManagerAddToRoleSuccess();
        SetupRoleManagerFindRole();
        SetupRoleManagerGetClaims();
        SetupUserManagerAddClaimSuccess();

        // Act
        await _permissionService.SetDefaultPermissionsAsync(_testUser.Id, tenantId);

        // Assert
        _mockUserManager.Verify(
            x => x.AddToRoleAsync(_testUser, Roles.SkyLabDeveloper.GetName()),
            Times.Once,
            "應該為 SkyLabDevelop 租戶分配 SkyLabDeveloper 角色");

        VerifyLoggerCalled(LogLevel.Information, 
            $"成功為 SkyLabDevelop 用戶 {_testUser.Id} 分配角色和權限");
    }

    [Fact]
    public async Task SetDefaultPermissionsAsync_Should_Use_Empty_Custom_Permissions_For_SkyLabdevelop_Tenant()
    {
        // Arrange
        var tenantId = nameof(Tenants.SkyLabdevelop);
        SetupUserManagerFindUser();
        SetupUserManagerAddToRoleSuccess();
        SetupRoleManagerFindRole();
        SetupRoleManagerGetClaims(new List<Claim>()); // 空的 Claims 列表
        SetupUserManagerAddClaimSuccess();

        // Act
        await _permissionService.SetDefaultPermissionsAsync(_testUser.Id, tenantId);

        // Assert - 確認沒有嘗試添加額外的 Claims（因為自訂權限配置為空）
        _mockUserManager.Verify(
            x => x.AddClaimAsync(It.IsAny<ApplicationUser>(), It.IsAny<Claim>()),
            Times.Never,
            "SkyLabDevelop 租戶的自訂權限配置應該為空，不應添加額外的 Claims");
    }

    [Fact]
    public async Task SetDefaultPermissionsAsync_Should_Log_Start_And_Success_Messages()
    {
        // Arrange
        var tenantId = nameof(Tenants.SkyLabdevelop);
        SetupUserManagerFindUser();
        SetupUserManagerAddToRoleSuccess();
        SetupRoleManagerFindRole();
        SetupRoleManagerGetClaims();
        SetupUserManagerAddClaimSuccess();

        // Act
        await _permissionService.SetDefaultPermissionsAsync(_testUser.Id, tenantId);

        // Assert
        VerifyLoggerCalled(LogLevel.Information, 
            $"開始為 SkyLabDevelop 租戶使用者 {_testUser.Id} 設定專用預設權限");
        
        VerifyLoggerCalled(LogLevel.Information, 
            $"成功為 SkyLabDevelop 租戶使用者 {_testUser.Id} 設定專用預設權限");
    }

    #endregion

    #region 錯誤處理測試案例

    [Fact]
    public async Task SetDefaultPermissionsAsync_Should_Throw_Exception_When_Role_Assignment_Fails()
    {
        // Arrange
        var tenantId = nameof(Tenants.SkyLabdevelop);
        SetupUserManagerFindUser();
        SetupUserManagerAddToRoleFailed();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _permissionService.SetDefaultPermissionsAsync(_testUser.Id, tenantId));

        Assert.Contains($"設定使用者 {_testUser.Id} 的預設權限失敗", exception.Message);
        VerifyLoggerCalled(LogLevel.Error, $"為 SkyLabDevelop 租戶使用者 {_testUser.Id} 設定專用預設權限失敗");
    }

    [Fact]
    public async Task SetDefaultPermissionsAsync_Should_Handle_Null_AdditionalData()
    {
        // Arrange
        var tenantId = nameof(Tenants.SkyLabdevelop);
        SetupUserManagerFindUser();
        SetupUserManagerAddToRoleSuccess();
        SetupRoleManagerFindRole();
        SetupRoleManagerGetClaims();
        SetupUserManagerAddClaimSuccess();

        // Act & Assert - 不應拋出異常
        await _permissionService.SetDefaultPermissionsAsync(_testUser.Id, tenantId, additionalData: null);

        _mockUserManager.Verify(
            x => x.AddToRoleAsync(_testUser, Roles.SkyLabDeveloper.GetName()),
            Times.Once);
    }

    [Fact]
    public async Task SetDefaultPermissionsAsync_Should_Log_Error_And_Throw_New_Exception_On_Failure()
    {
        // Arrange
        var tenantId = nameof(Tenants.SkyLabdevelop);
        var originalException = new InvalidOperationException("Test exception");
        
        SetupUserManagerFindUser();
        _mockUserManager
            .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ThrowsAsync(originalException);

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _permissionService.SetDefaultPermissionsAsync(_testUser.Id, tenantId));

        // 驗證拋出的是新的異常，包含預期的錯誤訊息
        Assert.Contains($"設定使用者 {_testUser.Id} 的預設權限失敗", actualException.Message);
        VerifyLoggerCalled(LogLevel.Error, $"為 SkyLabDevelop 租戶使用者 {_testUser.Id} 設定專用預設權限失敗");
    }

    #endregion

    #region 非 SkyLabDevelop 租戶測試案例

    [Theory]
    [InlineData(nameof(Tenants.SkyLabmgm))]
    public async Task SetDefaultPermissionsAsync_Should_Call_Base_Method_When_Not_SkyLabDevelop_Tenant(string tenantId)
    {
        // Arrange
        SetupUserManagerFindUser();

        // Act
        await _permissionService.SetDefaultPermissionsAsync(_testUser.Id, tenantId);

        // Assert - 確認不會調用 SkyLabDevelop 專用的角色分配
        _mockUserManager.Verify(
            x => x.AddToRoleAsync(_testUser, Roles.SkyLabDeveloper.GetName()),
            Times.Never,
            $"對於非 SkyLabDevelop 租戶 {tenantId}，不應分配 SkyLabDeveloper 角色");
    }

    [Fact]
    public async Task SetDefaultPermissionsAsync_Should_Use_Base_Method_For_Unknown_Tenant()
    {
        // Arrange
        var unknownTenantId = "UnknownTenant";
        SetupUserManagerFindUser();

        // Act
        await _permissionService.SetDefaultPermissionsAsync(_testUser.Id, unknownTenantId);

        // Assert - 確認不會調用 SkyLabDevelop 專用的角色分配
        _mockUserManager.Verify(
            x => x.AddToRoleAsync(_testUser, Roles.SkyLabDeveloper.GetName()),
            Times.Never,
            "對於未知租戶，不應分配 SkyLabDeveloper 角色");
    }

    #endregion

    #region 權限複製測試案例

    [Fact]
    public async Task SetDefaultPermissionsAsync_Should_Copy_Role_Claims_To_User_Claims()
    {
        // Arrange
        var tenantId = nameof(Tenants.SkyLabdevelop);
        var roleClaims = new List<Claim>
        {
            new("permission", "develop.read"),
            new("permission", "develop.write"),
            new("custom", "develop.special")
        };

        SetupUserManagerFindUser();
        SetupUserManagerAddToRoleSuccess();
        SetupRoleManagerFindRole();
        SetupRoleManagerGetClaims(roleClaims);
        SetupUserManagerAddClaimSuccess();

        // Act
        await _permissionService.SetDefaultPermissionsAsync(_testUser.Id, tenantId);

        // Assert
        foreach (var claim in roleClaims)
        {
            _mockUserManager.Verify(
                x => x.AddClaimAsync(_testUser, It.Is<Claim>(c => c.Type == claim.Type && c.Value == claim.Value)),
                Times.Once,
                $"應該將角色聲明 {claim.Type}:{claim.Value} 複製到使用者聲明");
        }
    }

    [Fact]
    public async Task SetDefaultPermissionsAsync_Should_Handle_Empty_Role_Claims()
    {
        // Arrange
        var tenantId = nameof(Tenants.SkyLabdevelop);
        var emptyRoleClaims = new List<Claim>();

        SetupUserManagerFindUser();
        SetupUserManagerAddToRoleSuccess();
        SetupRoleManagerFindRole();
        SetupRoleManagerGetClaims(emptyRoleClaims);
        SetupUserManagerAddClaimSuccess();

        // Act
        await _permissionService.SetDefaultPermissionsAsync(_testUser.Id, tenantId);

        // Assert
        _mockUserManager.Verify(
            x => x.AddClaimAsync(It.IsAny<ApplicationUser>(), It.IsAny<Claim>()),
            Times.Never,
            "當角色沒有聲明時，不應添加任何使用者聲明");
    }

    #endregion

    #region Helper Methods for Setup

    private void SetupUserManagerFindUser()
    {
        _mockUserManager
            .Setup(x => x.FindByIdAsync(_testUser.Id))
            .ReturnsAsync(_testUser);
    }

    private void SetupUserManagerAddToRoleSuccess()
    {
        _mockUserManager
            .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
    }

    private void SetupUserManagerAddToRoleFailed()
    {
        var errors = new List<IdentityError>
        {
            new() { Code = "RoleAssignmentFailed", Description = "角色分配失敗" }
        };

        _mockUserManager
            .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(errors.ToArray()));
    }

    private void SetupRoleManagerFindRole()
    {
        _mockRoleManager
            .Setup(x => x.FindByNameAsync(_testRole.Name!))
            .ReturnsAsync(_testRole);
    }

    private void SetupRoleManagerGetClaims(List<Claim>? claims = null)
    {
        claims ??= new List<Claim>();

        _mockRoleManager
            .Setup(x => x.GetClaimsAsync(It.IsAny<ApplicationRoles>()))
            .ReturnsAsync(claims);
    }

    private void SetupUserManagerAddClaimSuccess()
    {
        _mockUserManager
            .Setup(x => x.AddClaimAsync(It.IsAny<ApplicationUser>(), It.IsAny<Claim>()))
            .ReturnsAsync(IdentityResult.Success);
    }

    private void VerifyLoggerCalled(LogLevel level, string message)
    {
        _mockLogger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region Mock Factory Methods

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
