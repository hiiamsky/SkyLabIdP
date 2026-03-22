namespace SkyLabIdP.Application.Common.Interfaces;

/// <summary>
/// 權限提供者服務介面
/// 負責從資料庫查詢使用者權限並提供快取機制，取代從 JWT Claims 讀取敏感權限資料
/// </summary>
public interface IPermissionProvider
{
    /// <summary>
    /// 取得使用者特定功能的權限值
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    /// <param name="functionName">功能名稱 (例如: DocCatalog, CaseFileData)</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>權限值 (位元運算，例如: 511 = 所有權限)</returns>
    Task<int> GetUserPermissionAsync(string userId, string functionName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 取得使用者所有功能的權限
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>功能名稱與權限值的對應字典</returns>
    Task<Dictionary<string, int>> GetUserAllPermissionsAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 清除使用者所有權限快取
    /// 當使用者權限異動時，需要呼叫此方法確保權限即時生效
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    /// <param name="cancellationToken">取消權杖</param>
    Task InvalidateUserPermissionCacheAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 清除特定功能的權限快取
    /// 當特定功能權限異動時，可呼叫此方法進行精確的快取失效
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    /// <param name="functionName">功能名稱</param>
    /// <param name="cancellationToken">取消權杖</param>
    Task InvalidateFunctionPermissionCacheAsync(string userId, string functionName, CancellationToken cancellationToken = default);
}
