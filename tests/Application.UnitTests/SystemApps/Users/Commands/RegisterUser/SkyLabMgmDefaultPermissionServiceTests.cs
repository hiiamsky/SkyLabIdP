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
/// SkyLabMgmDefaultPermissionService 單元測試
/// 測試新增用戶時的權限分配邏輯
/// </summary>
[Collection("TransactionalTests")]
public class SkyLabMgmDefaultPermissionServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<RoleManager<ApplicationRoles>> _mockRoleManager;
    private readonly Mock<ILogger<SkyLabMgmDefaultPermissionService>> _mockLogger;
    private readonly SkyLabMgmDefaultPermissionService _permissionService;
    private readonly ApplicationUser _testUser;
    private readonly ApplicationRoles _testRole;

    public SkyLabMgmDefaultPermissionServiceTests()
    {
        // Arrange - 建立 Mock 物件
        _mockUserManager = MockUserManager();
        _mockRoleManager = MockRoleManager();
        _mockLogger = new Mock<ILogger<SkyLabMgmDefaultPermissionService>>();

        // 建立測試服務實例
        _permissionService = new SkyLabMgmDefaultPermissionService(
            _mockUserManager.Object,
            _mockRoleManager.Object,
            _mockLogger.Object);

        // 建立測試資料
        _testUser = new ApplicationUser
        {
            Id = "test-user-id",
            UserName = "test@example.com",
            Email = "test@example.com"
        };

        _testRole = new ApplicationRoles
        {
            Id = "test-role-id",
            Name = Roles.SkyLabSystemMgmt.GetName()
        };
    }

    #region ServiceAgency "00" 測試案例

    [Fact]
    public async Task SetDefaultPermissionsAsync_Should_Assign_SkyLabSystemMgmt_Role_When_ServiceAgency_Is_00()
    {
        // Arrange - 準備測試資料和預期行為
        var userId = _testUser.Id;
        var tenantId = nameof(Tenants.SkyLabmgm);
        var additionalData = new Dictionary<string, object>
        {
            ["ServiceAgency"] = "00"
        };
        var cancellationToken = CancellationToken.None;

        SetupUserManagerFindUser();
        SetupUserManagerAddToRoleSuccess();
        SetupRoleManagerFindRole();
        SetupRoleManagerGetClaims();
        SetupUserManagerAddClaimSuccess();

        // Act - 執行被測試的方法
        await _permissionService.SetDefaultPermissionsAsync(userId, tenantId, cancellationToken, additionalData);

        // Assert - 驗證結果
        _mockUserManager.Verify(
            x => x.AddToRoleAsync(_testUser, Roles.SkyLabSystemMgmt.GetName()), 
            Times.Once, 
            "應該將使用者加入 SkyLabSystemMgmt 角色");

        VerifyLoggerCalled(LogLevel.Information, "成功為 SkyLabMgm 租戶使用者");
    }



    #endregion

    #region 錯誤處理測試案例

    [Fact]
    public async Task SetDefaultPermissionsAsync_Should_Use_Default_Role_When_ServiceAgency_Is_Missing()
    {
        // Arrange - 不提供 ServiceAgency 資料
        var userId = _testUser.Id;
        var tenantId = nameof(Tenants.SkyLabmgm);
        var additionalData = new Dictionary<string, object>(); // 空的額外資料
        var cancellationToken = CancellationToken.None;

        SetupUserManagerFindUser();
        SetupUserManagerAddToRoleSuccess();
        SetupRoleManagerFindRole();
        SetupRoleManagerGetClaims();
        SetupUserManagerAddClaimSuccess();

        // Act
        await _permissionService.SetDefaultPermissionsAsync(userId, tenantId, cancellationToken, additionalData);

        // Assert - 應該使用預設角色
        _mockUserManager.Verify(
            x => x.AddToRoleAsync(_testUser, Roles.SkyLabSystemMgmt.GetName()), 
            Times.Once, 
            "當沒有提供 ServiceAgency 時，應該使用預設的 SkyLabSystemMgmt 角色");

        VerifyLoggerCalled(LogLevel.Information, "成功為 SkyLabMgm 租戶使用者");
    }

    [Fact]
    public async Task SetDefaultPermissionsAsync_Should_Use_Default_Role_When_AdditionalData_Is_Null()
    {
        // Arrange - additionalData 為 null
        var userId = _testUser.Id;
        var tenantId = nameof(Tenants.SkyLabmgm);
        Dictionary<string, object>? additionalData = null;
        var cancellationToken = CancellationToken.None;

        SetupUserManagerFindUser();
        SetupUserManagerAddToRoleSuccess();
        SetupRoleManagerFindRole();
        SetupRoleManagerGetClaims();
        SetupUserManagerAddClaimSuccess();

        // Act
        await _permissionService.SetDefaultPermissionsAsync(userId, tenantId, cancellationToken, additionalData);

        // Assert
        _mockUserManager.Verify(
            x => x.AddToRoleAsync(_testUser, Roles.SkyLabSystemMgmt.GetName()), 
            Times.Once, 
            "當 additionalData 為 null 時，應該使用預設的 SkyLabSystemMgmt 角色");

        VerifyLoggerCalled(LogLevel.Information, "成功為 SkyLabMgm 租戶使用者");
    }

    [Fact]
    public async Task SetDefaultPermissionsAsync_Should_Throw_Exception_When_Role_Assignment_Fails()
    {
        // Arrange - 設定角色分配失敗
        var userId = _testUser.Id;
        var tenantId = nameof(Tenants.SkyLabmgm);
        var additionalData = new Dictionary<string, object>
        {
            ["ServiceAgency"] = "00"
        };

        SetupUserManagerFindUser();
        SetupUserManagerAddToRoleFailed(); // 設定分配角色失敗

        // Act & Assert - 驗證會拋出例外
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _permissionService.SetDefaultPermissionsAsync(userId, tenantId, CancellationToken.None, additionalData));

        Assert.Contains("設定使用者", exception.Message);
        Assert.Contains("的預設權限失敗", exception.Message);

        VerifyLoggerCalled(LogLevel.Error, "為 SkyLabMgm 租戶使用者");
    }

    #endregion

    #region 非 SkyLabMgm 租戶測試案例

    [Fact]
    public async Task SetDefaultPermissionsAsync_Should_Call_Base_Method_When_Not_SkyLabMgm_Tenant()
    {
        // Arrange - 使用非 SkyLabMgm 租戶
        var userId = _testUser.Id;
        var tenantId = "SkyLabcommittee"; // 不同的租戶
        var additionalData = new Dictionary<string, object>
        {
            ["ServiceAgency"] = "00"
        };

        // Act
        await _permissionService.SetDefaultPermissionsAsync(userId, tenantId, CancellationToken.None, additionalData);

        // Assert - 應該不會呼叫角色分配方法（因為會呼叫基底類別方法）
        _mockUserManager.Verify(
            x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), 
            Times.Never, 
            "非 SkyLabMgm 租戶應該呼叫基底類別方法，不進行角色分配");

        VerifyLoggerCalled(LogLevel.Information, "開始為 SkyLabMgm 租戶使用者");
    }

    #endregion

    #region 權限複製測試案例

    [Fact]
    public async Task SetDefaultPermissionsAsync_Should_Copy_Role_Claims_To_User_Claims()
    {
        // Arrange
        var userId = _testUser.Id;
        var tenantId = nameof(Tenants.SkyLabmgm);
        var additionalData = new Dictionary<string, object>
        {
            ["ServiceAgency"] = "00"
        };

        var roleClaims = new List<Claim>
        {
            new("permission", "read"),
            new("permission", "write"),
            new("function", "user_management")
        };

        SetupUserManagerFindUser();
        SetupUserManagerAddToRoleSuccess();
        SetupRoleManagerFindRole();
        SetupRoleManagerGetClaims(roleClaims);
        SetupUserManagerAddClaimSuccess();

        // Act
        await _permissionService.SetDefaultPermissionsAsync(userId, tenantId, CancellationToken.None, additionalData);

        // Assert - 驗證每個角色權限都被複製到使用者權限
        foreach (var claim in roleClaims)
        {
            _mockUserManager.Verify(
                x => x.AddClaimAsync(_testUser, It.Is<Claim>(c => c.Type == claim.Type && c.Value == claim.Value)), 
                Times.Once, 
                $"角色權限 {claim.Type}:{claim.Value} 應該被複製到使用者權限");
        }
    }

    #endregion

    #region Helper Methods for Setup

    private void SetupUserManagerFindUser()
    {
        _mockUserManager.Setup(x => x.FindByIdAsync(_testUser.Id))
            .ReturnsAsync(_testUser);
    }

    private void SetupUserManagerAddToRoleSuccess()
    {
        _mockUserManager.Setup(x => x.AddToRoleAsync(_testUser, It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
    }

    private void SetupUserManagerAddToRoleFailed()
    {
        var errors = new[]
        {
            new IdentityError { Code = "RoleError", Description = "無法分配角色" }
        };
        _mockUserManager.Setup(x => x.AddToRoleAsync(_testUser, It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(errors));
    }

    private void SetupRoleManagerFindRole()
    {
        _mockRoleManager.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(_testRole);
    }

    private void SetupRoleManagerGetClaims(List<Claim>? claims = null)
    {
        claims ??= new List<Claim>();
        _mockRoleManager.Setup(x => x.GetClaimsAsync(_testRole))
            .ReturnsAsync(claims);
    }

    private void SetupUserManagerAddClaimSuccess()
    {
        _mockUserManager.Setup(x => x.AddClaimAsync(_testUser, It.IsAny<Claim>()))
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
        var optionsAccessor = new Mock<IOptions<IdentityOptions>>();
        var passwordHasher = new Mock<IPasswordHasher<ApplicationUser>>();
        var userValidators = new List<IUserValidator<ApplicationUser>>();
        var passwordValidators = new List<IPasswordValidator<ApplicationUser>>();
        var keyNormalizer = new Mock<ILookupNormalizer>();
        var errors = new Mock<IdentityErrorDescriber>();
        var services = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<UserManager<ApplicationUser>>>();
        
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, optionsAccessor.Object, passwordHasher.Object, userValidators, 
            passwordValidators, keyNormalizer.Object, errors.Object, services.Object, logger.Object);
    }

    private static Mock<RoleManager<ApplicationRoles>> MockRoleManager()
    {
        var store = new Mock<IRoleStore<ApplicationRoles>>();
        var roleValidators = new List<IRoleValidator<ApplicationRoles>>();
        var keyNormalizer = new Mock<ILookupNormalizer>();
        var errors = new Mock<IdentityErrorDescriber>();
        var logger = new Mock<ILogger<RoleManager<ApplicationRoles>>>();
        
        return new Mock<RoleManager<ApplicationRoles>>(
            store.Object, roleValidators, keyNormalizer.Object, errors.Object, logger.Object);
    }

    #endregion
}
