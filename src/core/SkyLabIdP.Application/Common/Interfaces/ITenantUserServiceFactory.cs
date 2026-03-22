using SkyLabIdP.Application.Common.Interfaces;

namespace SkyLabIdP.Application.Common.Interfaces;

/// <summary>
/// 多租戶用戶服務工廠
/// 提供基於當前租戶取得對應之使用者登入處理服務的能力
/// </summary>
public interface ITenantUserServiceFactory
{
    /// <summary>
    /// 根據 HttpRequest 解析出的當前租戶，取得對應之使用者的登入服務
    /// </summary>
    IUserService GetCurrentTenantService();

    /// <summary>
    /// 根據傳入的特定租戶 ID，取得對應此租戶之使用者的登入服務
    /// </summary>
    IUserService GetServiceByTenantId(string tenantId);
}
