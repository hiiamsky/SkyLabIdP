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
    /// SkyLabDevelop 租戶專用的預設權限服務
    /// 提供開發業者專用的權限分配邏輯
    /// </summary>
    public class SkyLabDevelopDefaultPermissionService : DefaultPermissionService
    {
        private readonly ILogger<SkyLabDevelopDefaultPermissionService> _logger;

        public SkyLabDevelopDefaultPermissionService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRoles> roleManager,
            ILogger<SkyLabDevelopDefaultPermissionService> logger)
            : base(userManager, roleManager, logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 覆寫 SkyLabDevelop 租戶的預設權限設定邏輯
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="tenantId">租戶 ID</param>
        /// <param name="cancellationToken">取消權杖</param>
        /// <param name="additionalData">額外的資料</param>
        /// <returns></returns>
        public override async Task SetDefaultPermissionsAsync(
            string userId, 
            string tenantId, 
            CancellationToken cancellationToken = default,
            Dictionary<string, object>? additionalData = null)
        {
            _logger.LogInformation("開始為 SkyLabDevelop 租戶使用者 {UserId} 設定專用預設權限", userId);

            // 如果是 SkyLabDevelop 租戶，使用專用邏輯
                if (tenantId == nameof(Tenants.SkyLabdevelop))
                {
                    // 獲取 SkyLabDevelop 自訂權限配置
                    var customPermissions = GetSkyLabDevelopCustomDefaultPermissions();

                    // 使用基底類別的通用方法直接分配角色和權限
                    var success = await AssignDefaultRoleAndPermissionsAsync(
                        userId, 
                        Roles.SkyLabDeveloper.GetName(), 
                        customPermissions, 
                        cancellationToken);

                    if (!success)
                    {
                        _logger.LogError("為 SkyLabDevelop 租戶使用者 {UserId} 設定專用預設權限失敗", userId);
                        throw new InvalidOperationException($"設定使用者 {userId} 的預設權限失敗");
                    }

                    _logger.LogInformation("成功為 SkyLabDevelop 用戶 {UserId} 分配角色和權限", userId);
                }
                else
                {
                    // 其他租戶使用基本邏輯
                    await base.SetDefaultPermissionsAsync(userId, tenantId, cancellationToken, additionalData);
                }

                _logger.LogInformation("成功為 SkyLabDevelop 租戶使用者 {UserId} 設定專用預設權限", userId);
        }



        /// <summary>
        /// 獲取 SkyLabDevelop 租戶的專用預設權限配置
        /// </summary>
        /// <returns></returns>
        private List<DefaultPermissionConfig> GetSkyLabDevelopCustomDefaultPermissions()
        {
            _logger.LogDebug("獲取 SkyLabDevelop 專用預設權限配置");

            var permissionConfigs = new List<DefaultPermissionConfig>();

            return permissionConfigs;
        }

        /// <summary>
        /// 取得使用者（包裝 UserManager 方法以便測試）
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns></returns>
        protected virtual async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            return await UserManager.FindByIdAsync(userId);
        }

        /// <summary>
        /// 為使用者新增 Claim（包裝 UserManager 方法以便測試）
        /// </summary>
        /// <param name="user">使用者</param>
        /// <param name="claim">權限聲明</param>
        /// <returns></returns>
        protected virtual async Task AddUserClaimAsync(ApplicationUser user, Claim claim)
        {
            await UserManager.AddClaimAsync(user, claim);
        }
    }
}
