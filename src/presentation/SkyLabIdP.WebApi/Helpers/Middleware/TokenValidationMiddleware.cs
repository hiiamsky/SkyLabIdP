using SkyLabIdP.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SkyLabIdP.Identity.Services;
using SkyLabIdP.Domain.Enums;

namespace SkyLabIdP.WebApi.Helpers.Middleware
{
    /// <summary>
    /// 檢查每個請求中的 JWT token 是否在黑名單中
    /// </summary>
    public class TokenValidationMiddleware(RequestDelegate next, ILogger<TokenValidationMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<TokenValidationMiddleware> _logger = logger;

        /// <summary>
        /// 處理HTTP請求並驗證JWT令牌
        /// </summary>
        /// <param name="context">HTTP上下文</param>
        /// <param name="tokenStorageService">令牌存儲服務</param>
        /// <param name="jwtService">JWT服務</param>
        /// <returns>處理請求的非同步任務</returns>
        public async Task InvokeAsync(HttpContext context, ITokenStorageService tokenStorageService, IJwtService jwtService)
        {
            try
            {
                var token = ExtractBearerToken(context);
                if (!string.IsNullOrEmpty(token))
                {
                    if (await RejectIfBlacklistedAsync(token, tokenStorageService, context))
                        return;

                    if (await RejectIfInvalidJwksAsync(token, context, jwtService))
                        return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "處理令牌驗證時發生錯誤");
            }

            await _next(context);
        }

        private static string? ExtractBearerToken(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
                return null;

            var authHeaderValue = authHeader.ToString();
            if (!authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return null;

            var token = authHeaderValue["Bearer ".Length..].Trim();
            return string.IsNullOrEmpty(token) ? null : token;
        }

        /// <returns>true 表示已拒絕請求（已寫入 401 回應）</returns>
        private async Task<bool> RejectIfBlacklistedAsync(string token, ITokenStorageService tokenStorageService, HttpContext context)
        {
            if (!await tokenStorageService.IsAccessTokenBlacklistedAsync(token))
                return false;

            _logger.LogWarning("已拒絕一個使用黑名單令牌的請求");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("無效的令牌，請重新登入");
            return true;
        }

        /// <returns>true 表示已拒絕請求（已寫入 401 回應）</returns>
        private async Task<bool> RejectIfInvalidJwksAsync(string token, HttpContext context, IJwtService jwtService)
        {
            try
            {
                var jwks = jwtService.GetJsonWebKeySet();
                if (jwks == null || jwks.Keys.Count == 0)
                    return false;

                var handler = new JsonWebTokenHandler();
                var jsonWebToken = handler.ReadJsonWebToken(token);
                var tenantIdClaim = jsonWebToken?.Claims.FirstOrDefault(c => c.Type == "tenant_id")?.Value;

                var config = context.RequestServices.GetRequiredService<IConfiguration>();
                var validAudience = ResolveAudience(tenantIdClaim, config);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config["JwtSettings:Issuer"],
                    ValidAudience = validAudience,
                    IssuerSigningKeys = jwks.Keys,
                    ClockSkew = TimeSpan.Zero
                };

                var result = await handler.ValidateTokenAsync(token, validationParameters);
                if (!result.IsValid)
                {
                    _logger.LogWarning("令牌驗證失敗：{ErrorMessage}", result.Exception?.Message);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("無效的令牌，請重新登入");
                    return true;
                }

                _logger.LogDebug("令牌驗證成功，租戶: {TenantId}", tenantIdClaim ?? "未指定");
                if (!string.IsNullOrEmpty(tenantIdClaim))
                {
                    context.Items["TenantId"] = tenantIdClaim;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "令牌預驗證失敗");
                // 不在這裡拒絕請求，讓標準驗證中間件處理
            }

            return false;
        }

        private string ResolveAudience(string? tenantIdClaim, IConfiguration config)
        {
            if (string.IsNullOrEmpty(tenantIdClaim))
            {
                var defaultAudience = config["JwtSettings:Audience"] ?? string.Empty;
                _logger.LogDebug("令牌中沒有租戶ID，使用默認Audience: {DefaultAudience}", defaultAudience);
                return defaultAudience;
            }

            var tenantAudience = config[$"JwtSettings:Audience:{tenantIdClaim}"];
            if (!string.IsNullOrEmpty(tenantAudience))
            {
                _logger.LogDebug("使用租戶 {TenantId} 的Audience: {Audience}", tenantIdClaim, tenantAudience);
                return tenantAudience;
            }

            var fallback = config["JwtSettings:Audience"] ?? string.Empty;
            _logger.LogWarning("未找到租戶 {TenantId} 的Audience配置，使用默認Audience: {DefaultAudience}",
                tenantIdClaim, fallback);
            return fallback;
        }
    }
}