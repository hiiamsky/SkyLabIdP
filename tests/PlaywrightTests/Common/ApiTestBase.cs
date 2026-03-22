using Microsoft.Playwright;
using System.Text;
using System.Text.Json;

namespace PlaywrightTests.Common;

/// <summary>
/// API 測試的基礎類別，提供通用的 HTTP 操作方法
/// </summary>
public class ApiTestBase : IAsyncLifetime
{
    protected IAPIRequestContext ApiContext { get; private set; } = null!;
    private IPlaywright? _playwright;
    
    /// <summary>
    /// 系統 API 金鑰（從環境變數取得）
    /// </summary>
    protected string ApiKey => Environment.GetEnvironmentVariable(ApiConfig.ApiKeyEnvVar) 
        ?? throw new InvalidOperationException($"Environment variable {ApiConfig.ApiKeyEnvVar} is not set");

    public virtual async Task InitializeAsync()
    {
        // 建立 Playwright 實例
        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        
        // 建立 API 請求上下文
        ApiContext = await _playwright.APIRequest.NewContextAsync(new()
        {
            BaseURL = ApiConfig.BaseUrl,
            Timeout = ApiConfig.DefaultTimeoutSeconds * 1000,
            IgnoreHTTPSErrors = true, // 開發環境可能使用自簽憑證
            ExtraHTTPHeaders = new Dictionary<string, string>
            {
                ["User-Agent"] = "SkyLabIdP-E2E-Tests/1.0"
            }
        });
    }

    public virtual async Task DisposeAsync()
    {
        if (ApiContext != null)
            await ApiContext.DisposeAsync();
        
        _playwright?.Dispose();
    }

    /// <summary>
    /// 建立包含基本認證標頭的 HTTP 標頭字典
    /// </summary>
    /// <param name="tenantId">租戶 ID</param>
    /// <param name="includeApiKey">是否包含 API Key</param>
    /// <param name="authToken">可選的 JWT 認證令牌</param>
    /// <returns>HTTP 標頭字典</returns>
    protected Dictionary<string, string> CreateHeaders(
        string tenantId, 
        bool includeApiKey = true, 
        string? authToken = null)
    {
        var headers = new Dictionary<string, string>
        {
            [Headers.TenantId] = tenantId,
            [Headers.ContentType] = "application/json"
        };

        if (includeApiKey)
        {
            headers[Headers.ApiKey] = ApiKey;
        }

        if (!string.IsNullOrEmpty(authToken))
        {
            headers[Headers.Authorization] = $"Bearer {authToken}";
        }

        return headers;
    }

    /// <summary>
    /// 發送 GET 請求
    /// </summary>
    protected async Task<IAPIResponse> GetAsync(
        string endpoint, 
        string tenantId, 
        string? authToken = null,
        Dictionary<string, string>? queryParams = null)
    {
        var url = $"{ApiConfig.ApiPrefix}{endpoint}";
        
        if (queryParams?.Any() == true)
        {
            var queryString = string.Join("&", 
                queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
            url += $"?{queryString}";
        }

        return await ApiContext.GetAsync(url, new()
        {
            Headers = CreateHeaders(tenantId, true, authToken)
        });
    }

    /// <summary>
    /// 發送 POST 請求
    /// </summary>
    protected async Task<IAPIResponse> PostAsync<T>(
        string endpoint, 
        string tenantId, 
        T data,
        string? authToken = null)
    {
        var json = JsonSerializer.Serialize(data, ApiConfig.JsonOptions);
        
        return await ApiContext.PostAsync($"{ApiConfig.ApiPrefix}{endpoint}", new()
        {
            Headers = CreateHeaders(tenantId, true, authToken),
            Data = json
        });
    }

    /// <summary>
    /// 發送 PUT 請求
    /// </summary>
    protected async Task<IAPIResponse> PutAsync<T>(
        string endpoint, 
        string tenantId, 
        T data,
        string? authToken = null)
    {
        var json = JsonSerializer.Serialize(data, ApiConfig.JsonOptions);
        
        return await ApiContext.PutAsync($"{ApiConfig.ApiPrefix}{endpoint}", new()
        {
            Headers = CreateHeaders(tenantId, true, authToken),
            Data = json
        });
    }

    /// <summary>
    /// 發送 DELETE 請求
    /// </summary>
    protected async Task<IAPIResponse> DeleteAsync(
        string endpoint, 
        string tenantId, 
        string? authToken = null)
    {
        return await ApiContext.DeleteAsync($"{ApiConfig.ApiPrefix}{endpoint}", new()
        {
            Headers = CreateHeaders(tenantId, true, authToken)
        });
    }

    /// <summary>
    /// 發送 PATCH 請求
    /// </summary>
    protected async Task<IAPIResponse> PatchAsync<T>(
        string endpoint, 
        string tenantId, 
        T data,
        string? authToken = null)
    {
        var json = JsonSerializer.Serialize(data, ApiConfig.JsonOptions);
        
        return await ApiContext.PatchAsync($"{ApiConfig.ApiPrefix}{endpoint}", new()
        {
            Headers = CreateHeaders(tenantId, true, authToken),
            Data = json
        });
    }

    /// <summary>
    /// 解析 API 回應為指定類型
    /// </summary>
    protected async Task<T?> ParseResponseAsync<T>(IAPIResponse response)
    {
        var content = await response.TextAsync();
        
        if (string.IsNullOrEmpty(content))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(content, ApiConfig.JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse response as {typeof(T).Name}: {content}", ex);
        }
    }

    /// <summary>
    /// 驗證 API 回應狀態碼
    /// </summary>
    protected static void AssertStatusCode(IAPIResponse response, int expectedStatusCode)
    {
        Assert.Equal(expectedStatusCode, response.Status);
    }

    /// <summary>
    /// 驗證 API 回應是否成功
    /// </summary>
    protected static void AssertSuccess(IAPIResponse response)
    {
        Assert.True(response.Ok, $"API request failed with status {response.Status}: {response.StatusText}");
    }

    /// <summary>
    /// 上傳檔案
    /// </summary>
    protected async Task<IAPIResponse> UploadFileAsync(
        string endpoint,
        string tenantId,
        string filePath,
        string? authToken = null,
        Dictionary<string, string>? additionalFields = null)
    {
        var headers = CreateHeaders(tenantId, true, authToken);
        headers.Remove(Headers.ContentType); // Let Playwright set multipart content type

        // 使用正確的 Playwright FormData 語法
        var multipartData = new Dictionary<string, object>
        {
            ["file"] = new FilePayload
            {
                Name = Path.GetFileName(filePath),
                MimeType = "application/pdf", // 預設 PDF
                Buffer = await File.ReadAllBytesAsync(filePath)
            }
        };

        if (additionalFields != null)
        {
            foreach (var field in additionalFields)
            {
                multipartData[field.Key] = field.Value;
            }
        }

        return await ApiContext.PostAsync($"{ApiConfig.ApiPrefix}{endpoint}", new()
        {
            Headers = headers,
            Multipart = (IFormData)multipartData
        });
    }

    /// <summary>
    /// 等待指定時間（用於測試中的延遲）
    /// </summary>
    protected static async Task WaitAsync(int milliseconds)
    {
        await Task.Delay(milliseconds);
    }

    /// <summary>
    /// 生成測試用的隨機字串
    /// </summary>
    protected static string GenerateRandomString(int length = 8)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    /// <summary>
    /// 生成測試用電子郵件地址
    /// </summary>
    protected static string GenerateTestEmail(string prefix = "test")
    {
        return $"{prefix}_{GenerateRandomString()}@example.com";
    }
}
