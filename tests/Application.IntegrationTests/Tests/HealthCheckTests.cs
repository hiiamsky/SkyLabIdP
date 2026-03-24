using System.Text.Json;
using Application.IntegrationTests.Common;
using Application.IntegrationTests.Fixtures;

namespace Application.IntegrationTests.Tests;

[Collection("Integration")]
public class HealthCheckTests : IntegrationTestBase
{
    public HealthCheckTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task HealthLive_ShouldReturn200()
    {
        // Act
        var response = await Client.GetAsync("/health/live");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthReady_ShouldReturn200_WhenDependenciesHealthy()
    {
        // Act
        var response = await Client.GetAsync("/health/ready");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task Health_ShouldReturn200_WithJsonResponse()
    {
        // Act
        var response = await Client.GetAsync("/health");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);

        // 驗證回應是有效的 JSON
        var doc = JsonDocument.Parse(content);
        Assert.NotNull(doc);
    }

    [Fact]
    public async Task Health_ShouldContainSqlServerAndRedisEntries()
    {
        // Act
        var response = await Client.GetAsync("/health");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        // 驗證包含 entries
        Assert.True(root.TryGetProperty("entries", out var entries),
            $"Response should contain 'entries'. Actual response: {content}");

        Assert.True(entries.TryGetProperty("sqlserver", out _),
            $"Entries should contain 'sqlserver'. Actual entries: {entries}");
        Assert.True(entries.TryGetProperty("redis", out _),
            $"Entries should contain 'redis'. Actual entries: {entries}");
    }
}
