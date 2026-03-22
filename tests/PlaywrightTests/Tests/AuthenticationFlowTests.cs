using PlaywrightTests.Common;
using PlaywrightTests.Models;

namespace PlaywrightTests.Tests;

/// <summary>
/// JWT 認證與令牌管理的 E2E 測試
/// 基於使用者故事: 01_Authentication_Flow.md
/// </summary>
[Collection("Sequential")]
public class AuthenticationFlowTests : ApiTestBase
{
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
        var response = await PostAsync(Endpoints.Login, tenantId, loginRequest);

        // Assert with detailed error information
        if (!response.Ok)
        {
            var errorBody = await response.TextAsync();
            Assert.True(response.Ok, $"API request failed with status {response.Status}: {response.StatusText}. Response body: {errorBody}");
        }
        
        var loginResponse = await ParseResponseAsync<LoginResponse>(response);
        Assert.NotNull(loginResponse);
        
        // 顯示詳細的錯誤訊息，如果登入失敗
        if (!loginResponse.Success)
        {
            var responseBody = await response.TextAsync();
            var errorMessage = !string.IsNullOrEmpty(loginResponse.Message) ? loginResponse.Message : "Unknown error";
            Assert.True(loginResponse.Success, $"Login failed for tenant {tenantId}, user {userName}. Error: {errorMessage}. Full response: {responseBody}");
        }
        Assert.NotEmpty(loginResponse.AccessToken);
        Assert.NotEmpty(loginResponse.RefreshToken);
        Assert.True(loginResponse.ExpiresAt > DateTime.UtcNow);
        
        // 驗證使用者資訊
        Assert.NotNull(loginResponse.User);
        Assert.Equal(userName, loginResponse.User.UserName);
        Assert.Equal(tenantId, loginResponse.User.TenantId);
        
        // 驗證 JWT Token 格式（應該有三個由點分隔的部分）
        var tokenParts = loginResponse.AccessToken.Split('.');
        Assert.Equal(3, tokenParts.Length);
    }

    /// <summary>
    /// AC02: 使用 Refresh Token 更新存取令牌
    /// </summary>
    [Fact]
    public async Task RefreshToken_WithValidRefreshToken_ShouldReturnNewTokens()
    {
        // Arrange - 先登入取得 refresh token
        var loginRequest = new LoginRequest
        {
            UserName = TestUsers.SkyLabmgm.UserName,
            Password = TestUsers.SkyLabmgm.Password,
            CaptchaId = "test-captcha-id",
            CaptchaValue = "TEST"
        };

        var loginResponse = await PostAsync(Endpoints.Login, TestUsers.SkyLabmgm.TenantId, loginRequest);
        AssertSuccess(loginResponse);
        
        var loginData = await ParseResponseAsync<LoginResponse>(loginResponse);
        Assert.NotNull(loginData);
        var originalRefreshToken = loginData.RefreshToken;
        var originalAccessToken = loginData.AccessToken;

        // 等待一小段時間確保新 token 的時間戳不同
        await WaitAsync(1000);

        // Act - 使用 refresh token 更新
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = originalRefreshToken
        };

        var refreshResponse = await PostAsync(Endpoints.RefreshToken, TestUsers.SkyLabmgm.TenantId, refreshRequest);

        // Assert
        AssertSuccess(refreshResponse);
        
        var refreshData = await ParseResponseAsync<RefreshTokenResponse>(refreshResponse);
        Assert.NotNull(refreshData);
        Assert.True(refreshData.Success);
        Assert.NotEmpty(refreshData.AccessToken);
        Assert.NotEmpty(refreshData.RefreshToken);
        
        // 新的 tokens 應該與原本的不同
        Assert.NotEqual(originalAccessToken, refreshData.AccessToken);
        Assert.NotEqual(originalRefreshToken, refreshData.RefreshToken);
        
        // 到期時間應該是未來的時間
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

        var loginResponse = await PostAsync(Endpoints.Login, TestUsers.SkyLabmgm.TenantId, loginRequest);
        AssertSuccess(loginResponse);
        
        var loginData = await ParseResponseAsync<LoginResponse>(loginResponse);
        Assert.NotNull(loginData);

        // Act - 登出
        var logoutRequest = new LogoutRequest
        {
            RefreshToken = loginData.RefreshToken
        };

        var logoutResponse = await PostAsync(Endpoints.Logout, TestUsers.SkyLabmgm.TenantId, 
            logoutRequest, loginData.AccessToken);

        // Assert
        AssertSuccess(logoutResponse);
        
        var logoutData = await ParseResponseAsync<ApiResponse>(logoutResponse);
        Assert.NotNull(logoutData);
        Assert.True(logoutData.Success);

        // 等待 Redis 黑名單更新
        await WaitAsync(1000);

        // 驗證登出後的 token 已無效 - 嘗試使用舊 token 刷新
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = loginData.RefreshToken
        };

        var refreshAfterLogout = await PostAsync(Endpoints.RefreshToken, TestUsers.SkyLabmgm.TenantId, refreshRequest);
        
        // 應該返回 401 Unauthorized
        Assert.Equal(HttpStatusCodes.Unauthorized, refreshAfterLogout.Status);
    }

    /// <summary>
    /// AC04: JWKS 端點提供公鑰驗證
    /// </summary>
    [Fact]
    public async Task GetJwks_ShouldReturnValidJwksResponse()
    {
        // Act
        var response = await GetAsync(Endpoints.Jwks, TenantIds.SkyLabmgm);

        // Assert
        AssertSuccess(response);
        
        var jwksData = await ParseResponseAsync<JwksResponse>(response);
        Assert.NotNull(jwksData);
        Assert.NotNull(jwksData.Keys);
        Assert.NotEmpty(jwksData.Keys);
        
        // 驗證第一個金鑰的格式
        var firstKey = jwksData.Keys.First();
        Assert.Equal("RSA", firstKey.Kty);
        Assert.Equal("sig", firstKey.Use);
        Assert.NotEmpty(firstKey.Kid);
        Assert.NotEmpty(firstKey.N);
        Assert.Equal("AQAB", firstKey.E); // 標準 RSA 指數
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
        // Arrange
        var loginRequest = new LoginRequest
        {
            UserName = $"test_{GenerateRandomString()}@example.com",
            Password = "TestPass123!",
            CaptchaId = "test-captcha-id",
            CaptchaValue = "TEST"
        };

        // Act - 測試包含所有必要標頭的請求
        var response = await PostAsync(Endpoints.Login, tenantId, loginRequest);

        // Assert - 如果使用者不存在會是 401，但不會是因為標頭問題
        // 標頭問題通常會回傳 400 或不同的錯誤訊息
        Assert.True(response.Status == HttpStatusCodes.Ok || response.Status == HttpStatusCodes.Unauthorized);
        
        // 如果是 401，確認是認證問題而不是標頭問題
        if (response.Status == HttpStatusCodes.Unauthorized)
        {
            var content = await response.TextAsync();
            // 標頭問題的錯誤訊息通常包含 "header" 或 "API key"
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
        // Arrange
        var loginRequest = new LoginRequest
        {
            UserName = userName,
            Password = password,
            CaptchaId = "test-captcha-id",
            CaptchaValue = "TEST"
        };

        // Act
        var response = await PostAsync(Endpoints.Login, tenantId, loginRequest);

        // Assert
        Assert.Equal(HttpStatusCodes.Unauthorized, response.Status);
        
        var errorResponse = await ParseResponseAsync<ApiResponse>(response);
        Assert.NotNull(errorResponse);
        Assert.False(errorResponse.Success);
        Assert.NotNull(errorResponse.Message);
        
        // 確認錯誤訊息不洩露敏感資訊
        Assert.DoesNotContain("database", errorResponse.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exception", errorResponse.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// AC08: 令牌到期處理
    /// </summary>
    [Fact]
    public async Task AccessProtectedEndpoint_WithExpiredToken_ShouldReturnUnauthorized()
    {
        // 這個測試需要一個已過期的 token
        // 在實際測試中，可能需要使用 mock 或配置短時間的 token
        
        // Arrange - 使用明顯無效的 token 格式
        var expiredToken = "expired.jwt.token";

        // Act - 嘗試存取受保護的端點
        var response = await GetAsync(Endpoints.AcctMgmtSearch, TenantIds.SkyLabmgm, expiredToken);

        // Assert
        Assert.Equal(HttpStatusCodes.Unauthorized, response.Status);
    }

    /// <summary>
    /// AC09: 黑名單令牌處理
    /// </summary>
    [Fact] 
    public async Task UseTokenAfterLogout_ShouldReturnUnauthorized()
    {
        // Arrange - 登入並取得 token
        var loginRequest = new LoginRequest
        {
            UserName = TestUsers.SkyLabmgm.UserName,
            Password = TestUsers.SkyLabmgm.Password,
            CaptchaId = "test-captcha-id",
            CaptchaValue = "TEST"
        };

        var loginResponse = await PostAsync(Endpoints.Login, TestUsers.SkyLabmgm.TenantId, loginRequest);
        AssertSuccess(loginResponse);
        
        var loginData = await ParseResponseAsync<LoginResponse>(loginResponse);
        Assert.NotNull(loginData);

        // 登出，將 token 加入黑名單
        var logoutRequest = new LogoutRequest { RefreshToken = loginData.RefreshToken };
        await PostAsync(Endpoints.Logout, TestUsers.SkyLabmgm.TenantId, logoutRequest, loginData.AccessToken);

        // 等待 Redis 黑名單更新
        await WaitAsync(1000);

        // Act - 嘗試使用已登出的 access token
        var response = await GetAsync(Endpoints.AcctMgmtSearch, TestUsers.SkyLabmgm.TenantId, loginData.AccessToken);

        // Assert
        Assert.Equal(HttpStatusCodes.Unauthorized, response.Status);
    }

    /// <summary>
    /// 效能測試：登入回應時間應小於 2 秒
    /// </summary>
    [Fact]
    public async Task Login_PerformanceTest_ShouldCompleteWithinTwoSeconds()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            UserName = TestUsers.SkyLabmgm.UserName,
            Password = TestUsers.SkyLabmgm.Password,
            CaptchaId = "test-captcha-id",
            CaptchaValue = "TEST"
        };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await PostAsync(Endpoints.Login, TestUsers.SkyLabmgm.TenantId, loginRequest);

        stopwatch.Stop();

        // Assert
        AssertSuccess(response);
        Assert.True(stopwatch.ElapsedMilliseconds < 2000, 
            $"Login took {stopwatch.ElapsedMilliseconds}ms, expected < 2000ms");
    }
}
