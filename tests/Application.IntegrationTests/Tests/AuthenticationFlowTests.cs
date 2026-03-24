using Application.IntegrationTests.Common;
using Application.IntegrationTests.Fixtures;
using Application.IntegrationTests.Models;

namespace Application.IntegrationTests.Tests;

[Collection("Integration")]
public class AuthenticationFlowTests : IntegrationTestBase
{
    public AuthenticationFlowTests(IntegrationTestFixture fixture) : base(fixture) { }

    /// <summary>
    /// AC01: 成功登入並取得 JWT 令牌
    /// </summary>
    [Theory]
    [InlineData(TestUsers.SkyLabmgm.TenantId, TestUsers.SkyLabmgm.UserName, TestUsers.SkyLabmgm.Password)]
    [InlineData(TestUsers.Committee.TenantId, TestUsers.Committee.UserName, TestUsers.Committee.Password)]
    [InlineData(TestUsers.Developer.TenantId, TestUsers.Developer.UserName, TestUsers.Developer.Password)]
    public async Task Login_WithValidCredentials_ShouldReturnJwtTokens(
        string tenantId, string userName, string password)
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            UserName = userName,
            Password = password,
            CaptchaId = "test-captcha-id",
            CaptchaValue = "TEST"
        };

        // Act
        var response = await PostAsync(ApiEndpoints.Login, tenantId, loginRequest);

        // Assert
        AssertSuccess(response);

        var loginResponse = await ParseResponseAsync<LoginResponse>(response);
        Assert.NotNull(loginResponse);
        Assert.True(loginResponse.Success, $"Login failed for tenant {tenantId}, user {userName}. Error: {loginResponse.Message}");
        Assert.NotEmpty(loginResponse.AccessToken);
        Assert.NotEmpty(loginResponse.RefreshToken);
        Assert.True(loginResponse.ExpiresAt > DateTime.UtcNow);

        Assert.NotNull(loginResponse.User);
        Assert.Equal(userName, loginResponse.User.UserName);
        Assert.Equal(tenantId, loginResponse.User.TenantId);

        // 驗證 JWT Token 格式
        var tokenParts = loginResponse.AccessToken.Split('.');
        Assert.Equal(3, tokenParts.Length);
    }

    /// <summary>
    /// AC02: 使用 Refresh Token 更新存取令牌
    /// </summary>
    [Fact]
    public async Task RefreshToken_WithValidRefreshToken_ShouldReturnNewTokens()
    {
        // Arrange - 先登入
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

        await WaitAsync(1000);

        // Act
        var refreshRequest = new RefreshTokenRequest { RefreshToken = loginData.RefreshToken };
        var refreshResponse = await PostAsync(ApiEndpoints.RefreshToken, TestUsers.SkyLabmgm.TenantId, refreshRequest);

        // Assert
        AssertSuccess(refreshResponse);

        var refreshData = await ParseResponseAsync<RefreshTokenResponse>(refreshResponse);
        Assert.NotNull(refreshData);
        Assert.True(refreshData.Success);
        Assert.NotEmpty(refreshData.AccessToken);
        Assert.NotEmpty(refreshData.RefreshToken);
        Assert.NotEqual(loginData.AccessToken, refreshData.AccessToken);
        Assert.NotEqual(loginData.RefreshToken, refreshData.RefreshToken);
        Assert.True(refreshData.ExpiresAt > DateTime.UtcNow);
    }

    /// <summary>
    /// AC03: 安全登出並撤銷令牌
    /// </summary>
    [Fact]
    public async Task Logout_WithValidToken_ShouldSucceed()
    {
        // Arrange - 先登入
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

        // Act - 登出
        var logoutRequest = new LogoutRequest { RefreshToken = loginData.RefreshToken };
        var logoutResponse = await PostAsync(ApiEndpoints.Logout, TestUsers.SkyLabmgm.TenantId,
            logoutRequest, loginData.AccessToken);

        // Assert
        AssertSuccess(logoutResponse);
        var logoutData = await ParseResponseAsync<ApiResponse>(logoutResponse);
        Assert.NotNull(logoutData);
        Assert.True(logoutData.Success);

        await WaitAsync(1000);

        // 驗證登出後的 token 已無效
        var refreshRequest = new RefreshTokenRequest { RefreshToken = loginData.RefreshToken };
        var refreshAfterLogout = await PostAsync(ApiEndpoints.RefreshToken, TestUsers.SkyLabmgm.TenantId, refreshRequest);
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, refreshAfterLogout.StatusCode);
    }

    /// <summary>
    /// AC04: JWKS 端點提供公鑰驗證
    /// </summary>
    [Fact]
    public async Task GetJwks_ShouldReturnValidJwksResponse()
    {
        // Act
        var response = await GetAsync(ApiEndpoints.Jwks, TenantIds.SkyLabmgm);

        // Assert
        AssertSuccess(response);

        var jwksData = await ParseResponseAsync<JwksResponse>(response);
        Assert.NotNull(jwksData);
        Assert.NotNull(jwksData.Keys);
        Assert.NotEmpty(jwksData.Keys);

        var firstKey = jwksData.Keys.First();
        Assert.Equal("RSA", firstKey.Kty);
        Assert.Equal("sig", firstKey.Use);
        Assert.NotEmpty(firstKey.Kid);
        Assert.NotEmpty(firstKey.N);
        Assert.Equal("AQAB", firstKey.E);
    }

    /// <summary>
    /// AC05: 多重認證標頭驗證
    /// </summary>
    [Theory]
    [InlineData(TenantIds.SkyLabmgm)]
    [InlineData(TenantIds.SkyLabcommittee)]
    [InlineData(TenantIds.SkyLabdevelop)]
    public async Task Login_WithRequiredHeaders_ShouldSucceed(string tenantId)
    {
        var loginRequest = new LoginRequest
        {
            UserName = $"test_{GenerateRandomString()}@example.com",
            Password = "TestPass123!",
            CaptchaId = "test-captcha-id",
            CaptchaValue = "TEST"
        };

        var response = await PostAsync(ApiEndpoints.Login, tenantId, loginRequest);

        Assert.True((int)response.StatusCode == 200 || (int)response.StatusCode == 401);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var content = await response.Content.ReadAsStringAsync();
            Assert.DoesNotContain("header", content, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("API key", content, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// AC07: 無效憑證處理
    /// </summary>
    [Theory]
    [InlineData("nonexistent@example.com", "ValidPassword123!", TenantIds.SkyLabmgm)]
    [InlineData(TestUsers.SkyLabmgm.UserName, "WrongPassword", TenantIds.SkyLabmgm)]
    [InlineData("", TestUsers.SkyLabmgm.Password, TenantIds.SkyLabmgm)]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized(
        string userName, string password, string tenantId)
    {
        var loginRequest = new LoginRequest
        {
            UserName = userName,
            Password = password,
            CaptchaId = "test-captcha-id",
            CaptchaValue = "TEST"
        };

        var response = await PostAsync(ApiEndpoints.Login, tenantId, loginRequest);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);

        var errorResponse = await ParseResponseAsync<ApiResponse>(response);
        Assert.NotNull(errorResponse);
        Assert.False(errorResponse.Success);
        Assert.NotNull(errorResponse.Message);

        Assert.DoesNotContain("database", errorResponse.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exception", errorResponse.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// AC08: 令牌到期處理
    /// </summary>
    [Fact]
    public async Task AccessProtectedEndpoint_WithExpiredToken_ShouldReturnUnauthorized()
    {
        var expiredToken = "expired.jwt.token";
        var response = await GetAsync(ApiEndpoints.AcctMgmtSearch, TenantIds.SkyLabmgm, expiredToken);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// AC09: 黑名單令牌處理
    /// </summary>
    [Fact]
    public async Task UseTokenAfterLogout_ShouldReturnUnauthorized()
    {
        // Arrange - 登入
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

        // 登出
        var logoutRequest = new LogoutRequest { RefreshToken = loginData.RefreshToken };
        await PostAsync(ApiEndpoints.Logout, TestUsers.SkyLabmgm.TenantId, logoutRequest, loginData.AccessToken);

        await WaitAsync(1000);

        // Act - 使用已登出的 token
        var response = await GetAsync(ApiEndpoints.AcctMgmtSearch, TestUsers.SkyLabmgm.TenantId, loginData.AccessToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// 效能測試：登入回應時間應小於 2 秒
    /// </summary>
    [Fact]
    public async Task Login_PerformanceTest_ShouldCompleteWithinTwoSeconds()
    {
        var loginRequest = new LoginRequest
        {
            UserName = TestUsers.SkyLabmgm.UserName,
            Password = TestUsers.SkyLabmgm.Password,
            CaptchaId = "test-captcha-id",
            CaptchaValue = "TEST"
        };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await PostAsync(ApiEndpoints.Login, TestUsers.SkyLabmgm.TenantId, loginRequest);
        stopwatch.Stop();

        AssertSuccess(response);
        Assert.True(stopwatch.ElapsedMilliseconds < 2000,
            $"Login took {stopwatch.ElapsedMilliseconds}ms, expected < 2000ms");
    }
}
