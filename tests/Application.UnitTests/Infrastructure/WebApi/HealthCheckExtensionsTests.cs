using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SkyLabIdP.WebApi.Extensions;

namespace Application.UnitTests.Infrastructure.WebApi;

public class HealthCheckExtensionsTests : IDisposable
{
    private readonly string? _originalDbConn;
    private readonly string? _originalRedisHost;
    private readonly string? _originalRedisPort;
    private readonly string? _originalRedisPassword;

    public HealthCheckExtensionsTests()
    {
        _originalDbConn = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
        _originalRedisHost = Environment.GetEnvironmentVariable("REDIS__HOST");
        _originalRedisPort = Environment.GetEnvironmentVariable("REDIS__PORT");
        _originalRedisPassword = Environment.GetEnvironmentVariable("REDIS__PASSWORD");

        Environment.SetEnvironmentVariable("DATABASE_CONNECTION_STRING", "Server=localhost;Database=Test;Trusted_Connection=true;");
        Environment.SetEnvironmentVariable("REDIS__HOST", "localhost");
        Environment.SetEnvironmentVariable("REDIS__PORT", "6379");
        Environment.SetEnvironmentVariable("REDIS__PASSWORD", "testpass");
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("DATABASE_CONNECTION_STRING", _originalDbConn);
        Environment.SetEnvironmentVariable("REDIS__HOST", _originalRedisHost);
        Environment.SetEnvironmentVariable("REDIS__PORT", _originalRedisPort);
        Environment.SetEnvironmentVariable("REDIS__PASSWORD", _originalRedisPassword);
    }

    [Fact]
    public void AddHealthCheckServices_RegistersHealthCheckService()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddHealthCheckServices();

        var provider = services.BuildServiceProvider();
        var healthCheckService = provider.GetService<HealthCheckService>();

        healthCheckService.Should().NotBeNull();
    }

    [Fact]
    public void AddHealthCheckServices_ReturnsServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddHealthCheckServices();

        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddHealthCheckServices_RegistersSqlServerCheck()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHealthCheckServices();

        var provider = services.BuildServiceProvider();

        var options = provider.GetService<Microsoft.Extensions.Options.IOptions<HealthCheckServiceOptions>>();
        options.Should().NotBeNull();
        options!.Value.Registrations.Should().Contain(r => r.Name == "sqlserver");
    }

    [Fact]
    public void AddHealthCheckServices_RegistersRedisCheck()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHealthCheckServices();

        var provider = services.BuildServiceProvider();

        var options = provider.GetService<Microsoft.Extensions.Options.IOptions<HealthCheckServiceOptions>>();
        options.Should().NotBeNull();
        options!.Value.Registrations.Should().Contain(r => r.Name == "redis");
    }

    [Fact]
    public void AddHealthCheckServices_SqlServerCheckHasReadyTag()
    {
        var services = new ServiceCollection();
        services.AddHealthCheckServices();

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<HealthCheckServiceOptions>>();
        var sqlRegistration = options.Value.Registrations.First(r => r.Name == "sqlserver");

        sqlRegistration.Tags.Should().Contain("ready");
    }

    [Fact]
    public void AddHealthCheckServices_RedisCheckHasReadyTag()
    {
        var services = new ServiceCollection();
        services.AddHealthCheckServices();

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<HealthCheckServiceOptions>>();
        var redisRegistration = options.Value.Registrations.First(r => r.Name == "redis");

        redisRegistration.Tags.Should().Contain("ready");
    }

    [Fact]
    public void AddHealthCheckServices_ThrowsWithoutDatabaseConnectionString()
    {
        Environment.SetEnvironmentVariable("DATABASE_CONNECTION_STRING", null);
        var services = new ServiceCollection();

        var act = () => services.AddHealthCheckServices();

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*DATABASE_CONNECTION_STRING*");
    }
}
