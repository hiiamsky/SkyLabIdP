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
                // 檢查是否有 Authorization 頭
                if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
                {
                    string authHeaderValue = authHeader.ToString();
                    if (authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        var token = authHeaderValue["Bearer ".Length..].Trim();

                        // 檢查令牌是否在黑名單中
                        if (!string.IsNullOrEmpty(token))
                        {
                            if (await tokenStorageService.IsAccessTokenBlacklistedAsync(token))
                            {
                                _logger.LogWarning("已拒絕一個使用黑名單令牌的請求");
                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                await context.Response.WriteAsync("無效的令牌，請重新登入");
                                return;
                            }

                            // 這裡添加手動驗證令牌的邏輯，以確保在後續中間件之前捕獲任何令牌問題
                            try
                            {
                                // 獲取JWKS來驗證令牌
                                var jwks = jwtService.GetJsonWebKeySet();
                                if (jwks != null && jwks.Keys.Count > 0)
                                {
                                    var handler = new JsonWebTokenHandler();
                                    
                                    // 先解析令牌以獲取租戶ID，但不進行驗證
                                    var tokenResult = handler.ReadJsonWebToken(token);
                                    var tenantIdClaim = tokenResult?.Claims.FirstOrDefault(c => c.Type == "tenant_id")?.Value;
                                    
                                    // 根據租戶ID選擇正確的Audience
                                    var config = context.RequestServices.GetRequiredService<IConfiguration>();
                                    string validAudience;
                                    
                                    if (!string.IsNullOrEmpty(tenantIdClaim))
                                    {
                                        // 嘗試從配置中獲取租戶特定的Audience
                                        var tenantAudience = config[$"JwtSettings:Audience:{tenantIdClaim}"];
                                        if (!string.IsNullOrEmpty(tenantAudience))
                                        {
                                            validAudience = tenantAudience;
                                            _logger.LogDebug("使用租戶 {TenantId} 的Audience: {Audience}", tenantIdClaim, validAudience);
                                        }
                                        else
                                        {
                                            // 如果沒有找到對應的租戶Audience，則使用默認值
                                            validAudience = config["JwtSettings:Audience"];
                                            _logger.LogWarning("未找到租戶 {TenantId} 的Audience配置，使用默認Audience: {DefaultAudience}", 
                                                tenantIdClaim, validAudience);
                                        }
                                    }
                                    else
                                    {
                                        // 沒有租戶ID，使用默認值
                                        validAudience = config["JwtSettings:Audience"];
                                        _logger.LogDebug("令牌中沒有租戶ID，使用默認Audience: {DefaultAudience}", validAudience);
                                    }
                                    
                                    // 設定令牌驗證參數
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
                                        return;
                                    }
                                    _logger.LogDebug("令牌驗證成功，租戶: {TenantId}", tenantIdClaim ?? "未指定");
                                    
                                    // 如果有租戶ID，將其添加到HttpContext中以便後續使用
                                    if (!string.IsNullOrEmpty(tenantIdClaim))
                                    {
                                        context.Items["TenantId"] = tenantIdClaim;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "令牌預驗證失敗");
                                // 我們不在這裡拒絕請求，而是讓標準驗證中間件處理它
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "處理令牌驗證時發生錯誤");
            }

            // 繼續處理請求
            await _next(context);
        }
    }
}