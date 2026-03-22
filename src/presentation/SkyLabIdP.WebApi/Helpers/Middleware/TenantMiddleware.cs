using System;

namespace SkyLabIdP.WebApi.Helpers.Middleware;
/// <summary>
/// Middleware to extract tenant information from the request header and store it in the HttpContext.Items collection.
/// </summary>
public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;
    private const int ForbiddenStatusCode = StatusCodes.Status403Forbidden;
    private const int CustomErrorStatusCode = StatusCodes.Status403Forbidden;
    /// <summary>
    /// Constructor for TenantMiddleware.
    /// </summary>
    /// <param name="next"></param>
    /// <param name="logger"></param>
    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    /// <summary>
    /// Invokes the middleware to extract tenant information from the request header and store it in HttpContext.Items.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    // Google OAuth 重定向流程是瀏覽器行為，無法帶自訂 Header，必須排除
    private static readonly string[] _bypassPaths =
    [
        "/skylabidp/api/v1/ExternalAuth/login",
        "/skylabidp/api/v1/ExternalAuth/callback",
        "/skylabidp/api/v1/ExternalAuth/google-callback",
        "/skylabidp/api/v1/Jwks/.well-known/jwks.json"
    ];

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (Array.Exists(_bypassPaths, p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        // 取得租戶資訊 (例如從 Header 中)
        var tenantId = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();

        if (string.IsNullOrEmpty(tenantId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                OperationResult = new
                {
                    Success = false,
                    Message = "未提供租戶識別碼 X-Tenant-Id",
                    StatusCode = StatusCodes.Status400BadRequest
                }
            });
            return;
        }

        // 將租戶 ID 寫入 HttpContext.Items，後續 Controller 可讀取
        context.Items["Tenant"] = tenantId;

        await _next(context);

    }

}