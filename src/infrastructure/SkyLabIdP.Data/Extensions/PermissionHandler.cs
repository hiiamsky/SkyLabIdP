using System.Security.Claims;
using SkyLabIdP.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SkyLabIdP.Data.Extensions;

/// <summary>
/// 權限授權處理器
/// 使用 IPermissionProvider 從資料庫查詢權限，取代從 JWT Claims 讀取敏感權限資料
/// </summary>
public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PermissionHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                   PermissionRequirement requirement)
    {
        // 📋 從 JWT 取得使用者 ID (僅基本身份資訊，不包含敏感權限資料)
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return; // 使用者未驗證
        }

        using var scope = _serviceScopeFactory.CreateScope();
        var permissionProvider = scope.ServiceProvider.GetRequiredService<IPermissionProvider>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<PermissionHandler>>();

        try
        {
            // 🔄 從資料庫/快取查詢權限，而非從 JWT
            var permissionValue = await permissionProvider
                .GetUserPermissionAsync(userId, requirement.FunctionName);

            // 👤 檢查使用者是否具有所需權限 (位元運算)
            if ((permissionValue & (int)requirement.RequiredPermission) == (int)requirement.RequiredPermission)
            {
                logger.LogDebug("✅ 權限驗證成功 - 使用者: {UserId}, 功能: {FunctionName}", userId, requirement.FunctionName);
                context.Succeed(requirement);
            }
            else
            {
                logger.LogWarning("⚠️ 權限驗證失敗 - 使用者: {UserId}, 功能: {FunctionName}", userId, requirement.FunctionName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ 權限查詢失敗，拒絕存取 - 使用者: {UserId}, 功能: {FunctionName}", userId, requirement.FunctionName);
        }
    }
}
