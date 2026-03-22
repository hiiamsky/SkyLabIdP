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
/// SkyLabMgm 用戶註冊權限分配整合測試
/// 測試從註冊命令到權限分配的完整流程
/// </summary>
[Collection("TransactionalTests")]
public class SkyLabMgmUserRegistrationPermissionIntegrationTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<RoleManager<ApplicationRoles>> _mockRoleManager;
    private readonly Mock<ILogger<SkyLabMgmDefaultPermissionService>> _mockLogger;
    
    private readonly SkyLabMgmDefaultPermissionService _permissionService;

    public SkyLabMgmUserRegistrationPermissionIntegrationTests()
    {
        // Arrange - 建立 Mock 和實際物件
        _mockUserManager = MockUserManager();
        _mockRoleManager = MockRoleManager();
        _mockLogger = new Mock<ILogger<SkyLabMgmDefaultPermissionService>>();

        // 建立實際的權限服務
        _permissionService = new SkyLabMgmDefaultPermissionService(
            _mockUserManager.Object,
            _mockRoleManager.Object,
            _mockLogger.Object);
    }

    #region 完整權限分配流程測試

    [Theory]
    [InlineData("00", "SkyLabSystemMgmt")] // SkyLab -> 一般管理者
    public async Task RegisterUser_Should_Assign_Correct_Role_Based_On_ServiceAgency(
        string serviceAgency, 
        string expectedRoleName)
    {
        // Arrange - 準備測試資料
        var userId = "test-user-id";
        var user = CreateTestUser(userId);
        var role = CreateTestRole(expectedRoleName);
        var roleClaims = CreateTestRoleClaims();

        SetupUserManagerForSuccessfulRegistration(user);
        SetupRoleManagerForSuccessfulRegistration(role, roleClaims);

        var additionalData = new Dictionary<string, object>
        {
            ["ServiceAgency"] = serviceAgency
        };

        // Act - 執行權限設定
        await _permissionService.SetDefaultPermissionsAsync(
            userId, 
            nameof(Tenants.SkyLabmgm), 
            CancellationToken.None, 
            additionalData);

        // Assert - 驗證正確的角色被分配
        _mockUserManager.Verify(
            x => x.AddToRoleAsync(user, expectedRoleName),
            Times.Once,
            $"ServiceAgency '{serviceAgency}' 應該分配角色 '{expectedRoleName}'");

        // 驗證角色權限被複製到用戶
        foreach (var claim in roleClaims)
        {
            _mockUserManager.Verify(
                x => x.AddClaimAsync(user, It.Is<Claim>(c => 
                    c.Type == claim.Type && c.Value == claim.Value)),
                Times.Once,
                $"角色權限 {claim.Type}:{claim.Value} 應該被複製到用戶");
        }
    }

    [Fact]
    public async Task RegisterUser_Should_Handle_Multiple_Role_Claims_Correctly()
    {
        // Arrange - 建立包含多個權限的角色
        var userId = "test-user-id";
        var user = CreateTestUser(userId);
        var role = CreateTestRole(Roles.SkyLabSystemMgmt.GetName());
        
        var complexRoleClaims = new List<Claim>
        {
            new("permission", "user.read"),
            new("permission", "user.write"),
            new("permission", "user.delete"),
            new("function", "account_management"),
            new("function", "system_administration"),
            new("resource", "skylab_documents"),
            new("scope", "organization")
        };

        SetupUserManagerForSuccessfulRegistration(user);
        SetupRoleManagerForSuccessfulRegistration(role, complexRoleClaims);

        var additionalData = new Dictionary<string, object>
        {
            ["ServiceAgency"] = "00"
        };

        // Act
        await _permissionService.SetDefaultPermissionsAsync(
            userId, 
            nameof(Tenants.SkyLabmgm), 
            CancellationToken.None, 
            additionalData);

        // Assert - 驗證所有權限都被正確複製
        Assert.Equal(complexRoleClaims.Count, 
            _mockUserManager.Invocations.Count(i => 
                i.Method.Name == nameof(UserManager<ApplicationUser>.AddClaimAsync)));

        foreach (var expectedClaim in complexRoleClaims)
        {
            _mockUserManager.Verify(
                x => x.AddClaimAsync(user, It.Is<Claim>(c => 
                    c.Type == expectedClaim.Type && 
                    c.Value == expectedClaim.Value)),
                Times.Once,
                $"應該複製權限: {expectedClaim.Type} = {expectedClaim.Value}");
        }
    }

    #endregion

    #region 基本工廠模式測試

    [Fact]
    public void SkyLabMgmDefaultPermissionService_Should_Be_Created_Successfully()
    {
        // Act & Assert - 驗證權限服務可以成功建立
        Assert.NotNull(_permissionService);
        Assert.IsType<SkyLabMgmDefaultPermissionService>(_permissionService);
    }

    #endregion

    #region 錯誤處理和邊界測試

    [Fact]
    public async Task RegisterUser_Should_Handle_Role_Assignment_Failure_Gracefully()
    {
        // Arrange - 設定角色分配失敗
        var userId = "test-user-id";
        var user = CreateTestUser(userId);

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);
        
        var errors = new[]
        {
            new IdentityError { Code = "RoleAssignmentFailed", Description = "無法分配角色到用戶" }
        };
        _mockUserManager.Setup(x => x.AddToRoleAsync(user, It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(errors));

        var additionalData = new Dictionary<string, object>
        {
            ["ServiceAgency"] = "00"
        };

        // Act & Assert - 應該拋出例外
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _permissionService.SetDefaultPermissionsAsync(
                userId, 
                nameof(Tenants.SkyLabmgm), 
                CancellationToken.None, 
                additionalData));

        Assert.Contains("設定使用者", exception.Message);
        Assert.Contains("的預設權限失敗", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    public async Task RegisterUser_Should_Use_Default_Role_For_Invalid_ServiceAgency(string? invalidServiceAgency)
    {
        // Arrange
        var userId = "test-user-id";
        var user = CreateTestUser(userId);
        var role = CreateTestRole(Roles.SkyLabSystemMgmt.GetName());

        SetupUserManagerForSuccessfulRegistration(user);
        SetupRoleManagerForSuccessfulRegistration(role, new List<Claim>());

        var additionalData = new Dictionary<string, object>();
        if (invalidServiceAgency != null)
        {
            additionalData["ServiceAgency"] = invalidServiceAgency;
        }

        // Act
        await _permissionService.SetDefaultPermissionsAsync(
            userId, 
            nameof(Tenants.SkyLabmgm), 
            CancellationToken.None, 
            additionalData);

        // Assert - 應該分配預設角色
        _mockUserManager.Verify(
            x => x.AddToRoleAsync(user, Roles.SkyLabSystemMgmt.GetName()),
            Times.Once,
            "無效的 ServiceAgency 應該使用預設角色");
    }



    #endregion

    #region 效能和並發測試

    [Fact]
    public async Task RegisterUser_Should_Handle_Concurrent_Permission_Assignments()
    {
        // Arrange - 準備多個並發請求
        var userTasks = new List<Task>();
        var userIds = Enumerable.Range(1, 5).Select(i => $"user-{i}").ToList();

        foreach (var userId in userIds)
        {
            var user = CreateTestUser(userId);
            var role = CreateTestRole(Roles.SkyLabSystemMgmt.GetName());
            
            SetupUserManagerForUser(user);
            SetupRoleManagerForRole(role);

            var additionalData = new Dictionary<string, object>
            {
                ["ServiceAgency"] = "00"
            };

            // Act - 建立並發任務
            userTasks.Add(_permissionService.SetDefaultPermissionsAsync(
                userId, 
                nameof(Tenants.SkyLabmgm), 
                CancellationToken.None, 
                additionalData));
        }

        // 執行所有並發任務
        await Task.WhenAll(userTasks);

        // Assert - 驗證所有用戶都成功分配權限
        foreach (var userId in userIds)
        {
            _mockUserManager.Verify(
                x => x.FindByIdAsync(userId),
                Times.Once,
                $"應該查找用戶 {userId}");
        }
    }

    #endregion

    #region Helper Methods

    private ApplicationUser CreateTestUser(string userId)
    {
        return new ApplicationUser
        {
            Id = userId,
            UserName = $"{userId}@example.com",
            Email = $"{userId}@example.com"
        };
    }

    private ApplicationRoles CreateTestRole(string roleName)
    {
        return new ApplicationRoles
        {
            Id = $"role-{roleName}",
            Name = roleName
        };
    }

    private List<Claim> CreateTestRoleClaims()
    {
        return new List<Claim>
        {
            new("permission", "read"),
            new("permission", "write"),
            new("function", "user_management")
        };
    }

    private void SetupUserManagerForSuccessfulRegistration(ApplicationUser user)
    {
        _mockUserManager.Setup(x => x.FindByIdAsync(user.Id))
            .ReturnsAsync(user);
        _mockUserManager.Setup(x => x.AddToRoleAsync(user, It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.AddClaimAsync(user, It.IsAny<Claim>()))
            .ReturnsAsync(IdentityResult.Success);
    }

    private void SetupUserManagerForUser(ApplicationUser user)
    {
        _mockUserManager.Setup(x => x.FindByIdAsync(user.Id))
            .ReturnsAsync(user);
        _mockUserManager.Setup(x => x.AddToRoleAsync(user, It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.AddClaimAsync(user, It.IsAny<Claim>()))
            .ReturnsAsync(IdentityResult.Success);
    }

    private void SetupRoleManagerForSuccessfulRegistration(ApplicationRoles role, List<Claim> claims)
    {
        _mockRoleManager.Setup(x => x.FindByNameAsync(role.Name!))
            .ReturnsAsync(role);
        _mockRoleManager.Setup(x => x.GetClaimsAsync(role))
            .ReturnsAsync(claims);
    }

    private void SetupRoleManagerForRole(ApplicationRoles role)
    {
        _mockRoleManager.Setup(x => x.FindByNameAsync(role.Name!))
            .ReturnsAsync(role);
        _mockRoleManager.Setup(x => x.GetClaimsAsync(role))
            .ReturnsAsync(new List<Claim>());
    }

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
