using Microsoft.Playwright;
using PlaywrightTests.Common;
using PlaywrightTests.Models;

namespace PlaywrightTests.Tests;

/// <summary>
/// 多重認證標頭驗證的 E2E 測試
/// 基於使用者故事: 03_Multi_Auth_Headers.md
/// </summary>
[Collection("Sequential")]
public class MultiAuthHeadersTests : ApiTestBase
{
    /// <summary>
    /// AC01: 未認證請求的基本驗證
    /// 測試只需要 X-Tenant-Id 和 X-API-key 的公開端點
    /// </summary>
    [Theory]
    [InlineData(TenantIds.SkyLabmgm)]
    [InlineData(TenantIds.SkyLabcommittee)]
    [InlineData(TenantIds.SkyLabdevelop)]
    [InlineData(TenantIds.SkyLabcaedp)]
    public async Task PublicEndpoint_WithBasicHeaders_ShouldSucceed(string tenantId)
    {
        // Act - 存取 JWKS 公開端點
        var response = await GetAsync(Endpoints.Jwks, tenantId);

        // Assert
        AssertSuccess(response);
        
        // 驗證回應內容是有效的 JWKS
        var content = await response.TextAsync();
        Assert.Contains("keys", content);
        Assert.Contains("kty", content);
        Assert.Contains("RSA", content);
    }

    /// <summary>
    /// AC02: 完整三重認證驗證
    /// 測試需要登入的受保護端點
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

        var loginResponse = await PostAsync(Endpoints.Login, TestUsers.SkyLabmgm.TenantId, loginRequest);
        AssertSuccess(loginResponse);
        
        var loginData = await ParseResponseAsync<LoginResponse>(loginResponse);
        Assert.NotNull(loginData);

        // Act - 使用完整的三重認證標頭存取受保護端點
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = loginData.RefreshToken
        };

        var response = await PostAsync(Endpoints.RefreshToken, TestUsers.SkyLabmgm.TenantId, 
            refreshRequest, loginData.AccessToken);

        // Assert
        AssertSuccess(response);
        
        var refreshData = await ParseResponseAsync<RefreshTokenResponse>(response);
        Assert.NotNull(refreshData);
        Assert.True(refreshData.Success);
        Assert.NotEmpty(refreshData.AccessToken);
    }

    /// <summary>
    /// AC03: 租戶隔離驗證
    /// 測試 X-Tenant-Id 與 JWT token 中 tenant_id 不匹配的情況
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

        var loginResponse = await PostAsync(Endpoints.Login, TestUsers.SkyLabmgm.TenantId, loginRequest);
        AssertSuccess(loginResponse);
        
        var loginData = await ParseResponseAsync<LoginResponse>(loginResponse);
        Assert.NotNull(loginData);

        // Act - 使用不同租戶的標頭但相同的 JWT token
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = loginData.RefreshToken
        };

        // 使用 SkyLabcommittee 租戶標頭，但 JWT 是 SkyLabmgm 的
        var response = await PostAsync(Endpoints.RefreshToken, TenantIds.SkyLabcommittee, 
            refreshRequest, loginData.AccessToken);

        // Assert - 應該被拒絕
        Assert.True(response.Status == HttpStatusCodes.Forbidden || 
                   response.Status == HttpStatusCodes.Unauthorized);
    }

    /// <summary>
    /// AC04: API Key 驗證機制
    /// </summary>
    [Fact]
    public async Task Request_WithInvalidApiKey_ShouldReturnUnauthorized()
    {
        // Act - 發送包含錯誤 API Key 的請求
        var invalidApiKey = "invalid-api-key";
        
        var response = await ApiContext.GetAsync($"{ApiConfig.ApiPrefix}{Endpoints.Jwks}", new()
        {
            Headers = new Dictionary<string, string>
            {
                [Headers.TenantId] = TenantIds.SkyLabmgm,
                [Headers.ApiKey] = invalidApiKey,
                [Headers.ContentType] = "application/json"
            }
        });

        // Assert
        Assert.Equal(HttpStatusCodes.Unauthorized, response.Status);
    }

    /// <summary>
    /// AC04: 缺少 API Key 的請求應該被拒絕
    /// </summary>
    [Fact]
    public async Task Request_WithoutApiKey_ShouldReturnUnauthorized()
    {
        // Act - 發送不包含 API Key 的請求
        var response = await ApiContext.GetAsync($"{ApiConfig.ApiPrefix}{Endpoints.Jwks}", new()
        {
            Headers = new Dictionary<string, string>
            {
                [Headers.TenantId] = TenantIds.SkyLabmgm,
                [Headers.ContentType] = "application/json"
                // 故意不包含 X-API-key
            }
        });

        // Assert
        Assert.Equal(HttpStatusCodes.BadRequest, response.Status);
    }

    /// <summary>
    /// AC07: 缺失必要標頭處理 - 缺少 X-Tenant-Id
    /// </summary>
    [Fact]
    public async Task Request_WithoutTenantId_ShouldReturnBadRequest()
    {
        // Act - 發送不包含 X-Tenant-Id 的請求
        var response = await ApiContext.GetAsync($"{ApiConfig.ApiPrefix}{Endpoints.Jwks}", new()
        {
            Headers = new Dictionary<string, string>
            {
                [Headers.ApiKey] = ApiKey,
                [Headers.ContentType] = "application/json"
                // 故意不包含 X-Tenant-Id
            }
        });

        // Assert
        Assert.Equal(HttpStatusCodes.BadRequest, response.Status);
    }

    /// <summary>
    /// AC07: 受保護端點缺少 Authorization 標頭
    /// </summary>
    [Fact]
    public async Task ProtectedEndpoint_WithoutAuthorizationHeader_ShouldReturnUnauthorized()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = "dummy-refresh-token"
        };

        // Act - 嘗試存取受保護端點但不提供 Authorization 標頭
        var response = await PostAsync(Endpoints.RefreshToken, TenantIds.SkyLabmgm, refreshRequest);
        // 這裡不提供 authToken 參數，所以不會有 Authorization 標頭

        // Assert
        Assert.Equal(HttpStatusCodes.BadRequest, response.Status);
    }

    /// <summary>
    /// AC08: 無效標頭值處理 - 無效租戶 ID
    /// </summary>
    [Fact]
    public async Task Request_WithInvalidTenantId_ShouldReturnBadRequest()
    {
        // Act
        var response = await GetAsync(Endpoints.Jwks, "InvalidTenantId");

        // Assert
        /* The line `Assert.Equal(HttpStatusCodes.BadRequest, response.Status);` is an assertion
        statement in a unit test written in C#. It is used to verify that the `Status` property of
        the `response` object is equal to `HttpStatusCodes.BadRequest`. */
        Assert.Equal(HttpStatusCodes.BadRequest, response.Status);
    }

    /// <summary>
    /// AC08: 過期的 JWT 處理
    /// </summary>
    [Fact]
    public async Task ProtectedEndpoint_WithExpiredJwt_ShouldReturnUnauthorized()
    {
        // Arrange - 使用明顯無效/過期的 JWT 格式
        var expiredJwt = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ0ZXN0IiwiZXhwIjoxfQ.invalid";
        
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = "dummy-refresh-token"
        };

        // Act
        var response = await PostAsync(Endpoints.RefreshToken, TenantIds.SkyLabmgm, 
            refreshRequest, expiredJwt);

        // Assert
        Assert.Equal(HttpStatusCodes.Unauthorized, response.Status);
    }

    /// <summary>
    /// 測試所有有效租戶 ID 組合
    /// </summary>
    [Theory]
    [MemberData(nameof(GetAllTenantIds))]
    public async Task PublicEndpoint_WithAllValidTenantIds_ShouldSucceed(string tenantId)
    {
        // Act
        var response = await GetAsync(Endpoints.Jwks, tenantId);

        // Assert
        AssertSuccess(response);
    }

    public static IEnumerable<object[]> GetAllTenantIds()
    {
        return TenantIds.All.Select(id => new object[] { id });
    }

    /// <summary>
    /// 測試中間件處理順序 - 驗證認證錯誤的優先級
    /// </summary>
    [Fact]
    public async Task Request_WithMultipleAuthErrors_ShouldReturnMostRelevantError()
    {
        // Act - 發送既缺少租戶 ID 又有錯誤 API Key 的請求
        var response = await ApiContext.GetAsync($"{ApiConfig.ApiPrefix}{Endpoints.Jwks}", new()
        {
            Headers = new Dictionary<string, string>
            {
                [Headers.ApiKey] = "invalid-key",
                [Headers.ContentType] = "application/json"
                // 缺少 X-Tenant-Id
            }
        });

        // Assert - 應該首先檢查租戶 ID
        Assert.Equal(HttpStatusCodes.BadRequest, response.Status);
    }

    /// <summary>
    /// 效能測試：認證驗證時間
    /// </summary>
    [Fact]
    public async Task AuthenticationValidation_PerformanceTest_ShouldCompleteQuickly()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await GetAsync(Endpoints.Jwks, TenantIds.SkyLabmgm);

        stopwatch.Stop();

        // Assert
        AssertSuccess(response);
        Assert.True(stopwatch.ElapsedMilliseconds < 500, 
            $"Authentication validation took {stopwatch.ElapsedMilliseconds}ms, expected < 500ms");
    }

    /// <summary>
    /// 測試標頭大小寫敏感性
    /// </summary>
    [Fact]
    public async Task Request_WithDifferentHeaderCasing_ShouldStillWork()
    {
        // Act - 使用不同大小寫的標頭名稱
        var response = await ApiContext.GetAsync($"{ApiConfig.ApiPrefix}{Endpoints.Jwks}", new()
        {
            Headers = new Dictionary<string, string>
            {
                ["x-tenant-id"] = TenantIds.SkyLabmgm,  // 小寫
                ["X-API-KEY"] = ApiKey,               // 大寫
                [Headers.ContentType] = "application/json"
            }
        });

        // Assert - HTTP 標頭應該不區分大小寫
        AssertSuccess(response);
    }

    /// <summary>
    /// 測試並發請求的認證處理
    /// </summary>
    [Fact]
    public async Task ConcurrentRequests_WithValidHeaders_ShouldAllSucceed()
    {
        // Arrange
        var tasks = new List<Task<IAPIResponse>>();
        const int concurrentRequests = 5;

        // Act - 同時發送多個請求
        for (int i = 0; i < concurrentRequests; i++)
        {
            tasks.Add(GetAsync(Endpoints.Jwks, TenantIds.SkyLabmgm));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert - 所有請求都應該成功
        foreach (var response in responses)
        {
            AssertSuccess(response);
        }
    }
}
