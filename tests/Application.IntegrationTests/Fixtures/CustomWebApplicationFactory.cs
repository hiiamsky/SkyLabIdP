using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Application.IntegrationTests.Fixtures;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public required string DbConnectionString { get; init; }
    public required string RedisHost { get; init; }
    public required int RedisPort { get; init; }
    public required string ApiKey { get; init; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // 覆寫環境變數，指向 Testcontainers
        Environment.SetEnvironmentVariable("DATABASE_CONNECTION_STRING", DbConnectionString);
        Environment.SetEnvironmentVariable("REDIS__HOST", RedisHost);
        Environment.SetEnvironmentVariable("REDIS__PORT", RedisPort.ToString());
        Environment.SetEnvironmentVariable("REDIS__PASSWORD", "");
        Environment.SetEnvironmentVariable("SKYLABIDP_APIKEY", ApiKey);
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "IntegrationTestSecretKeyThatIsAtLeast32Characters!");

        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = DbConnectionString,
                ["REDIS__HOST"] = RedisHost,
                ["REDIS__PORT"] = RedisPort.ToString(),
                ["REDIS__PASSWORD"] = "",
                ["ApiKey"] = ApiKey
            });
        });
    }
}
