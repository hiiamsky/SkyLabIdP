using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Domain.Entities;
using SkyLabIdP.Domain.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SkyLabIdP.Data.Services;

public class AuditLogService : IAuditLogService
{
    private readonly ILogger<AuditLogService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AuditLogSettings _settings;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuditLogService(
        ILogger<AuditLogService> logger,
        IUnitOfWork unitOfWork,
        IOptions<AuditLogSettings> settings)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _settings = settings.Value;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }

    public async Task LogAsync(AuditLog auditLog)
    {
        var tasks = new List<Task>();

        try
        {
            // 檢查是否啟用資料庫記錄
            if (_settings.DbSettings.Enabled)
            {
                tasks.Add(LogToDatabaseAsync(auditLog));
            }

            // 檢查是否啟用檔案記錄
            if (_settings.FileAuditSettings.Enabled)
            {
                tasks.Add(LogToFileAsync(auditLog));
            }

            // 等待所有日誌任務完成
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            // 如果發生錯誤，記錄但不阻止應用程序繼續運行
            _logger.LogError(ex, "Failed to save audit log: {@AuditLog}", auditLog);
        }
    }

    public async Task LogToDatabaseAsync(AuditLog auditLog)
    {
        try
        {
            await _unitOfWork.AuditLogs.AddAsync(auditLog);
            _logger.LogInformation("Audit log saved to database: {Id}", auditLog.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save audit log to database: {Id}", auditLog.Id);
        }
    }

    public async Task LogToFileAsync(AuditLog auditLog)
    {
        try
        {
            // 確保目錄存在
            Directory.CreateDirectory(_settings.FileAuditSettings.FilePath);

            // 根據設定的滾動間隔生成檔案名稱
            string fileName = GenerateFileName();
            string fullPath = Path.Combine(_settings.FileAuditSettings.FilePath, fileName);

            // 序列化審計日誌
            string logJson = JsonSerializer.Serialize(auditLog, _jsonOptions);

            // 寫入檔案（追加模式）
            await File.AppendAllTextAsync(fullPath, logJson + Environment.NewLine);

            _logger.LogInformation("Audit log saved to file: {FilePath}", fullPath);

            // 清理舊檔案
            CleanupOldFiles();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save audit log to file: {Id}", auditLog.Id);
        }
    }

    private string GenerateFileName()
    {
        // 根據設定的滾動間隔格式化日期
        string dateSuffix = _settings.FileAuditSettings.RollingInterval.ToLower() switch
        {
            "day" => DateTime.Now.ToString("yyyyMMdd"),
            "hour" => DateTime.Now.ToString("yyyyMMddHH"),
            "minute" => DateTime.Now.ToString("yyyyMMddHHmm"),
            _ => DateTime.Now.ToString("yyyyMMdd") // 默認每天
        };

        // 從基本檔名獲取不帶副檔名的部分，並添加日期
        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(_settings.FileAuditSettings.FileName);
        string extension = Path.GetExtension(_settings.FileAuditSettings.FileName);

        return $"{fileNameWithoutExt}{dateSuffix}{extension}";
    }

    private void CleanupOldFiles()
    {
        try
        {
            // 獲取保留的檔案數量限制
            int retainedFileCount = _settings.FileAuditSettings.RetainedFileCountLimit;
            if (retainedFileCount <= 0)
                return;

            // 獲取目錄下的所有審計日誌檔案（按照檔名前綴篩選）
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(_settings.FileAuditSettings.FileName);
            string extension = Path.GetExtension(_settings.FileAuditSettings.FileName);
            string searchPattern = $"{fileNameWithoutExt}*{extension}";

            var directory = new DirectoryInfo(_settings.FileAuditSettings.FilePath);
            var files = directory.GetFiles(searchPattern)
                                 .OrderByDescending(f => f.LastWriteTime)
                                 .Skip(retainedFileCount);

            // 刪除超出保留數量的舊檔案
            foreach (var file in files)
            {
                file.Delete();
                _logger.LogInformation("Deleted old audit log file: {FileName}", file.FullName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup old audit log files");
        }
    }
}
