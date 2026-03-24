using DbUp;
using DbUp.Engine;
using Respawn;
using Testcontainers.MsSql;
using Testcontainers.Redis;

namespace Application.IntegrationTests.Fixtures;

public class IntegrationTestFixture : IAsyncLifetime
{
    private MsSqlContainer _msSqlContainer = null!;
    private RedisContainer _redisContainer = null!;

    public string DbConnectionString { get; private set; } = null!;
    public string RedisHost { get; private set; } = null!;
    public int RedisPort { get; private set; }
    public string ApiKey => "integration-test-api-key";

    public CustomWebApplicationFactory Factory { get; private set; } = null!;
    private Respawner? _respawner;

    public async Task InitializeAsync()
    {
        // 1. 啟動 SQL Server 容器
        _msSqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();
        await _msSqlContainer.StartAsync();
        DbConnectionString = _msSqlContainer.GetConnectionString();

        // 2. 啟動 Redis 容器
        _redisContainer = new RedisBuilder("redis:7-alpine")
            .Build();
        await _redisContainer.StartAsync();
        RedisHost = _redisContainer.Hostname;
        RedisPort = _redisContainer.GetMappedPublicPort(6379);

        // 3. 使用 DbUp 初始化 schema
        ApplyMigrations();

        // 4. 建立 WebApplicationFactory
        Factory = new CustomWebApplicationFactory
        {
            DbConnectionString = DbConnectionString,
            RedisHost = RedisHost,
            RedisPort = RedisPort,
            ApiKey = ApiKey
        };

        // 5. 建立 Respawn checkpoint
        using var conn = new Microsoft.Data.SqlClient.SqlConnection(DbConnectionString);
        await conn.OpenAsync();
        _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            TablesToIgnore = [new Respawn.Graph.Table("SchemaVersions")]
        });
    }

    public async Task ResetDatabaseAsync()
    {
        if (_respawner is null) return;
        using var conn = new Microsoft.Data.SqlClient.SqlConnection(DbConnectionString);
        await conn.OpenAsync();
        await _respawner.ResetAsync(conn);
    }

    public async Task DisposeAsync()
    {
        if (Factory is not null)
            await Factory.DisposeAsync();
        if (_msSqlContainer is not null)
            await _msSqlContainer.DisposeAsync();
        if (_redisContainer is not null)
            await _redisContainer.DisposeAsync();
    }

    private void ApplyMigrations()
    {
        // 尋找 db/ 目錄（從解決方案根目錄）
        var baseDir = FindSolutionRoot();

        // Phase 1: baseline.sql
        var baselinePath = Path.Combine(baseDir, "db", "baseline.sql");
        if (File.Exists(baselinePath))
        {
            var baselineUpgrader = DeployChanges.To
                .SqlDatabase(DbConnectionString)
                .WithScript(new SqlScript("baseline.sql", File.ReadAllText(baselinePath)))
                .WithTransactionPerScript()
                .JournalToSqlTable("dbo", "SchemaVersions")
                .LogToConsole()
                .Build();

            var result = baselineUpgrader.PerformUpgrade();
            if (!result.Successful)
                throw result.Error;
        }

        // Phase 2: db/scripts/
        var scriptsPath = Path.Combine(baseDir, "db", "scripts");
        if (Directory.Exists(scriptsPath))
        {
            var scriptsUpgrader = DeployChanges.To
                .SqlDatabase(DbConnectionString)
                .WithScriptsFromFileSystem(scriptsPath, new DbUp.ScriptProviders.FileSystemScriptOptions
                {
                    IncludeSubDirectories = false,
                    Extensions = [".sql"]
                })
                .WithTransactionPerScript()
                .JournalToSqlTable("dbo", "SchemaVersions")
                .LogToConsole()
                .Build();

            var result = scriptsUpgrader.PerformUpgrade();
            if (!result.Successful)
                throw result.Error;
        }
    }

    private static string FindSolutionRoot()
    {
        var dir = Directory.GetCurrentDirectory();
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir, "SkyLabIdP.sln")))
                return dir;
            dir = Directory.GetParent(dir)?.FullName;
        }
        throw new InvalidOperationException("Cannot find solution root directory (SkyLabIdP.sln).");
    }
}
