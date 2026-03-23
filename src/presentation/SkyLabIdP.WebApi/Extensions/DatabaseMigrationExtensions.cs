using DbUp;
using DbUp.Engine;
using Serilog;

namespace SkyLabIdP.WebApi.Extensions;
/// <summary>
/// 提供資料庫遷移相關的擴充方法，使用 DbUp 執行 SQL 腳本來管理資料庫版本
/// </summary>
public static class DatabaseMigrationExtensions
{
    /// <summary>
    /// 執行 DbUp 資料庫遷移：先套用 baseline（僅空資料庫），再執行 db/scripts/ 下的增量腳本
    /// </summary>
    public static WebApplication ApplyDatabaseMigrations(this WebApplication app)
    {
        var connectionString = app.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing 'DefaultConnection' connection string.");

        var baseDir = AppContext.BaseDirectory;

        // Phase 1: baseline.sql — 使用獨立的 journal table 確保只在空資料庫時執行
        var baselinePath = Path.Combine(baseDir, "db", "baseline.sql");
        if (File.Exists(baselinePath))
        {
            var baselineUpgrader = DeployChanges.To
                .SqlDatabase(connectionString)
                .WithScript(new SqlScript("baseline.sql", File.ReadAllText(baselinePath)))
                .WithTransactionPerScript()
                .JournalToSqlTable("dbo", "SchemaVersions")
                .LogTo(new SerilogDbUpLog())
                .Build();

            var baselineResult = baselineUpgrader.PerformUpgrade();
            if (!baselineResult.Successful)
            {
                Log.Fatal(baselineResult.Error, "DbUp baseline migration failed");
                throw baselineResult.Error;
            }
        }

        // Phase 2: db/scripts/ — 增量腳本，依檔名字母序執行
        var scriptsPath = Path.Combine(baseDir, "db", "scripts");
        if (Directory.Exists(scriptsPath))
        {
            var scriptsUpgrader = DeployChanges.To
                .SqlDatabase(connectionString)
                .WithScriptsFromFileSystem(scriptsPath, new DbUp.ScriptProviders.FileSystemScriptOptions
                {
                    IncludeSubDirectories = false,
                    Extensions = new[] { ".sql" }
                })
                .WithTransactionPerScript()
                .JournalToSqlTable("dbo", "SchemaVersions")
                .LogTo(new SerilogDbUpLog())
                .Build();

            var scriptsResult = scriptsUpgrader.PerformUpgrade();
            if (!scriptsResult.Successful)
            {
                Log.Fatal(scriptsResult.Error, "DbUp scripts migration failed");
                throw scriptsResult.Error;
            }
        }

        Log.Information("Database migrations applied successfully");
        return app;
    }

    /// <summary>
    /// 將 DbUp 日誌導向 Serilog
    /// </summary>
    private sealed class SerilogDbUpLog : DbUp.Engine.Output.IUpgradeLog
    {
        public void WriteInformation(string format, params object[] args) =>
            Log.Information(format, args);

        public void WriteError(string format, params object[] args) =>
            Log.Error(format, args);

        public void WriteWarning(string format, params object[] args) =>
            Log.Warning(format, args);
    }
}
