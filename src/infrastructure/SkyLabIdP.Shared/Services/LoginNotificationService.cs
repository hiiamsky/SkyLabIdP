using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos.Email;
using SkyLabIdP.Domain.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SkyLabIdP.Shared.Services;

/// <summary>
/// 登入通知服務實作
/// 提供登入成功/失敗的郵件通知功能，支援多租戶配置和開發環境覆寫
/// </summary>
public class LoginNotificationService : ILoginNotificationService
{
    private readonly IEmailService _emailService;
    private readonly IOptions<LoginNotificationSettings> _loginNotificationSettings;
    private readonly IOptions<MailSettings> _mailSettings;
    private readonly ILogger<LoginNotificationService> _logger;
    private readonly IHostEnvironment _environment;

    public LoginNotificationService(
        IEmailService emailService,
        IOptions<LoginNotificationSettings> loginNotificationSettings,
        IOptions<MailSettings> mailSettings,
        ILogger<LoginNotificationService> logger,
        IHostEnvironment environment)
    {
        _emailService = emailService;
        _loginNotificationSettings = loginNotificationSettings;
        _mailSettings = mailSettings;
        _logger = logger;
        _environment = environment;
    }

    public async Task SendLoginNotificationAsync(
        string tenantId,
        string userName,
        string? officialEmail,
        bool isSuccess,
        string? failureReason,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = _loginNotificationSettings.Value;
            
            // 檢查總開關
            if (!settings.EnableNotification)
            {
                _logger.LogDebug("登入通知功能已停用，跳過發送通知");
                return;
            }

            // 取得租戶配置
            var tenantConfig = GetTenantConfiguration(tenantId, settings);
            
            // 檢查該租戶是否啟用該類型的通知
            if (!ShouldSendNotification(isSuccess, tenantConfig))
            {
                _logger.LogDebug("租戶 {TenantId} 未啟用 {NotificationType} 通知，跳過發送", 
                    tenantId, isSuccess ? "成功" : "失敗");
                return;
            }

            // 決定收件人
            var recipients = GetRecipients(officialEmail, settings);
            if (!recipients.Any())
            {
                _logger.LogWarning("無法取得有效的收件人清單，跳過發送通知。TenantId: {TenantId}, UserName: {UserName}", 
                    tenantId, userName);
                return;
            }

            // 建構郵件內容
            var emailDto = BuildEmailDto(
                recipients,
                tenantConfig,
                tenantId,
                userName,
                isSuccess,
                failureReason,
                ipAddress);

            // 發送郵件
            await _emailService.SendAsync(emailDto);

            _logger.LogInformation("登入通知郵件已發送。TenantId: {TenantId}, UserName: {UserName}, Success: {IsSuccess}, Recipients: {RecipientCount}", 
                tenantId, userName, isSuccess, recipients.Count);
        }
        catch (Exception ex)
        {
            // 錯誤隔離：記錄錯誤但不拋出異常，避免影響登入流程
            _logger.LogWarning(ex, "發送登入通知時發生錯誤。TenantId: {TenantId}, UserName: {UserName}, Success: {IsSuccess}", 
                tenantId, userName, isSuccess);
        }
    }

    /// <summary>
    /// 取得租戶配置，如果找不到則使用預設值
    /// </summary>
    private TenantLoginNotificationConfig GetTenantConfiguration(string tenantId, LoginNotificationSettings settings)
    {
        if (settings.TenantConfigurations.TryGetValue(tenantId, out var config))
        {
            return config;
        }

        _logger.LogDebug("租戶 {TenantId} 未找到專屬配置，使用預設配置", tenantId);
        return new TenantLoginNotificationConfig
        {
            Subject = "系統登入通知",
            EnableSuccessNotification = true,
            EnableFailureNotification = true
        };
    }

    /// <summary>
    /// 判斷是否應該發送通知
    /// </summary>
    private static bool ShouldSendNotification(bool isSuccess, TenantLoginNotificationConfig tenantConfig)
    {
        return isSuccess ? tenantConfig.EnableSuccessNotification : tenantConfig.EnableFailureNotification;
    }

    /// <summary>
    /// 決定收件人清單
    /// </summary>
    private List<string> GetRecipients(string? officialEmail, LoginNotificationSettings settings)
    {
        // 開發環境且啟用覆寫：使用覆寫收件人
        if (_environment.IsDevelopment() && 
            settings.DevelopmentEmailOverride?.Enabled == true && 
            settings.DevelopmentEmailOverride.Recipients.Any())
        {
            _logger.LogDebug("使用開發環境覆寫收件人清單：{Recipients}", 
                string.Join(", ", settings.DevelopmentEmailOverride.Recipients));
            return settings.DevelopmentEmailOverride.Recipients.ToList();
        }

        // 生產環境或未啟用覆寫：使用使用者官方郵件
        if (!string.IsNullOrWhiteSpace(officialEmail))
        {
            return new List<string> { officialEmail };
        }

        // 無有效收件人
        return new List<string>();
    }

    /// <summary>
    /// 建構郵件 DTO
    /// </summary>
    private EmailDto BuildEmailDto(
        List<string> recipients,
        TenantLoginNotificationConfig tenantConfig,
        string tenantId,
        string userName,
        bool isSuccess,
        string? failureReason,
        string ipAddress)
    {
        var subject = BuildSubject(tenantConfig.Subject, isSuccess);
        var body = BuildEmailBody(tenantId, userName, isSuccess, failureReason, ipAddress);

        return new EmailDto
        {
            To = recipients,
            Subject = subject,
            Body = body,
            From = _mailSettings.Value.EmailFrom
        };
    }

    /// <summary>
    /// 建構郵件主旨
    /// </summary>
    private static string BuildSubject(string baseSubject, bool isSuccess)
    {
        var status = isSuccess ? "登入成功" : "登入失敗";
        return $"{baseSubject} - {status}";
    }

    /// <summary>
    /// 建構郵件內容（HTML 格式）
    /// </summary>
    private string BuildEmailBody(
        string tenantId,
        string userName,
        bool isSuccess,
        string? failureReason,
        string ipAddress)
    {
        var timestamp = DateTime.UtcNow.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss"); // 台灣時間
        var environment = _environment.EnvironmentName;
        var statusText = isSuccess ? "成功" : "失敗";
        var statusColor = isSuccess ? "#28a745" : "#dc3545"; // 綠色/紅色

        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>登入通知</title>
</head>
<body style=""font-family: Arial, sans-serif; margin: 20px; line-height: 1.6;"">
    <div style=""max-width: 600px; margin: 0 auto; border: 1px solid #ddd; border-radius: 8px; padding: 20px;"">
        <h3 style=""color: {statusColor}; margin-top: 0;"">
            {(isSuccess ? "🔓" : "🚨")} 使用者登入{statusText}通知
        </h3>
        
        <table style=""width: 100%; border-collapse: collapse; margin: 20px 0;"">
            <tr>
                <td style=""padding: 8px; border-bottom: 1px solid #eee; font-weight: bold; width: 120px;"">租戶系統：</td>
                <td style=""padding: 8px; border-bottom: 1px solid #eee;"">{tenantId}</td>
            </tr>
            <tr>
                <td style=""padding: 8px; border-bottom: 1px solid #eee; font-weight: bold;"">使用者名稱：</td>
                <td style=""padding: 8px; border-bottom: 1px solid #eee;"">{userName}</td>
            </tr>
            <tr>
                <td style=""padding: 8px; border-bottom: 1px solid #eee; font-weight: bold;"">登入狀態：</td>
                <td style=""padding: 8px; border-bottom: 1px solid #eee; color: {statusColor}; font-weight: bold;"">{statusText}</td>
            </tr>
            <tr>
                <td style=""padding: 8px; border-bottom: 1px solid #eee; font-weight: bold;"">登入時間：</td>
                <td style=""padding: 8px; border-bottom: 1px solid #eee;"">{timestamp} (UTC+8)</td>
            </tr>
            <tr>
                <td style=""padding: 8px; border-bottom: 1px solid #eee; font-weight: bold;"">IP 位址：</td>
                <td style=""padding: 8px; border-bottom: 1px solid #eee;"">{ipAddress}</td>
            </tr>
            <tr>
                <td style=""padding: 8px; border-bottom: 1px solid #eee; font-weight: bold;"">環境：</td>
                <td style=""padding: 8px; border-bottom: 1px solid #eee;"">{environment}</td>
            </tr>";

        // 如果是失敗，加入失敗原因
        if (!isSuccess && !string.IsNullOrWhiteSpace(failureReason))
        {
            body += $@"
            <tr>
                <td style=""padding: 8px; border-bottom: 1px solid #eee; font-weight: bold;"">失敗原因：</td>
                <td style=""padding: 8px; border-bottom: 1px solid #eee; color: #dc3545;"">{failureReason}</td>
            </tr>";
        }

        body += @"
        </table>";

        // 如果是失敗，加入安全提醒
        if (!isSuccess)
        {
            body += @"
        <div style=""background-color: #fff3cd; border: 1px solid #ffeaa7; border-radius: 4px; padding: 15px; margin: 20px 0;"">
            <p style=""margin: 0; color: #856404;"">
                <strong>⚠️ 安全提醒：</strong>這可能是安全威脅的跡象，請檢查相關日誌並評估是否需要採取進一步的安全措施。
            </p>
        </div>";
        }

        body += @"
        <hr style=""margin: 30px 0; border: none; border-top: 1px solid #eee;"">
        <p style=""font-size: 12px; color: #666; margin: 0;"">
            此為系統自動發送的通知郵件，請勿回覆。<br>
            如有疑問，請聯繫系統管理員。
        </p>
    </div>
</body>
</html>";

        return body;
    }
}
