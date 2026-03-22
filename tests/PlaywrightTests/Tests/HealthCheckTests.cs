using PlaywrightTests.Common;

namespace PlaywrightTests.Tests;

/// <summary>
/// 基本健康檢查測試，驗證 API 連接和 Playwright 設置
/// </summary>
[Collection("Sequential")]
public class HealthCheckTests : ApiTestBase
{
    /// <summary>
    /// 驗證 API 伺服器是否可達
    /// </summary>
    [Fact]
    public async Task HealthCheck_ApiServer_ShouldBeReachable()
    {
        // Act - 嘗試存取 JWKS 端點（公開端點）
        var response = await GetAsync(Endpoints.Jwks, TenantIds.SkyLabmgm);

        // Assert - 驗證伺服器有回應
        Assert.NotNull(response);
        
        // 允許 200 或其他狀態（因為可能還沒有完整的認證設置）
        // 重點是驗證連接是否建立
        Assert.True(response.Status >= 200 && response.Status < 500, 
            $"Expected server response but got status {response.Status}");
        
        var content = await response.TextAsync();
        Assert.NotNull(content);
    }

    /// <summary>
    /// 驗證環境變數設置
    /// </summary>
    [Fact]
    public void EnvironmentVariables_ApiKey_ShouldBeSet()
    {
        // Act & Assert
        var apiKey = Environment.GetEnvironmentVariable(ApiConfig.ApiKeyEnvVar);
        Assert.False(string.IsNullOrEmpty(apiKey), 
            $"Environment variable {ApiConfig.ApiKeyEnvVar} should be set for testing");
    }

    /// <summary>
    /// 驗證 API 基礎設定
    /// </summary>
    [Fact]
    public void ApiConfig_ShouldHaveCorrectValues()
    {
        // Assert - 驗證基本配置
        Assert.Equal("http://localhost:8083", ApiConfig.BaseUrl);
        Assert.Equal("/skylabidp/api/v1", ApiConfig.ApiPrefix);
        Assert.Equal("SKYLABIDP_APIKEY", ApiConfig.ApiKeyEnvVar);
        Assert.Equal(30, ApiConfig.DefaultTimeoutSeconds);
        
        // 驗證所有租戶 ID 都有定義
        Assert.Equal(6, TenantIds.All.Length);
        Assert.Contains(TenantIds.SkyLabmgm, TenantIds.All);
        Assert.Contains(TenantIds.SkyLabcommittee, TenantIds.All);
        Assert.Contains(TenantIds.SkyLabdevelop, TenantIds.All);
    }

    /// <summary>
    /// 驗證 Playwright API Context 初始化
    /// </summary>
    [Fact]
    public async Task PlaywrightContext_ShouldBeInitialized()
    {
        // Assert - 驗證 ApiContext 已正確初始化
        Assert.NotNull(ApiContext);
        
        // 建立一個簡單的請求來測試 context
        var response = await ApiContext.GetAsync($"{ApiConfig.ApiPrefix}/health-check-dummy");
        Assert.NotNull(response);
        
        // 不管回應內容，重點是 context 能夠發送請求
    }
}
