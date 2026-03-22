using System;

namespace SkyLabIdP.Domain.Settings;
public class AuditLogSettings
{
    public FileSettings FileAuditSettings { get; set; } = new FileSettings();
    public DatabaseSettings DbSettings { get; set; } = new DatabaseSettings();
    public List<string> ExcludedPaths { get; set; } = new List<string>();
    
    public class FileSettings
    {
        public bool Enabled { get; set; } = true;
        public string FilePath { get; set; } = "./Logs/AuditLogs";
        public string FileName { get; set; } = "audit_.txt";
        public string RollingInterval { get; set; } = "Day";
        public int RetainedFileCountLimit { get; set; } = 31;
    }
    
    public class DatabaseSettings
    {
        public bool Enabled { get; set; } = true;
    }
}
