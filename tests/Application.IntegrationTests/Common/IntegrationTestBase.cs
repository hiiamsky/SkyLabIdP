using System.Net.Http.Json;
using System.Text.Json;
using Application.IntegrationTests.Fixtures;

namespace Application.IntegrationTests.Common;

[Collection("Integration")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly IntegrationTestFixture Fixture;
    protected HttpClient Client { get; private set; } = null!;

    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    protected IntegrationTestBase(IntegrationTestFixture fixture)
    {
        Fixture = fixture;
    }

    public virtual async Task InitializeAsync()
    {
        await Fixture.ResetDatabaseAsync();
        Client = Fixture.Factory.CreateClient();
    }

    public virtual Task DisposeAsync()
    {
        Client?.Dispose();
        return Task.CompletedTask;
    }

    protected void SetTenantHeaders(string tenantId, bool includeApiKey = true, string? authToken = null)
    {
        Client.DefaultRequestHeaders.Clear();
        Client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId);
        Client.DefaultRequestHeaders.Add("User-Agent", "SkyLabIdP-IntegrationTests/1.0");

        if (includeApiKey)
        {
            Client.DefaultRequestHeaders.Add("X-API-key", Fixture.ApiKey);
        }

        if (!string.IsNullOrEmpty(authToken))
        {
            Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        }
    }

    protected async Task<HttpResponseMessage> GetAsync(string endpoint, string tenantId, string? authToken = null)
    {
        SetTenantHeaders(tenantId, authToken: authToken);
        return await Client.GetAsync($"{ApiEndpoints.ApiPrefix}{endpoint}");
    }

    protected async Task<HttpResponseMessage> PostAsync<T>(string endpoint, string tenantId, T data, string? authToken = null)
    {
        SetTenantHeaders(tenantId, authToken: authToken);
        return await Client.PostAsJsonAsync($"{ApiEndpoints.ApiPrefix}{endpoint}", data, JsonOptions);
    }

    protected async Task<TResponse?> ParseResponseAsync<TResponse>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(content))
            return default;

        return JsonSerializer.Deserialize<TResponse>(content, JsonOptions);
    }

    protected static void AssertSuccess(HttpResponseMessage response)
    {
        Assert.True(response.IsSuccessStatusCode,
            $"API request failed with status {(int)response.StatusCode}: {response.ReasonPhrase}");
    }

    protected static string GenerateRandomString(int length = 8)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    protected static string GenerateTestEmail(string prefix = "test")
    {
        return $"{prefix}_{GenerateRandomString()}@example.com";
    }

    protected static async Task WaitAsync(int milliseconds)
    {
        await Task.Delay(milliseconds);
    }
}
