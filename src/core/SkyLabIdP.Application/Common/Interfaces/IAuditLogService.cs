using System;
using SkyLabIdP.Domain.Entities;

namespace SkyLabIdP.Application.Common.Interfaces;

public interface IAuditLogService
{    Task LogAsync(AuditLog auditLog);
    
    /// <summary>
    /// 將審計日誌寫入資料庫
    /// </summary>
    /// <param name="auditLog">審計日誌實體</param>
    /// <returns>任務</returns>
    Task LogToDatabaseAsync(AuditLog auditLog);
    
    /// <summary>
    /// 將審計日誌寫入檔案
    /// </summary>
    /// <param name="auditLog">審計日誌實體</param>
    /// <returns>任務</returns>
    Task LogToFileAsync(AuditLog auditLog);
}
