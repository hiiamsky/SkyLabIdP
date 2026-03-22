using System;
using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Domain.Entities;
using SkyLabIdP.WebApi.Helpers.Utilities;
using Serilog;
using Serilog.Formatting.Json;

namespace SkyLabIdP.WebApi.Services;

/// <summary>
/// 使用 Serilog 實現審計日誌服務
/// </summary>
public class SerilogAuditLogService : IAuditLogService
{
    private readonly IConfiguration _configuration;
    private readonly Serilog.ILogger _auditLogger;

    /// <summary>
    /// 初始化 <see cref="SerilogAuditLogService"/> 類的新實例
    /// </summary>
    /// <param name="configuration">應用程式配置</param>
    public SerilogAuditLogService(IConfiguration configuration)
    {
        _configuration = configuration;

        try
        {
            // 建立專用於審計日誌的 Serilog 記錄器
            var loggerConfig = new LoggerConfiguration()
                .Enrich.WithProperty("SourceContext", "Audit")
                .Enrich.FromLogContext() // 確保從日誌上下文中讀取所有屬性
                .Enrich.WithProperty("Application", "SkyLabDocMgm2g"); // 添加應用程式標識

            // 嘗試從配置中讀取 Serilog 設定
            var serilogSection = configuration.GetSection("Serilog");
            if (serilogSection != null)
            {
                loggerConfig = loggerConfig.ReadFrom.Configuration(configuration);
            }

            // 確保使用 JSON 格式輸出到文件，並包含所有屬性
            // 使用更嚴格的 JSON 格式化選項，確保每行一個完整的 JSON 記錄
            loggerConfig = loggerConfig.WriteTo.File(
                path: "Logs/AuditLogs/audit_.json",
                rollingInterval: RollingInterval.Day,
                formatter: new JsonFormatter(renderMessage: true, formatProvider: null, closingDelimiter: "\n"),
                buffered: false,  // 不緩衝輸出，確保每次寫入立即刷新
                shared: true,     // 允許多個進程共享同一個日誌文件
                flushToDiskInterval: TimeSpan.FromSeconds(1)); // 定期將緩衝區刷新到磁盤

            // 設置豐富化選項，確保所有屬性都被記錄
            loggerConfig = loggerConfig.Enrich.WithProperty("Application", "SkyLabDocMgm2g")
                                     .Enrich.FromLogContext();

            _auditLogger = loggerConfig.CreateLogger().ForContext("SourceContext", "Audit");
        }
        catch (Exception ex)
        {
            // 如果配置出錯，創建一個最小化的記錄器
            _auditLogger = new LoggerConfiguration()
                .Enrich.WithProperty("SourceContext", "Audit")
                .Enrich.WithProperty("Application", "SkyLabDocMgm2g")
                .Enrich.FromLogContext()
                .WriteTo.File(
                    path: "Logs/AuditLogs/audit_fallback_.json",
                    rollingInterval: RollingInterval.Day,
                    formatter: new JsonFormatter(renderMessage: true, formatProvider: null, closingDelimiter: "\n"),
                    buffered: false,  // 不緩衝輸出，確保每次寫入立即刷新
                    shared: true,     // 允許多個進程共享同一個日誌文件
                    flushToDiskInterval: TimeSpan.FromSeconds(1)) // 定期將緩衝區刷新到磁盤
                .CreateLogger()
                .ForContext("SourceContext", "Audit");

            // 記錄初始化失敗
            _auditLogger.Error(ex, "審計日誌初始化失敗，使用備用配置");
        }
    }

    /// <summary>
    /// 記錄審計日誌
    /// </summary>
    /// <param name="auditLog">審計日誌實體</param>
    /// <returns>任務</returns>
    public async Task LogAsync(AuditLog auditLog)
    {
        try
        {
            // 確保所有屬性不為 null，避免日誌格式錯誤
            auditLog.IPAddress = auditLog.IPAddress ?? "Unknown";
            auditLog.UserName = auditLog.UserName ?? "Anonymous";
            auditLog.UserId = auditLog.UserId ?? "Unknown";
            auditLog.TraceId = auditLog.TraceId ?? "Unknown";
            auditLog.RequestMethod = auditLog.RequestMethod ?? "Unknown";
            auditLog.RequestPath = auditLog.RequestPath ?? "Unknown";
            auditLog.RequestQueryString = auditLog.RequestQueryString ?? "";
            auditLog.UserAgent = auditLog.UserAgent ?? "";

            var auditLogSettings = _configuration?.GetSection("AuditLog");

            // 如果配置節點不存在或未啟用，則預設啟用檔案記錄
            bool fileEnabled = true;  // 預設啟用檔案記錄
            bool dbEnabled = false;   // 預設禁用資料庫記錄

            if (auditLogSettings != null)
            {
                var fileSettingsSection = auditLogSettings.GetSection("FileSettings");
                var fileEnabledValue = fileSettingsSection?["Enabled"];
                if (fileSettingsSection != null && !string.IsNullOrEmpty(fileEnabledValue))
                {
                    if (bool.TryParse(fileEnabledValue, out bool parsedValue))
                    {
                        fileEnabled = parsedValue;
                    }
                }

                var dbSettingsSection = auditLogSettings.GetSection("DatabaseSettings");
                var dbEnabledValue = dbSettingsSection?["Enabled"];
                if (dbSettingsSection != null && !string.IsNullOrEmpty(dbEnabledValue))
                {
                    if (bool.TryParse(dbEnabledValue, out bool parsedValue))
                    {
                        dbEnabled = parsedValue;
                    }
                }
            }

            if (fileEnabled)
            {
                await LogToFileAsync(auditLog);
            }

            if (dbEnabled)
            {
                await LogToDatabaseAsync(auditLog);
            }
        }
        catch (Exception ex)
        {
            // 如果記錄過程中發生錯誤，嘗試使用 fallback 記錄器記錄錯誤
            _auditLogger.Error(ex, "記錄審計日誌時發生錯誤");
        }
    }

    /// <summary>
    /// 將審計日誌寫入資料庫
    /// </summary>
    /// <param name="auditLog">審計日誌實體</param>
    /// <returns>任務</returns>
    public Task LogToDatabaseAsync(AuditLog auditLog)
    {
        // 資料庫記錄已禁用，返回完成的任務
        return Task.CompletedTask;
    }

    /// <summary>
    /// 將審計日誌寫入檔案
    /// </summary>
    /// <param name="auditLog">審計日誌實體</param>
    /// <returns>任務</returns>
    public Task LogToFileAsync(AuditLog auditLog)
    {
        // 創建包含所有審計屬性的結構化日誌，確保所有字段都有正確的值
        // 使用 ForContext 而不是直接在日誌消息中傳遞屬性，以確保所有屬性都被記錄
        var loggerWithContext = _auditLogger
            .ForContext("IPAddress", string.IsNullOrEmpty(auditLog.IPAddress) ? "Unknown" : auditLog.IPAddress)
            .ForContext("UserName", string.IsNullOrEmpty(auditLog.UserName) ? "Anonymous" : auditLog.UserName)
            .ForContext("UserId", string.IsNullOrEmpty(auditLog.UserId) ? "Unknown" : auditLog.UserId)
            .ForContext("TraceId", string.IsNullOrEmpty(auditLog.TraceId) ? "Unknown" : auditLog.TraceId)
            .ForContext("RequestMethod", string.IsNullOrEmpty(auditLog.RequestMethod) ? "Unknown" : auditLog.RequestMethod)
            .ForContext("RequestPath", string.IsNullOrEmpty(auditLog.RequestPath) ? "Unknown" : auditLog.RequestPath)
            .ForContext("RequestQueryString", auditLog.RequestQueryString ?? string.Empty)
            .ForContext("StatusCode", auditLog.StatusCode)
            .ForContext("ExecutionTime", auditLog.ExecutionTime)
            .ForContext("Timestamp", auditLog.Timestamp)
            .ForContext("UserAgent", auditLog.UserAgent ?? string.Empty);

        // 使用 Serilog 記錄結構化的審計日誌（JSON 格式）
        loggerWithContext.Information("Audit Log: {RequestMethod} {RequestPath} {StatusCode}",
            auditLog.RequestMethod, auditLog.RequestPath, auditLog.StatusCode);

        // 記錄請求和回應主體（如果有需要、不是敏感資訊、且不是二進制內容）
        if (!string.IsNullOrEmpty(auditLog.RequestBody) &&
            !ContentUtility.IsSensitiveContent(auditLog.RequestBody) &&
            !ContentUtility.IsBinaryContent(auditLog.RequestMethod, auditLog.RequestPath, auditLog.RequestBody))
        {
            // 限制請求主體的大小，避免日誌格式被破壞
            var truncatedRequestBody = auditLog.RequestBody.Length > 5000
                ? auditLog.RequestBody.Substring(0, 5000) + "... [截斷，完整長度: " + auditLog.RequestBody.Length + "]"
                : auditLog.RequestBody;

            loggerWithContext.ForContext("RequestBody", truncatedRequestBody)
                .Verbose("Request Body");
        }
        else if (!string.IsNullOrEmpty(auditLog.RequestBody))
        {
            loggerWithContext.Verbose("Request Body: [二進制或敏感內容已過濾]");
        }

        if (!string.IsNullOrEmpty(auditLog.ResponseBody) &&
            !ContentUtility.IsSensitiveContent(auditLog.ResponseBody) &&
            !ContentUtility.IsBinaryContent(auditLog.RequestMethod, auditLog.RequestPath, auditLog.ResponseBody))
        {
            // 限制響應主體的大小，避免日誌格式被破壞
            var truncatedResponseBody = auditLog.ResponseBody.Length > 5000
                ? auditLog.ResponseBody.Substring(0, 5000) + "... [截斷，完整長度: " + auditLog.ResponseBody.Length + "]"
                : auditLog.ResponseBody;

            loggerWithContext.ForContext("ResponseBody", truncatedResponseBody)
                .Verbose("Response Body");
        }
        else if (!string.IsNullOrEmpty(auditLog.ResponseBody))
        {
            loggerWithContext.Verbose("Response Body: [二進制或敏感內容已過濾]");
        }

        return Task.CompletedTask;
    }


 
}
