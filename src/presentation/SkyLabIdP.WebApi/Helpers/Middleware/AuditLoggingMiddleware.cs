using System;
using System.Diagnostics;
using System.Security.Claims;
using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Domain.Entities;
using SkyLabIdP.WebApi.Helpers.Utilities;

namespace SkyLabIdP.WebApi.Helpers.Middleware;

/// <summary>
/// 審計日誌中間件
/// </summary>
public class AuditLoggingMiddleware
{
    private const string AnonymousUser = "Anonymous";
    private const string UnknownIpAddress = "Unknown";

    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    /// <summary>
    /// 初始化 <see cref="AuditLoggingMiddleware"/> 類的新實例
    /// </summary>
    /// <param name="next">請求委託</param>
    /// <param name="logger">日誌記錄器</param>
    /// <param name="serviceScopeFactory">服務範圍工廠</param>
    public AuditLoggingMiddleware(
        RequestDelegate next,
        ILogger<AuditLoggingMiddleware> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _next = next;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <summary>
    /// 處理HTTP請求
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <returns>處理任務</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        // 跳過非API路徑的請求，例如靜態檔案
        if (!context.Request.Path.Value?.StartsWith("/skylabidp/api") ?? true)
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var requestWrapper = new HttpRequestWrapper(context.Request);
        var responseWrapper = new HttpResponseWrapper(context.Response);
        var requestBody = await requestWrapper.GetBodyAsync();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var responseBody = await responseWrapper.GetBodyAsync();
            await responseWrapper.CopyBodyToOriginalStreamAsync();

            // 檢查是否為二進制或大型內容類型
            var requestContentType = context.Request.ContentType?.ToLower() ?? string.Empty;
            var responseContentType = context.Response.ContentType?.ToLower() ?? string.Empty;

            // 不記錄二進制或大型內容以避免日誌過大
            if (ContentUtility.IsBinaryOrLargeContentType(requestContentType))
            {
                requestBody = $"[Binary content: {requestContentType}]";
            }

            if (ContentUtility.IsBinaryOrLargeContentType(responseContentType))
            {
                responseBody = $"[Binary content: {responseContentType}]";
            }

            // 敏感URL路徑(如含有密碼等)不應該記錄請求/回應主體
            bool isSensitiveUrl = ContentUtility.IsSensitiveUrl(context.Request.Path.Value ?? string.Empty);

            // 檢查路徑是否可能包含二進制內容
            bool isBinaryContentPath = ContentUtility.IsBinaryContentPath(context.Request.Path.Value ?? string.Empty);
            if (isBinaryContentPath)
            {
                // 如果路徑表明可能是二進制內容，則不記錄完整內容
                requestBody = requestBody.Length > 100
                    ? $"[Binary content (path-based detection), length: {requestBody.Length}]"
                    : requestBody;
                responseBody = responseBody.Length > 100
                    ? $"[Binary content (path-based detection), length: {responseBody.Length}]"
                    : responseBody;
            }

            // 檢查請求/響應主體大小，如果太大則截斷
            if (!isSensitiveUrl && !isBinaryContentPath)
            {
                // 為了避免日誌文件過大，限制記錄的內容長度
                const int maxLogLength = 10000; // 最大記錄長度：10K 字符

                requestBody = ContentUtility.TruncateContent(requestBody, maxLogLength);
                responseBody = ContentUtility.TruncateContent(responseBody, maxLogLength);
            }
            var auditLog = new AuditLog
            {
                UserId = GetDecryptedUserId(context),
                UserName = context.User.FindFirstValue(ClaimTypes.Name),
                TraceId = Activity.Current?.Id ?? context.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                RequestMethod = context.Request.Method,
                RequestPath = context.Request.Path,
                RequestQueryString = context.Request.QueryString.ToString(),
                // 應用敏感、二進制和大小限制的過濾
                RequestBody = isSensitiveUrl ? "[Sensitive data]" : requestBody,
                StatusCode = context.Response.StatusCode,
                ResponseBody = isSensitiveUrl ? "[Sensitive data]" : responseBody,
                ExecutionTime = stopwatch.ElapsedMilliseconds,
                // 獲取真實 IP 位址，與 ApiController 中的邏輯保持一致
                IPAddress = GetRealIpAddress(context),
                UserAgent = context.Request.Headers.UserAgent,
                AdditionalInfo = new Dictionary<string, string>
                {
                    ["RequestContentType"] = requestContentType,
                    ["ResponseContentType"] = responseContentType,
                    ["IsSensitiveUrl"] = isSensitiveUrl.ToString(),
                    ["IsBinaryContentPath"] = isBinaryContentPath.ToString(),
                    ["Scheme"] = context.Request.Scheme,
                    ["Host"] = context.Request.Host.Value ?? string.Empty
                }
            };

            // 非同步記錄審計，不影響響應時間，使用範圍服務工廠建立新的範圍
            _ = Task.Run(async () =>
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var auditLogService = scope.ServiceProvider.GetRequiredService<IAuditLogService>();
                await auditLogService.LogAsync(auditLog);
            });
        }
    }

    /// <summary>
    /// 獲取解密後的使用者 ID
    /// </summary>
    /// <param name="context">HTTP 上下文</param>
    /// <returns>解密後的使用者 ID</returns>
    private string GetDecryptedUserId(HttpContext context)
    {
        if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
        {
            string protectedUserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (!string.IsNullOrEmpty(protectedUserId))
            {
                try
                {
                    // 從當前請求的服務提供者獲取 IDataProtectionService
                    var dataProtectionService = context.RequestServices.GetRequiredService<IDataProtectionService>();
                    // 嘗試解密使用者 ID
                    return dataProtectionService.Unprotect(protectedUserId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "解密使用者 ID 失敗");
                    return AnonymousUser;
                }
            }
        }
        return AnonymousUser;
    }

    /// <summary>
    /// 獲取真實的使用者 IP 位址
    /// </summary>
    /// <param name="context">HTTP 上下文</param>
    /// <returns>真實的使用者 IP 位址</returns>
    private static string GetRealIpAddress(HttpContext context)
    {
        // 首先檢查常見的代理標頭，這些通常由 API Gateway 和反向代理設置

        // 1. 優先檢查 X-Forwarded-For 標頭 (最常見的方式)
        string forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // X-Forwarded-For 可能包含多個 IP，以第一個為真實客戶端 IP
            string[] ips = forwardedFor.Split(',');
            if (ips.Length > 0)
            {
                return ips[0].Trim();
            }
        }

        // 2. 嘗試從 X-Real-IP 標頭獲取 (某些 Nginx 配置使用)
        string realIp = context.Request.Headers["X-Real-IP"].ToString();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp.Trim();
        }

        // 3. 檢查 HttpContext.Items["IPAddress"]，這可能由其他中間件設置
        if (context.Items.TryGetValue("IPAddress", out var contextIp) && contextIp is string ipFromContext && !string.IsNullOrEmpty(ipFromContext))
        {
            return ipFromContext;
        }

        // 4. 最後才使用 RemoteIpAddress
        return context.Connection.RemoteIpAddress?.ToString() ?? UnknownIpAddress;
    }
}