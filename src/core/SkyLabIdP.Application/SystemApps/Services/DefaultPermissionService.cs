using System.Security.Claims;
using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos.Permission;
using SkyLabIdP.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace SkyLabIdP.Application.SystemApps.Services;

/// <summary>
/// 預設權限服務實作
/// </summary>
public class DefaultPermissionService : IDefaultPermissionService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRoles>? _roleManager;
    private readonly ILogger<DefaultPermissionService> _logger;

    /// <summary>
    /// 提供給子類存取的 UserManager
    /// </summary>
    protected UserManager<ApplicationUser> UserManager => _userManager;

    /// <summary>
    /// 提供給子類存取的 RoleManager
    /// </summary>
    protected RoleManager<ApplicationRoles>? RoleManager => _roleManager;

    public DefaultPermissionService(
        UserManager<ApplicationUser> userManager, 
        ILogger<DefaultPermissionService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public DefaultPermissionService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRoles> roleManager,
        ILogger<DefaultPermissionService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    /// <summary>
    /// 為指定用戶設定租戶預設權限
    /// 基礎實作：子類別應該覆寫此方法來提供租戶特定的權限設定
    /// </summary>
    /// <param name="userId">用戶 ID</param>
    /// <param name="tenantId">租戶 ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <param name="additionalData">額外的資料（例如：ServiceAgency）</param>
    public virtual async Task SetDefaultPermissionsAsync(string userId, string tenantId, CancellationToken cancellationToken, Dictionary<string, object>? additionalData = null)
    {
        _logger.LogWarning("使用基礎的 SetDefaultPermissionsAsync 方法，租戶 {TenantId} 應該使用專屬的權限服務", tenantId);
        await Task.CompletedTask;
    }

    /// <summary>
    /// 為使用者分配預設角色和權限的通用方法
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    /// <param name="defaultRole">預設角色</param>
    /// <param name="customPermissions">額外的自訂權限配置</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns></returns>
    protected virtual async Task<bool> AssignDefaultRoleAndPermissionsAsync(
        string userId,
        string defaultRole,
        List<DefaultPermissionConfig>? customPermissions = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 取得使用者
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("找不到用戶 ID: {UserId}", userId);
                return false;
            }

            // 1. 分配角色
            if (!string.IsNullOrEmpty(defaultRole))
            {
                var roleResult = await _userManager.AddToRoleAsync(user, defaultRole);
                if (!roleResult.Succeeded)
                {
                    _logger.LogError("分配角色失敗: {Errors}", string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    return false;
                }

                // 2. 獲取角色權限並複製到用戶Claims
                if (_roleManager != null)
                {
                    var role = await _roleManager.FindByNameAsync(defaultRole);
                    if (role != null)
                    {
                        var roleClaims = await _roleManager.GetClaimsAsync(role);
                        
                        // 將角色權限複製到用戶權限
                        foreach (var roleClaim in roleClaims)
                        {
                            var userClaimResult = await _userManager.AddClaimAsync(user, roleClaim);
                            if (!userClaimResult.Succeeded)
                            {
                                _logger.LogWarning("添加用戶權限失敗: {ClaimType} = {ClaimValue}", 
                                    roleClaim.Type, roleClaim.Value);
                            }
                        }
                        
                        _logger.LogInformation("為用戶 {UserId} 分配角色 {Role} 和 {ClaimCount} 個權限", 
                            user.Id, defaultRole, roleClaims.Count);
                    }
                }
            }

            // 3. 添加自訂權限
            if (customPermissions?.Any() == true)
            {
                await AddCustomPermissionsAsync(user, customPermissions);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "分配用戶角色和權限時發生錯誤");
            return false;
        }
    }

    /// <summary>
    /// 添加自訂權限到使用者
    /// </summary>
    /// <param name="user">使用者</param>
    /// <param name="customPermissions">自訂權限配置</param>
    /// <returns></returns>
    protected virtual async Task AddCustomPermissionsAsync(ApplicationUser user, List<DefaultPermissionConfig> customPermissions)
    {
        _logger.LogDebug("為使用者 {UserId} 添加自訂權限", user.Id);

        // 建立 Claims 並儲存
        foreach (var permission in customPermissions)
        {
            var claimType = $"{permission.FunctionName}.Permissions";
            var claimValue = permission.PermissionValue.ToString();
            var claim = new Claim(claimType, claimValue);
            
            var result = await _userManager.AddClaimAsync(user, claim);
            if (!result.Succeeded)
            {
                _logger.LogWarning("添加自訂權限失敗: {ClaimType} = {ClaimValue}, 錯誤: {Errors}", 
                    claimType, claimValue, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        _logger.LogInformation(
            "成功為使用者 {UserId} 設定了 {PermissionCount} 個自訂權限",
            user.Id,
            customPermissions.Count);
    }
}
