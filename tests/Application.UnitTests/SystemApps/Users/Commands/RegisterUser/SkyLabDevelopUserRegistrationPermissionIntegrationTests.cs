using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos.User.Registration;
using SkyLabIdP.Application.SystemApps.Services;
using SkyLabIdP.Application.SystemApps.Users.Commands.RegisterUser;
using SkyLabIdP.Domain.Entities;
using SkyLabIdP.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;

namespace Application.UnitTests.SystemApps.Users.Commands.RegisterUser;

/// <summary>
/// SkyLabDevelop 用戶註冊權限分配整合測試
/// 測試從註冊命令到權限分配的完整流程
/// </summary>
[Collection("TransactionalTests")]
public class SkyLabDevelopUserRegistrationPermissionIntegrationTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<RoleManager<ApplicationRoles>> _mockRoleManager;
    private readonly Mock<ILogger<SkyLabDevelopDefaultPermissionService>> _mockLogger;
    
    private readonly SkyLabDevelopDefaultPermissionService _permissionService;

    public SkyLabDevelopUserRegistrationPermissionIntegrationTests()
    {
        // Arrange - 建立 Mock 和實際物件
        _mockUserManager = MockUserManager();
        _mockRoleManager = MockRoleManager();
        _mockLogger = new Mock<ILogger<SkyLabDevelopDefaultPermissionService>>();

        // 建立真實的 Permission Service
        _permissionService = new SkyLabDevelopDefaultPermissionService(
            _mockUserManager.Object,
            _mockRoleManager.Object,
            _mockLogger.Object);
    }

    #region 完整權限分配流程測試

    [Fact]
    public async Task RegisterUser_Should_Assign_SkyLabDeveloper_Role_For_SkyLabdevelop_Tenant()
    {
        // Arrange
        var user = CreateTestUser("developer-user-id");
        var role = CreateTestRole(Roles.SkyLabDeveloper.GetName());
        var tenantId = nameof(Tenants.SkyLabdevelop);
        var roleClaims = CreateTestRoleClaims();

        SetupUserManagerForSuccessfulRegistration(user);
        SetupRoleManagerForSuccessfulRegistration(role, roleClaims);

        // Act
        await _permissionService.SetDefaultPermissionsAsync(user.Id, tenantId);

        // Assert
        _mockUserManager.Verify(
            x => x.AddToRoleAsync(user, Roles.SkyLabDeveloper.GetName()),
            Times.Once,
            "應該為 SkyLabdevelop 租戶分配 SkyLabDeveloper 角色");

        // 驗證角色聲明複製
        foreach (var claim in roleClaims)
        {
            _mockUserManager.Verify(
                x => x.AddClaimAsync(user, It.Is<Claim>(c => c.Type == claim.Type && c.Value == claim.Value)),
                Times.Once,
                $"應該複製角色聲明 {claim.Type}:{claim.Value} 到使用者");
        }
    }

    [Fact]
    public async Task RegisterUser_Should_Use_Empty_Custom_Permissions_For_SkyLabdevelop_Tenant()
    {
        // Arrange
        var user = CreateTestUser("empty-permissions-user");
        var role = CreateTestRole(Roles.SkyLabDeveloper.GetName());
        var tenantId = nameof(Tenants.SkyLabdevelop);
        var emptyRoleClaims = new List<Claim>(); // 空的權限配置

        SetupUserManagerForSuccessfulRegistration(user);
        SetupRoleManagerForSuccessfulRegistration(role, emptyRoleClaims);

        // Act
        await _permissionService.SetDefaultPermissionsAsync(user.Id, tenantId);

        // Assert
        _mockUserManager.Verify(
            x => x.AddToRoleAsync(user, Roles.SkyLabDeveloper.GetName()),
            Times.Once,
            "應該為 SkyLabDevelop 租戶分配 SkyLabDeveloper 角色");

        // 確認沒有額外的權限聲明被添加
        _mockUserManager.Verify(
            x => x.AddClaimAsync(It.IsAny<ApplicationUser>(), It.IsAny<Claim>()),
            Times.Never,
            "SkyLabDevelop 租戶應該使用空的自訂權限配置");
    }

    [Fact]
    public async Task RegisterUser_Should_Handle_Multiple_Role_Claims_Correctly()
    {
        // Arrange
        var user = CreateTestUser("multi-claims-user");
        var role = CreateTestRole(Roles.SkyLabDeveloper.GetName());
        var tenantId = nameof(Tenants.SkyLabdevelop);
        var roleClaims = new List<Claim>
        {
            new("permission", "develop.read"),
            new("permission", "develop.write"),
            new("permission", "develop.deploy"),
            new("custom", "develop.admin"),
            new("scope", "develop.projects")
        };

        SetupUserManagerForSuccessfulRegistration(user);
        SetupRoleManagerForSuccessfulRegistration(role, roleClaims);

        // Act
        await _permissionService.SetDefaultPermissionsAsync(user.Id, tenantId);

        // Assert
        foreach (var claim in roleClaims)
        {
            _mockUserManager.Verify(
                x => x.AddClaimAsync(user, It.Is<Claim>(c => c.Type == claim.Type && c.Value == claim.Value)),
                Times.Once,
                $"應該複製角色聲明 {claim.Type}:{claim.Value}");
        }

        Assert.Equal(5, roleClaims.Count);
    }

    [Fact]
    public async Task RegisterUser_Should_Work_Without_Additional_Data()
    {
        // Arrange
        var user = CreateTestUser("no-additional-data-user");
        var role = CreateTestRole(Roles.SkyLabDeveloper.GetName());
        var tenantId = nameof(Tenants.SkyLabdevelop);

        SetupUserManagerForSuccessfulRegistration(user);
        SetupRoleManagerForSuccessfulRegistration(role, new List<Claim>());

        // Act & Assert - 不應拋出異常
        await _permissionService.SetDefaultPermissionsAsync(user.Id, tenantId, additionalData: null);

        _mockUserManager.Verify(
            x => x.AddToRoleAsync(user, Roles.SkyLabDeveloper.GetName()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterUser_Should_Log_Comprehensive_Information()
    {
        // Arrange
        var user = CreateTestUser("logged-user");
        var role = CreateTestRole(Roles.SkyLabDeveloper.GetName());
        var tenantId = nameof(Tenants.SkyLabdevelop);

        SetupUserManagerForSuccessfulRegistration(user);
        SetupRoleManagerForSuccessfulRegistration(role, CreateTestRoleClaims());

        // Act
        await _permissionService.SetDefaultPermissionsAsync(user.Id, tenantId);

        // Assert - 驗證日誌記錄
        VerifyLoggerCalled(LogLevel.Information, 
            $"開始為 SkyLabDevelop 租戶使用者 {user.Id} 設定專用預設權限");
        
        VerifyLoggerCalled(LogLevel.Information, 
            $"成功為 SkyLabDevelop 用戶 {user.Id} 分配角色和權限");
        
        VerifyLoggerCalled(LogLevel.Information, 
            $"成功為 SkyLabDevelop 租戶使用者 {user.Id} 設定專用預設權限");
    }

    #endregion

    #region 基本工廠模式測試

    [Fact]
    public void SkyLabDevelopDefaultPermissionService_Should_Be_Created_Successfully()
    {
        // Act & Assert
        Assert.NotNull(_permissionService);
        Assert.IsType<SkyLabDevelopDefaultPermissionService>(_permissionService);
    }

    [Fact]
    public void SkyLabDevelopDefaultPermissionService_Should_Inherit_From_DefaultPermissionService()
    {
        // Act & Assert
        Assert.IsAssignableFrom<DefaultPermissionService>(_permissionService);
        Assert.IsAssignableFrom<IDefaultPermissionService>(_permissionService);
    }

    #endregion

    #region 錯誤處理和邊界測試

    [Fact]
    public async Task RegisterUser_Should_Handle_Role_Assignment_Failure_Gracefully()
    {
        // Arrange
        var user = CreateTestUser("failed-assignment-user");
        var tenantId = nameof(Tenants.SkyLabdevelop);

        SetupUserManagerForUser(user);
        SetupUserManagerForFailedRoleAssignment();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _permissionService.SetDefaultPermissionsAsync(user.Id, tenantId));

        Assert.Contains($"設定使用者 {user.Id} 的預設權限失敗", exception.Message);
        
        // 驗證錯誤日誌
        VerifyLoggerCalled(LogLevel.Error, 
            $"為 SkyLabDevelop 租戶使用者 {user.Id} 設定專用預設權限失敗");
    }

    [Theory]
    [InlineData(nameof(Tenants.SkyLabmgm))]
    public async Task RegisterUser_Should_Use_Base_Method_For_Non_SkyLabDevelop_Tenant(string tenantId)
    {
        // Arrange
        var user = CreateTestUser($"non-develop-user-{tenantId}");
        SetupUserManagerForUser(user);

        // Act
        await _permissionService.SetDefaultPermissionsAsync(user.Id, tenantId);

        // Assert - 確認不會分配 SkyLabDeveloper 角色
        _mockUserManager.Verify(
            x => x.AddToRoleAsync(user, Roles.SkyLabDeveloper.GetName()),
            Times.Never,
            $"對於非 SkyLabDevelop 租戶 {tenantId}，不應分配 SkyLabDeveloper 角色");
    }

    [Fact]
    public async Task RegisterUser_Should_Handle_Unknown_Tenant()
    {
        // Arrange
        var user = CreateTestUser("unknown-tenant-user");
        var unknownTenantId = "UnknownTenant";
        SetupUserManagerForUser(user);

        // Act
        await _permissionService.SetDefaultPermissionsAsync(user.Id, unknownTenantId);

        // Assert
        _mockUserManager.Verify(
            x => x.AddToRoleAsync(user, Roles.SkyLabDeveloper.GetName()),
            Times.Never,
            "對於未知租戶，不應分配 SkyLabDeveloper 角色");
    }

    [Fact]
    public async Task RegisterUser_Should_Handle_Exception_During_Permission_Assignment()
    {
        // Arrange
        var user = CreateTestUser("exception-user");
        var tenantId = nameof(Tenants.SkyLabdevelop);
        var originalException = new InvalidOperationException("測試異常");

        SetupUserManagerForUser(user);
        _mockUserManager
            .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ThrowsAsync(originalException);

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _permissionService.SetDefaultPermissionsAsync(user.Id, tenantId));

        // 驗證拋出的是新的異常，包含預期的錯誤訊息
        Assert.Contains($"設定使用者 {user.Id} 的預設權限失敗", actualException.Message);
        VerifyLoggerCalled(LogLevel.Error, 
            $"為 SkyLabDevelop 租戶使用者 {user.Id} 設定專用預設權限失敗");
    }

    #endregion

    #region 效能和並發測試

    [Fact]
    public async Task RegisterUser_Should_Handle_Concurrent_Permission_Assignments()
    {
        // Arrange
        var users = Enumerable.Range(1, 5)
            .Select(i => CreateTestUser($"concurrent-user-{i}"))
            .ToList();

        var role = CreateTestRole(Roles.SkyLabDeveloper.GetName());
        var tenantId = nameof(Tenants.SkyLabdevelop);

        foreach (var user in users)
        {
            SetupUserManagerForUser(user);
        }
        
        SetupUserManagerForSuccessfulRoleAssignment();
        SetupRoleManagerForRole(role);
        SetupRoleManagerForClaims(role, CreateTestRoleClaims());
        SetupUserManagerForSuccessfulClaimAssignment();

        // Act
        var tasks = users.Select(user => 
            _permissionService.SetDefaultPermissionsAsync(user.Id, tenantId)).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        foreach (var user in users)
        {
            _mockUserManager.Verify(
                x => x.AddToRoleAsync(user, Roles.SkyLabDeveloper.GetName()),
                Times.Once,
                $"使用者 {user.Id} 應該被分配角色");
        }
    }

    [Fact]
    public async Task RegisterUser_Should_Handle_Large_Number_Of_Role_Claims()
    {
        // Arrange
        var user = CreateTestUser("large-claims-user");
        var role = CreateTestRole(Roles.SkyLabDeveloper.GetName());
        var tenantId = nameof(Tenants.SkyLabdevelop);
        
        // 建立大量的角色聲明
        var largeClaims = Enumerable.Range(1, 50)
            .Select(i => new Claim("permission", $"develop.action{i}"))
            .ToList();

        SetupUserManagerForSuccessfulRegistration(user);
        SetupRoleManagerForSuccessfulRegistration(role, largeClaims);

        // Act
        await _permissionService.SetDefaultPermissionsAsync(user.Id, tenantId);

        // Assert
        foreach (var claim in largeClaims)
        {
            _mockUserManager.Verify(
                x => x.AddClaimAsync(user, It.Is<Claim>(c => c.Type == claim.Type && c.Value == claim.Value)),
                Times.Once,
                $"應該複製大量聲明中的 {claim.Value}");
        }
    }

    #endregion

    #region Helper Methods

    private ApplicationUser CreateTestUser(string userId)
    {
        return new ApplicationUser
        {
            Id = userId,
            UserName = $"{userId}@test.com",
            Email = $"{userId}@test.com",
            IsActive = true,
            IsApproved = true
        };
    }

    private ApplicationRoles CreateTestRole(string roleName)
    {
        return new ApplicationRoles
        {
            Id = $"{roleName.ToLower()}-role-id",
            Name = roleName
        };
    }

    private List<Claim> CreateTestRoleClaims()
    {
        return new List<Claim>
        {
            new("permission", "develop.read"),
            new("permission", "develop.write"),
            new("custom", "develop.deploy")
        };
    }

    private void SetupUserManagerForSuccessfulRegistration(ApplicationUser user)
    {
        SetupUserManagerForUser(user);
        SetupUserManagerForSuccessfulRoleAssignment();
        SetupUserManagerForSuccessfulClaimAssignment();
    }

    private void SetupUserManagerForUser(ApplicationUser user)
    {
        _mockUserManager
            .Setup(x => x.FindByIdAsync(user.Id))
            .ReturnsAsync(user);
    }

    private void SetupUserManagerForSuccessfulRoleAssignment()
    {
        _mockUserManager
            .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
    }

    private void SetupUserManagerForFailedRoleAssignment()
    {
        var errors = new List<IdentityError>
        {
            new() { Code = "RoleAssignmentFailed", Description = "角色分配失敗" }
        };

        _mockUserManager
            .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(errors.ToArray()));
    }

    private void SetupUserManagerForSuccessfulClaimAssignment()
    {
        _mockUserManager
            .Setup(x => x.AddClaimAsync(It.IsAny<ApplicationUser>(), It.IsAny<Claim>()))
            .ReturnsAsync(IdentityResult.Success);
    }

    private void SetupRoleManagerForSuccessfulRegistration(ApplicationRoles role, List<Claim> claims)
    {
        SetupRoleManagerForRole(role);
        SetupRoleManagerForClaims(role, claims);
    }

    private void SetupRoleManagerForRole(ApplicationRoles role)
    {
        _mockRoleManager
            .Setup(x => x.FindByNameAsync(role.Name!))
            .ReturnsAsync(role);
    }

    private void SetupRoleManagerForClaims(ApplicationRoles role, List<Claim> claims)
    {
        _mockRoleManager
            .Setup(x => x.GetClaimsAsync(role))
            .ReturnsAsync(claims);
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
