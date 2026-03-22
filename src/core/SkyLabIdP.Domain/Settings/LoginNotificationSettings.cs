namespace SkyLabIdP.Domain.Settings;

/// <summary>
/// 登入通知設定
/// </summary>
public class LoginNotificationSettings
{
    /// <summary>
    /// 是否啟用登入通知
    /// </summary>
    public bool EnableNotification { get; set; } = true;

    /// <summary>
    /// 開發環境郵件覆寫設定
    /// </summary>
    public DevelopmentEmailOverride DevelopmentEmailOverride { get; set; } = new();

    /// <summary>
    /// 各租戶的通知配置
    /// </summary>
    public Dictionary<string, TenantLoginNotificationConfig> TenantConfigurations { get; set; } = new();
}

/// <summary>
/// 開發環境郵件覆寫設定
/// </summary>
public class DevelopmentEmailOverride
{
    /// <summary>
    /// 是否啟用覆寫（生產環境必須為 false）
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// 覆寫收件人列表
    /// </summary>
    public List<string> Recipients { get; set; } = new();
}

/// <summary>
/// 租戶登入通知配置
/// </summary>
public class TenantLoginNotificationConfig
{
    /// <summary>
    /// 郵件主旨
    /// </summary>
    public string Subject { get; set; } = "系統登入通知";

    /// <summary>
    /// 模板名稱（預留給未來的模板引擎使用）
    /// </summary>
    public string? TemplateName { get; set; }

    /// <summary>
    /// 是否啟用成功登入通知
    /// </summary>
    public bool EnableSuccessNotification { get; set; } = true;

    /// <summary>
    /// 是否啟用失敗登入通知
    /// </summary>
    public bool EnableFailureNotification { get; set; } = true;
}
