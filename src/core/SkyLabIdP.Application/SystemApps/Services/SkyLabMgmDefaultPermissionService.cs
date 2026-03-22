using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos.Permission;
using SkyLabIdP.Domain.Entities;
using SkyLabIdP.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace SkyLabIdP.Application.SystemApps.Services
{
    /// <summary>
    /// SkyLabMgm 租戶專用的預設權限服務
    /// 提供自訂的權限分配邏輯
    /// </summary>
    public class SkyLabMgmDefaultPermissionService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRoles> roleManager,
        ILogger<SkyLabMgmDefaultPermissionService> logger) : DefaultPermissionService(userManager, roleManager, logger)
    {
        private readonly ILogger<SkyLabMgmDefaultPermissionService> _logger = logger;

        /// <summary>
        /// 覆寫 SkyLabMgm 租戶的預設權限設定邏輯
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="tenantId">租戶 ID</param>
        /// <param name="cancellationToken">取消權杖</param>
        /// <param name="additionalData">額外的資料（例如：ServiceAgency）</param>
        /// <returns></returns>
        public override async Task SetDefaultPermissionsAsync(
            string userId, 
            string tenantId, 
            CancellationToken cancellationToken = default,
            Dictionary<string, object>? additionalData = null)
        {
            _logger.LogInformation("開始為 SkyLabMgm 租戶使用者 {UserId} 設定自訂預設權限", userId);

            // 如果是 SkyLabMgm 租戶，使用自訂邏輯
                if (tenantId == nameof(Tenants.SkyLabmgm))
                {
                    // 預設分配 SkyLabSystemMgmt 角色
                    var roleName = Roles.SkyLabSystemMgmt.GetName();
                    
                    // 獲取 SkyLabMgm 自訂權限配置
                    var customPermissions = GetSkyLabMgmCustomDefaultPermissions();

                    // 使用基底類別的通用方法直接分配角色和權限
                    var success = await AssignDefaultRoleAndPermissionsAsync(
                        userId, 
                        roleName, 
                        customPermissions, 
                        cancellationToken);

                    if (!success)
                    {
                        _logger.LogError("為 SkyLabMgm 租戶使用者 {UserId} 設定自訂預設權限失敗", userId);
                        throw new InvalidOperationException($"設定使用者 {userId} 的預設權限失敗");
                    }

                    _logger.LogInformation("成功為 SkyLabMgm 用戶 {UserId} 分配角色 {Role} 和權限", userId, roleName);
                }
                else
                {
                    // 其他租戶使用基本邏輯
                    await base.SetDefaultPermissionsAsync(userId, tenantId, cancellationToken, additionalData);
                }

                _logger.LogInformation("成功為 SkyLabMgm 租戶使用者 {UserId} 設定自訂預設權限", userId);
        }

        /// <summary>
        /// 獲取 SkyLabMgm 租戶的自訂預設權限配置
        /// 目前回傳空的權限設定，待日後根據實際需求新增
        /// </summary>
        /// <returns></returns>
        private List<DefaultPermissionConfig> GetSkyLabMgmCustomDefaultPermissions()
        {
            _logger.LogDebug("獲取 SkyLabMgm 自訂預設權限配置（目前為空）");

            // 目前不設定任何自訂權限，回傳空列表
            // 待日後根據 SkyLabMgm 租戶的實際需求再新增具體權限設定
            var permissionConfigs = new List<DefaultPermissionConfig>();

            return permissionConfigs;
        }
    }
}
