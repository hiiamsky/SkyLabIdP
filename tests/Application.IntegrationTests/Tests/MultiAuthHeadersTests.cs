using System.Net;
using Application.IntegrationTests.Common;
using Application.IntegrationTests.Fixtures;
using Application.IntegrationTests.Models;

namespace Application.IntegrationTests.Tests;

[Collection("Integration")]
public class MultiAuthHeadersTests : IntegrationTestBase
{
    public MultiAuthHeadersTests(IntegrationTestFixture fixture) : base(fixture) { }

    /// <summary>
    /// AC01: 未認證請求的基本驗證 — 公開端點只需 X-Tenant-Id 和 X-API-key
    /// </summary>
    [Theory]
    [InlineData(TenantIds.SkyLabmgm)]
    [InlineData(TenantIds.SkyLabcommittee)]
    [InlineData(TenantIds.SkyLabdevelop)]
    [InlineData(TenantIds.SkyLabcaedp)]
    public async Task PublicEndpoint_WithBasicHeaders_ShouldSucceed(string tenantId)
    {
        // Act
        var response = await GetAsync(ApiEndpoints.Jwks, tenantId);

        // Assert
        AssertSuccess(response);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("keys", content);
        Assert.Contains("kty", content);
        Assert.Contains("RSA", content);
    }

    /// <summary>
    /// AC02: 完整三重認證驗證 — 受保護端點需要 JWT
    /// </summary>
    [Fact]
    public async Task ProtectedEndpoint_WithCompleteHeaders_ShouldSucceed()
    {
        // Arrange - 先登入取得 JWT token
        var loginRequest = new LoginRequest
        {
            UserName = TestUsers.SkyLabmgm.UserName,
            Password = TestUsers.SkyLabmgm.Password,
            CaptchaId = "test-captcha-id",
            CaptchaValue = "TEST"
        };

        var loginResponse = await PostAsync(ApiEndpoints.Login, TestUsers.SkyLabmgm.TenantId, loginRequest);
        AssertSuccess(loginResponse);
        var loginData = await ParseResponseAsync<LoginResponse>(loginResponse);
        Assert.NotNull(loginData);

        // Act - 使用完整三重認證標頭存取受保護端點
        var refreshRequest = new RefreshTokenRequest { RefreshToken = loginData.RefreshToken };
        var response = await PostAsync(ApiEndpoints.RefreshToken, TestUsers.SkyLabmgm.TenantId,
            refreshRequest, loginData.AccessToken);

        // Assert
        AssertSuccess(response);
        var refreshData = await ParseResponseAsync<RefreshTokenResponse>(response);
        Assert.NotNull(refreshData);
        Assert.True(refreshData.Success);
        Assert.NotEmpty(refreshData.AccessToken);
    }

    /// <summary>
    /// AC03: 租戶隔離驗證 — X-Tenant-Id 與 JWT tenant 不匹配
    /// </summary>
    [Fact]
    public async Task ProtectedEndpoint_WithMismatchedTenantId_ShouldReturnForbidden()
    {
        // Arrange - 使用 SkyLabmgm 租戶登入
        var loginRequest = new LoginRequest
        {
            UserName = TestUsers.SkyLabmgm.UserName,
            Password = TestUsers.SkyLabmgm.Password,
            CaptchaId = "test-captcha-id",
            CaptchaValue = "TEST"
        };

        var loginResponse = await PostAsync(ApiEndpoints.Login, TestUsers.SkyLabmgm.TenantId, loginRequest);
        AssertSuccess(loginResponse);
        var loginData = await ParseResponseAsync<LoginResponse>(loginResponse);
        Assert.NotNull(loginData);

        // Act - 使用不同租戶的標頭
        var refreshRequest = new RefreshTokenRequest { RefreshToken = loginData.RefreshToken };
        var response = await PostAsync(ApiEndpoints.RefreshToken, TenantIds.SkyLabcommittee,
            refreshRequest, loginData.AccessToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// AC04: 錯誤的 API Key 應被拒絕
    /// </summary>
    [Fact]
    public async Task Request_WithInvalidApiKey_ShouldReturnUnauthorized()
    {
        // Act - 使用手動設定的 header 以帶入錯誤 API key
        Client.DefaultRequestHeaders.Clear();
        Client.DefaultRequestHeaders.Add("X-Tenant-Id", TenantIds.SkyLabmgm);
        Client.DefaultRequestHeaders.Add("X-API-key", "invalid-api-key");
        var response = await Client.GetAsync($"{ApiEndpoints.ApiPrefix}{ApiEndpoints.Jwks}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// AC04: 缺少 API Key 應被拒絕
    /// </summary>
    [Fact]
    public async Task Request_WithoutApiKey_ShouldReturnBadRequest()
    {
        // Act
        Client.DefaultRequestHeaders.Clear();
        Client.DefaultRequestHeaders.Add("X-Tenant-Id", TenantIds.SkyLabmgm);
        var response = await Client.GetAsync($"{ApiEndpoints.ApiPrefix}{ApiEndpoints.Jwks}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// AC07: 缺少 X-Tenant-Id 應返回 BadRequest
    /// </summary>
    [Fact]
    public async Task Request_WithoutTenantId_ShouldReturnBadRequest()
    {
        // Act
        Client.DefaultRequestHeaders.Clear();
        Client.DefaultRequestHeaders.Add("X-API-key", Fixture.ApiKey);
        var response = await Client.GetAsync($"{ApiEndpoints.ApiPrefix}{ApiEndpoints.Jwks}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// AC07: 受保護端點缺少 Authorization 標頭
    /// </summary>
    [Fact]
    public async Task ProtectedEndpoint_WithoutAuthorizationHeader_ShouldReturnUnauthorized()
    {
        // Act - 不提供 authToken
        var refreshRequest = new RefreshTokenRequest { RefreshToken = "dummy-refresh-token" };
        var response = await PostAsync(ApiEndpoints.RefreshToken, TenantIds.SkyLabmgm, refreshRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// AC08: 無效租戶 ID 應被拒絕
    /// </summary>
    [Fact]
    public async Task Request_WithInvalidTenantId_ShouldReturnBadRequest()
    {
        // Act
        var response = await GetAsync(ApiEndpoints.Jwks, "InvalidTenantId");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// AC08: 過期的 JWT 應返回 Unauthorized
    /// </summary>
    [Fact]
    public async Task ProtectedEndpoint_WithExpiredJwt_ShouldReturnUnauthorized()
    {
        var expiredJwt = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ0ZXN0IiwiZXhwIjoxfQ.invalid";

        var refreshRequest = new RefreshTokenRequest { RefreshToken = "dummy-refresh-token" };
        var response = await PostAsync(ApiEndpoints.RefreshToken, TenantIds.SkyLabmgm,
            refreshRequest, expiredJwt);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// 所有有效租戶 ID 都能存取公開端點
    /// </summary>
    [Theory]
    [MemberData(nameof(GetAllTenantIds))]
    public async Task PublicEndpoint_WithAllValidTenantIds_ShouldSucceed(string tenantId)
    {
        var response = await GetAsync(ApiEndpoints.Jwks, tenantId);
        AssertSuccess(response);
    }

    public static IEnumerable<object[]> GetAllTenantIds()
    {
        return TenantIds.All.Select(id => new object[] { id });
    }

    /// <summary>
    /// 多重認證錯誤的優先級 — 缺少租戶 ID 優先
    /// </summary>
    [Fact]
    public async Task Request_WithMultipleAuthErrors_ShouldReturnMostRelevantError()
    {
        // Act - 缺少租戶 ID 且 API Key 錯誤
        Client.DefaultRequestHeaders.Clear();
        Client.DefaultRequestHeaders.Add("X-API-key", "invalid-key");
        var response = await Client.GetAsync($"{ApiEndpoints.ApiPrefix}{ApiEndpoints.Jwks}");

        // Assert - 應先檢查租戶 ID
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// 效能測試：認證驗證時間
    /// </summary>
    [Fact]
    public async Task AuthenticationValidation_PerformanceTest_ShouldCompleteQuickly()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await GetAsync(ApiEndpoints.Jwks, TenantIds.SkyLabmgm);
        stopwatch.Stop();

        AssertSuccess(response);
        Assert.True(stopwatch.ElapsedMilliseconds < 500,
            $"Authentication validation took {stopwatch.ElapsedMilliseconds}ms, expected < 500ms");
    }

    /// <summary>
    /// 標頭大小寫不敏感
    /// </summary>
    [Fact]
    public async Task Request_WithDifferentHeaderCasing_ShouldStillWork()
    {
        Client.DefaultRequestHeaders.Clear();
        Client.DefaultRequestHeaders.Add("x-tenant-id", TenantIds.SkyLabmgm);
        Client.DefaultRequestHeaders.Add("X-API-KEY", Fixture.ApiKey);
        var response = await Client.GetAsync($"{ApiEndpoints.ApiPrefix}{ApiEndpoints.Jwks}");

        AssertSuccess(response);
    }

    /// <summary>
    /// 並發請求的認證處理
    /// </summary>
    [Fact]
    public async Task ConcurrentRequests_WithValidHeaders_ShouldAllSucceed()
    {
        const int concurrentRequests = 5;
        var tasks = new List<Task<HttpResponseMessage>>();

        for (int i = 0; i < concurrentRequests; i++)
        {
            tasks.Add(GetAsync(ApiEndpoints.Jwks, TenantIds.SkyLabmgm));
        }

        var responses = await Task.WhenAll(tasks);

        foreach (var response in responses)
        {
            AssertSuccess(response);
        }
    }
}
