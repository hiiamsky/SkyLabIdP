namespace SkyLabIdP.Application.Common.Interfaces;

/// <summary>
/// 預設權限服務介面
/// </summary>
public interface IDefaultPermissionService
{
    /// <summary>
    /// 為指定用戶設定租戶預設權限
    /// </summary>
    /// <param name="userId">用戶 ID</param>
    /// <param name="tenantId">租戶 ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <param name="additionalData">額外的資料（例如：ServiceAgency）</param>
    /// <returns></returns>
    Task SetDefaultPermissionsAsync(string userId, string tenantId, CancellationToken cancellationToken, Dictionary<string, object>? additionalData = null);
}
