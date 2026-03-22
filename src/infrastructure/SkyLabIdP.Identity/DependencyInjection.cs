using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Identity.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Cryptography;
using System.Text;

namespace SkyLabIdP.Identity;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        // 從配置中讀取 JWT 設定並驗證
        var issuer = configuration["JwtSettings:Issuer"];

        // 驗證必要的配置參數
        ArgumentException.ThrowIfNullOrEmpty(issuer, nameof(issuer));

        // 處理多個 audience 的情況
        var audienceSection = configuration.GetSection("JwtSettings:Audience");
        var audiences = new List<string>();

        if (audienceSection.Exists())
        {
            // 如果 Audience 是對象，獲取所有值
            foreach (var child in audienceSection.GetChildren())
            {
                var audienceValue = child.Value;
                if (!string.IsNullOrEmpty(audienceValue))
                {
                    audiences.Add(audienceValue);
                }
            }
        }
        else
        {
            // 如果 Audience 是單一字串
            var singleAudience = configuration["JwtSettings:Audience"];
            if (!string.IsNullOrEmpty(singleAudience))
            {
                audiences.Add(singleAudience);
            }
        }

        // 確保至少有一個 audience
        if (!audiences.Any())
        {
            throw new ArgumentException("至少需要配置一個有效的 JWT Audience", nameof(audiences));
        }

        // 註冊 KeyStoreService (JWT 密鑰管理)
        services.AddSingleton<IKeyStoreService, InMemoryKeyStoreService>();

        // 註冊 JwtService (JWT 令牌生成)
        services.AddScoped<IJwtService, JwtService>();

        // 註冊 ExternalLoginHandler (外部登入處理)
        services.AddScoped<IExternalLoginHandler, ExternalLoginService>();

        // 註冊金鑰輪換背景服務
        services.AddHostedService<KeyRotationBackgroundService>();

        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = "MultiAuth";
                options.DefaultChallengeScheme = "MultiAuth";
            }).AddCookie("Cookies", options =>
            {
                options.Cookie.Name = "SkyLabIdP.AuthCookie";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;

                // 根據環境調整 SameSite 設定
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (environment == "Development")
                {
                    // 開發環境允許跨站使用
                    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                }
                else
                {
                    // 生產環境使用 Lax，除非需要在跨站中使用
                    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                }

                options.LoginPath = "/skylabidp/api/v1/Account/Login";
                options.LogoutPath = "/skylabidp/api/v1/Account/Logout";
            }).AddJwtBearer(options =>
            {
                // 配置令牌驗證參數
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudiences = audiences, // 使用多個 audiences
                    ValidAlgorithms = new[] { SecurityAlgorithms.RsaSha256 },
                    ClockSkew = TimeSpan.Zero,
                    // 🔧 設定角色 claim 類型為簡潔格式
                    RoleClaimType = "role",  // 使用標準的 role claim 名稱
                    NameClaimType = JwtRegisteredClaimNames.UniqueName
                };

                // 添加事件處理
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtService>>();
                        logger.LogDebug("收到JWT令牌驗證請求");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtService>>();
                        logger.LogInformation("JWT令牌驗證成功");
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtService>>();
                        logger.LogWarning(context.Exception, "JWT令牌驗證失敗: {Message}", context.Exception.Message);

                        // 檢查是否是密鑰不存在的錯誤
                        if (context.Exception is SecurityTokenSignatureKeyNotFoundException)
                        {
                            try
                            {
                                // 獲取密鑰存儲服務
                                var keyStoreService = context.HttpContext.RequestServices.GetRequiredService<IKeyStoreService>();
                                var allKeys = keyStoreService.GetAllValidationKeys();

                                // 將所有密鑰添加到驗證參數
                                if (allKeys.Any())
                                {
                                    logger.LogInformation("發現 {Count} 個密鑰，嘗試更新TokenValidationParameters", allKeys.Count());

                                    // 在每次失敗時更新密鑰，嘗試解決運行時密鑰變化的問題
                                    if (context.Options != null && context.Options.TokenValidationParameters != null)
                                    {
                                        context.Options.TokenValidationParameters.IssuerSigningKeys = allKeys;
                                        logger.LogInformation("已將密鑰添加到TokenValidationParameters");
                                    }
                                }
                                else
                                {
                                    logger.LogWarning("未找到可用的密鑰");
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "嘗試獲取密鑰時發生錯誤");
                            }
                        }

                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtService>>();
                        logger.LogWarning("JWT令牌驗證挑戰: {Error}, {ErrorDescription}",
                            context.Error, context.ErrorDescription);
                        return Task.CompletedTask;
                    }
                };
                // 配置 JWKS 端點的網址
                var baseUrl = configuration["AppSettings:BaseUrl"];
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    var jwksUri = $"{baseUrl}/skylabidp/api/v1/Jwks/.well-known/jwks.json";
                    // 移除 BuildServiceProvider 的使用，記錄將在實際請求時進行
                }

                // 允許訪問自簽證書
                options.BackchannelHttpHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
                // 註冊金鑰初始化到事件中，避免在服務註冊時使用 BuildServiceProvider
                options.Events.OnMessageReceived = context =>
                {
                    // 確保密鑰在第一次請求時就已載入
                    try
                    {
                        var keyStoreService = context.HttpContext.RequestServices.GetRequiredService<IKeyStoreService>();
                        var allKeys = keyStoreService.GetAllValidationKeys();

                        if (allKeys.Any() && context.Options?.TokenValidationParameters != null)
                        {
                            context.Options.TokenValidationParameters.IssuerSigningKeys = allKeys;
                        }
                    }
                    catch (Exception ex)
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                        logger.LogError(ex, "在載入JWT密鑰時發生錯誤");
                    }

                    return Task.CompletedTask;
                };
            })
            // 動態配置多租戶 Google OAuth - 使用環境變數
            .AddGoogle(options =>
            {
                options.ClientId = Environment.GetEnvironmentVariable("OAUTH_GOOGLE_DEFAULT_CLIENT_ID") ?? "";
                options.ClientSecret = Environment.GetEnvironmentVariable("OAUTH_GOOGLE_DEFAULT_CLIENT_SECRET") ?? "";
                options.CallbackPath = "/skylabidp/api/v1/ExternalAuth/google-callback";
                options.SaveTokens = true;
            })
            .AddPolicyScheme("MultiAuth", "Bearer or Cookie Auth", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    // 1. 先檢查是否是 API 路徑
                    if (context.Request.Path.StartsWithSegments("/api") ||
                        context.Request.Path.StartsWithSegments("/skylabidp/api"))
                    {
                        // API 路徑優先檢查 Authorization 標頭
                        string? authorization = context.Request.Headers["Authorization"];
                        if (!string.IsNullOrEmpty(authorization) &&
                            authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            return JwtBearerDefaults.AuthenticationScheme;
                        }

                        // 沒有 Bearer 但有 Cookie，也可以接受 Cookie
                        if (context.Request.Cookies.ContainsKey("SkyLabIdP.AuthCookie"))
                        {
                            return "Cookies";
                        }

                        // 預設 API 使用 Bearer 驗證失敗
                        return JwtBearerDefaults.AuthenticationScheme;
                    }

                    // 2. 非 API 路徑默認使用 Cookie
                    return "Cookies";
                };
            });

        services.AddSingleton<IApiKeyValidation, ApiKeyValidation>();
        return services;
    }
}
