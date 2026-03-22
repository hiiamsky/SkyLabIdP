namespace SkyLabIdP.Application.Common.Interfaces;

/// <summary>
/// 登入通知服務介面
/// 提供登入成功/失敗的郵件通知功能，支援多租戶配置和開發環境覆寫
/// </summary>
public interface ILoginNotificationService
{
    /// <summary>
    /// 發送登入通知郵件
    /// </summary>
    /// <param name="tenantId">租戶識別碼</param>
    /// <param name="userName">使用者名稱</param>
    /// <param name="officialEmail">使用者官方郵件地址（生產環境的收件人）</param>
    /// <param name="isSuccess">登入是否成功</param>
    /// <param name="failureReason">登入失敗原因（僅在 isSuccess=false 時有效）</param>
    /// <param name="ipAddress">客戶端 IP 位址</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>非同步任務</returns>
    /// <remarks>
    /// 此方法會根據租戶配置決定是否發送通知，並在開發環境中支援郵件收件人覆寫。
    /// 所有錯誤都會被內部處理，不會拋出異常以避免影響登入流程。
    /// </remarks>
    Task SendLoginNotificationAsync(
        string tenantId,
        string userName,
        string? officialEmail,
        bool isSuccess,
        string? failureReason,
        string ipAddress,
        CancellationToken cancellationToken = default);
}
