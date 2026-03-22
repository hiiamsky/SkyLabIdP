using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Domain.Entities;
using SkyLabIdP.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;


namespace SkyLabIdP.Identity.Services
{
    public class JwtService(
        IApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        UserManager<ApplicationUser> userManager,
        IDataProtectionService dataprotectionservice ,
        ILogger<JwtService> logger,
        IKeyStoreService keyStoreService,
        ITokenStorageService tokenStorageService) : IJwtService
    {
        private readonly IApplicationDbContext _context = context;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IConfiguration _configuration = configuration;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IDataProtectionService _dataprotectionservice = dataprotectionservice;
        private readonly ILogger<JwtService> _logger = logger;
        private readonly IKeyStoreService _keyStoreService = keyStoreService;
        private readonly ITokenStorageService _tokenStorageService = tokenStorageService;
        
        public Task<string> GenerateTokenAsync(ApplicationUser user)
        {
            _logger.LogInformation("【JWT服務】開始生成傳統令牌 - 用戶ID: {UserId}, 用戶名: {UserName}", 
                user?.Id, user?.UserName);

            if (user == null)
            {
                _logger.LogError("【JWT服務】生成傳統令牌失敗 - 用戶為空");
                throw new ArgumentNullException(nameof(user), "用戶不能為空");
            }
            
            // 保持原有的方法，但轉為調用下面的生成存取令牌方法
            var result = GenerateAccessTokenAsync(user, Tenants.SkyLabmgm.ToString());
            
            _logger.LogInformation("【JWT服務】完成生成傳統令牌 - 用戶ID: {UserId}", user.Id);
            return result;
        }

        // 根據租戶ID獲取對應的Audience
        private string GetAudienceByTenantId(string tenantId)
        {
            _logger.LogDebug("【JWT服務】開始解析租戶對應的Audience - 租戶ID: '{TenantId}'", tenantId);
            
            if (string.IsNullOrEmpty(tenantId))
            {
                _logger.LogWarning("【JWT服務】租戶ID為空，使用默認Audience");
                var defaultAudience = _configuration["JwtSettings:Audience"] ?? string.Empty;
                _logger.LogDebug("【JWT服務】使用默認Audience: '{DefaultAudience}'", defaultAudience);
                return defaultAudience;
            }

            var audienceSection = _configuration.GetSection($"JwtSettings:Audience:{tenantId}");
            _logger.LogDebug("【JWT服務】檢查配置段落: 'JwtSettings:Audience:{TenantId}'", tenantId);
            
            if (audienceSection.Exists())
            {
                var audience = audienceSection.Value ?? string.Empty;
                _logger.LogDebug("【JWT服務】為租戶 {TenantId} 找到Audience: '{Audience}'", tenantId, audience);
                return audience;
            }
            else
            {
                _logger.LogWarning("【JWT服務】無法為租戶ID '{TenantId}' 找到對應的Audience，使用默認Audience", tenantId);
                var fallbackAudience = _configuration["JwtSettings:Audience"] ?? string.Empty;
                _logger.LogDebug("【JWT服務】使用回退Audience: '{FallbackAudience}'", fallbackAudience);
                return fallbackAudience;
            }
        }

        // 使用KeyStoreService中的密鑰生成存取令牌
        public async Task<string> GenerateAccessTokenAsync(ApplicationUser user, string tenantId = "")
        {
            _logger.LogInformation("【JWT服務】開始生成存取令牌 - 用戶ID: {UserId}, 用戶名: {UserName}, 租戶ID: '{TenantId}'", 
                user?.Id, user?.UserName, tenantId);

            if (user == null)
            {
                _logger.LogError("【JWT服務】生成存取令牌失敗 - 用戶為空");
                throw new ArgumentNullException(nameof(user), "用戶不能為空");
            }

            _logger.LogDebug("【JWT服務】檢查用戶狀態 - 用戶ID: {UserId}, IsActive: {IsActive}, LockoutEnabled: {LockoutEnabled}", 
                user.Id, user.IsActive, user.LockoutEnabled);

            // 檢查用戶狀態
            if (!user.IsActive  || user.LockoutEnabled)
            {
                _logger.LogWarning("【JWT服務】用戶狀態不允許生成存取令牌 - 用戶ID: {UserId}, IsActive: {IsActive}, LockoutEnabled: {LockoutEnabled}", 
                    user.Id, user.IsActive, user.LockoutEnabled);
                throw new InvalidOperationException("用戶狀態不允許生成存取令牌");
            }
            
            _logger.LogDebug("【JWT服務】用戶狀態檢查通過，開始生成令牌 - 用戶ID: {UserId}", user.Id);
            
            {
                _logger.LogDebug("【JWT服務】獲取RSA簽名密鑰 - 用戶ID: {UserId}", user.Id);
                var rsaKey = _keyStoreService.GetCurrentSigningKey();
                var keyId = _keyStoreService.GetCurrentKeyId();
                _logger.LogDebug("【JWT服務】獲得密鑰ID: {KeyId} - 用戶ID: {UserId}", keyId, user.Id);

                var credentials = new SigningCredentials(rsaKey, SecurityAlgorithms.RsaSha256)
                {
                    CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false },
                    Key = { KeyId = keyId }
                };

                // 獲取對應租戶的Audience
                _logger.LogDebug("【JWT服務】解析租戶Audience - 用戶ID: {UserId}, 租戶ID: '{TenantId}'", user.Id, tenantId);
                var audience = GetAudienceByTenantId(tenantId);
                _logger.LogDebug("【JWT服務】解析到Audience: '{Audience}' - 用戶ID: {UserId}", audience, user.Id);

                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, _dataprotectionservice.Protect(user.Id.ToString())),
                    new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iss, _configuration["JwtSettings:Issuer"] ?? string.Empty),
                    new Claim(JwtRegisteredClaimNames.Aud, audience)
                };
                
                _logger.LogDebug("【JWT服務】建立基本Claims完成 - 用戶ID: {UserId}, Claims數量: {ClaimsCount}", user.Id, claims.Count);
                
                // 新增租戶 ID 到存取令牌的 Claim 中
                if (!string.IsNullOrEmpty(tenantId))
                {
                    claims.Add(new Claim("tenant_id", tenantId));
                    _logger.LogDebug("【JWT服務】已將租戶 ID '{TenantId}' 添加到存取令牌中 - 用戶ID: {UserId}", tenantId, user.Id);
                }
                
                // 加入用戶角色 - 使用標準的 JWT claim 名稱
                _logger.LogDebug("【JWT服務】開始獲取用戶角色 - 用戶ID: {UserId}", user.Id);
                var userRoles = await _userManager.GetRolesAsync(user);
                _logger.LogDebug("【JWT服務】獲得用戶角色 - 用戶ID: {UserId}, 角色數量: {RoleCount}, 角色列表: [{Roles}]", 
                    user.Id, userRoles.Count, string.Join(", ", userRoles));
                
                foreach (var role in userRoles)
                {
                    claims.Add(new Claim("role", role)); // 使用簡潔的標準名稱
                }
                _logger.LogDebug("【JWT服務】已為用戶 {UserId} 添加 {RoleCount} 個角色到存取令牌中", user.Id, userRoles.Count);

                // 較短的存取令牌有效期限，例如30分鐘或1小時
                var accessTokenExpiryMinutes = Convert.ToInt32(_configuration["JwtSettings:AccessTokenExpiryInMinutes"] ?? "60"); // 默認60分鐘
                _logger.LogDebug("【JWT服務】設定令牌有效期限 - 用戶ID: {UserId}, 有效期限: {ExpiryMinutes}分鐘", user.Id, accessTokenExpiryMinutes);

                // 創建JWT描述
                var expiryTime = DateTime.UtcNow.AddMinutes(accessTokenExpiryMinutes);
                var descriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = expiryTime,
                    SigningCredentials = credentials
                };

                _logger.LogDebug("【JWT服務】開始創建JWT令牌 - 用戶ID: {UserId}, 過期時間: {ExpiryTime}", user.Id, expiryTime);
                
                var handler = new JsonWebTokenHandler();
                var token = handler.CreateToken(descriptor);
                
                _logger.LogInformation("【JWT服務】成功生成存取令牌 - 用戶ID: {UserId}, 租戶: '{TenantId}', Audience: '{Audience}', 有效期至: {ExpiryTime}", 
                    user.Id, tenantId, audience, expiryTime);
                    
                return token;
            }
        }
        public async Task<(string accessToken, string error)> RefreshAccessTokenAsync(string encryptedUserId, string refreshToken, string tenantId = "", string oldAccessToken = "")
        {
            _logger.LogInformation("【JWT服務】開始刷新存取令牌 - 加密用戶ID: {EncryptedUserId}, 租戶ID: '{TenantId}', 是否提供舊令牌: {HasOldToken}", 
                encryptedUserId, tenantId, !string.IsNullOrEmpty(oldAccessToken));
            
            try
            {
                // 解密用戶 ID
                _logger.LogDebug("【JWT服務】開始解密用戶ID");
                var decryptedUserId = _dataprotectionservice.Unprotect(encryptedUserId);
                _logger.LogInformation("【JWT服務】解密用戶ID成功 - 用戶ID: {UserId}, 租戶ID: '{TenantId}'", decryptedUserId, tenantId);

                // 根據租戶 ID 獲取對應的 Audience
                _logger.LogDebug("【JWT服務】解析租戶對應的Audience - 用戶ID: {UserId}, 租戶ID: '{TenantId}'", decryptedUserId, tenantId);
                var audience = GetAudienceByTenantId(tenantId);
                _logger.LogDebug("【JWT服務】解析到Audience: '{Audience}' - 用戶ID: {UserId}", audience, decryptedUserId);

                // 驗證刷新令牌
                _logger.LogDebug("【JWT服務】開始驗證刷新令牌 - 用戶ID: {UserId}", decryptedUserId);
                var tokenHandler = new JsonWebTokenHandler();
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["JwtSettings:Issuer"],
                    ValidAudience = audience, // 使用租戶特定的 audience
                    IssuerSigningKeys = _keyStoreService.GetAllValidationKeys(),
                    CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
                };

                _logger.LogDebug("【JWT服務】令牌驗證參數設定完成 - 用戶ID: {UserId}, Issuer: {Issuer}, Audience: '{Audience}'", 
                    decryptedUserId, tokenValidationParameters.ValidIssuer, audience);

                // 驗證令牌
                _logger.LogDebug("【JWT服務】執行令牌驗證 - 用戶ID: {UserId}", decryptedUserId);
                var result = await tokenHandler.ValidateTokenAsync(refreshToken, tokenValidationParameters);
                
                if (!result.IsValid)
                {
                    // 詳細的驗證失敗日誌
                    var errorDetails = new List<string>();
                    
                    if (result.Exception != null)
                    {
                        errorDetails.Add($"異常: {result.Exception.Message}");
                        
                        // 具體的驗證錯誤類型
                        switch (result.Exception)
                        {
                            case SecurityTokenExpiredException:
                                errorDetails.Add("令牌已過期");
                                break;
                            case SecurityTokenInvalidIssuerException:
                                errorDetails.Add($"Issuer不匹配 - 預期: {tokenValidationParameters.ValidIssuer}");
                                break;
                            case SecurityTokenInvalidAudienceException:
                                errorDetails.Add($"Audience不匹配 - 預期: {audience}");
                                break;
                            case SecurityTokenInvalidSignatureException:
                                errorDetails.Add("簽名驗證失敗");
                                break;
                            case SecurityTokenMalformedException:
                                errorDetails.Add("令牌格式錯誤");
                                break;
                            default:
                                errorDetails.Add($"其他驗證錯誤: {result.Exception.GetType().Name}");
                                break;
                        }
                    }

                    // 嘗試解析令牌基本信息（不驗證簽名）
                    try
                    {
                        var unvalidatedToken = new JsonWebTokenHandler().ReadJsonWebToken(refreshToken);
                        errorDetails.Add($"令牌Issuer: {unvalidatedToken.Issuer}");
                        errorDetails.Add($"令牌Audience: {string.Join(", ", unvalidatedToken.Audiences)}");
                        errorDetails.Add($"令牌過期時間: {unvalidatedToken.ValidTo:yyyy-MM-dd HH:mm:ss}");
                        errorDetails.Add($"當前時間: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
                        
                        var tokenType = unvalidatedToken.Claims.FirstOrDefault(c => c.Type == "tokenType")?.Value;
                        errorDetails.Add($"令牌類型: {tokenType}");
                        
                        var tenantIdInToken = unvalidatedToken.Claims.FirstOrDefault(c => c.Type == "tenant_id")?.Value;
                        errorDetails.Add($"令牌中的租戶ID: {tenantIdInToken}");
                    }
                    catch (Exception parseEx)
                    {
                        errorDetails.Add($"無法解析令牌: {parseEx.Message}");
                    }

                    var errorMessage = string.Join(" | ", errorDetails);
                    _logger.LogWarning("【JWT服務】令牌驗證失敗 - 用戶ID: {UserId}, 詳細信息: {ErrorDetails}", 
                        decryptedUserId, errorMessage);
                    
                    return (string.Empty, "令牌驗證失敗");
                }
                
                _logger.LogDebug("【JWT服務】令牌驗證成功 - 用戶ID: {UserId}", decryptedUserId);

                // 檢查令牌類型
                _logger.LogDebug("【JWT服務】檢查令牌類型 - 用戶ID: {UserId}", decryptedUserId);
                var tokenClaims = result.ClaimsIdentity.Claims;
                var tokenTypeClaim = tokenClaims.FirstOrDefault(c => c.Type == "tokenType")?.Value;
                _logger.LogDebug("【JWT服務】令牌類型聲明: '{TokenType}' - 用戶ID: {UserId}", tokenTypeClaim, decryptedUserId);
                
                if (tokenTypeClaim != "refresh")
                {
                    _logger.LogWarning("【JWT服務】令牌類型錯誤 - 用戶ID: {UserId}, 預期: 'refresh', 實際: '{TokenType}'", 
                        decryptedUserId, tokenTypeClaim);
                    return (string.Empty, "不是有效的刷新令牌");
                }

                // 驗證令牌中的用戶 ID
                _logger.LogDebug("【JWT服務】驗證令牌中的用戶ID - 用戶ID: {UserId}", decryptedUserId);
                var userIdClaim = tokenClaims.FirstOrDefault(c =>
                    c.Type == JwtRegisteredClaimNames.Sub)?.Value;
                _logger.LogDebug("【JWT服務】令牌中的用戶ID聲明: '{TokenUserId}' - 用戶ID: {UserId}", userIdClaim, decryptedUserId);
                var decryptedUserIdFromToken = userIdClaim != null ? _dataprotectionservice.Unprotect(userIdClaim) : null;
                if (decryptedUserIdFromToken != decryptedUserId)
                {
                    _logger.LogWarning("【JWT服務】令牌中的用戶ID與請求中的不匹配 - 用戶ID: {UserId}, 令牌中: '{TokenUserId}', 請求中: '{RequestUserId}'", 
                        decryptedUserId, decryptedUserIdFromToken, decryptedUserId);
                    return (string.Empty, "令牌中的用戶 ID 與請求中的不匹配");
                }

                // 獲取用戶
                _logger.LogDebug("【JWT服務】查找用戶 - 用戶ID: {UserId}", decryptedUserId);
                var user = await _userManager.FindByIdAsync(decryptedUserId);
                if (user == null)
                {
                    _logger.LogWarning("【JWT服務】找不到用戶 - 用戶ID: {UserId}", decryptedUserId);
                    return (string.Empty, "找不到用戶");
                }
                
                _logger.LogDebug("【JWT服務】找到用戶 - 用戶ID: {UserId}, 用戶名: {UserName}", user.Id, user.UserName);

                // 檢查用戶狀態
                _logger.LogDebug("【JWT服務】檢查用戶狀態 - 用戶ID: {UserId}, IsActive: {IsActive}, IsApproved: {IsApproved}, LockoutEnabled: {LockoutEnabled}",
                    user.Id, user.IsActive, user.IsApproved, user.LockoutEnabled);
                    
                if (!user.IsActive || !user.IsApproved || user.LockoutEnabled)
                {
                    _logger.LogWarning("【JWT服務】用戶狀態不允許刷新令牌 - 用戶ID: {UserId}, IsActive: {IsActive}, IsApproved: {IsApproved}, LockoutEnabled: {LockoutEnabled}",
                        user.Id, user.IsActive, user.IsApproved, user.LockoutEnabled);
                    return (string.Empty, "用戶狀態不允許刷新令牌");
                }
                
                
                // 使用傳入的租戶 ID
                var tenantIdClaim = tenantId;
                _logger.LogDebug("【JWT服務】檢查用戶租戶關係 - 用戶ID: {UserId}, 租戶ID: '{TenantId}'", user.Id, tenantIdClaim);
                
                //由 UserTenants 實體確認租戶 ID
                var userTenants = await _unitOfWork.UserTenants.ExistsAsync(user.Id, tenantIdClaim);
                    
                _logger.LogDebug("【JWT服務】用戶租戶關係檢查結果 - 用戶ID: {UserId}, 租戶ID: '{TenantId}', 關係存在: {UserTenantExists}", 
                    user.Id, tenantIdClaim, userTenants);
                    
                if (!userTenants)
                {
                    _logger.LogWarning("【JWT服務】用戶不屬於該租戶 - 用戶ID: {UserId}, 租戶ID: '{TenantId}'", user.Id, tenantIdClaim);
                    return (string.Empty, "用戶不屬於該租戶");
                }
                
                // 生成新的存取令牌
                _logger.LogDebug("【JWT服務】開始生成新的存取令牌 - 用戶ID: {UserId}, 租戶ID: '{TenantId}'", user.Id, tenantIdClaim);
                var newAccessToken = await GenerateAccessTokenAsync(user, tenantIdClaim ?? Tenants.SkyLabmgm.ToString());
                
                if (string.IsNullOrEmpty(newAccessToken))
                {
                    _logger.LogError("【JWT服務】生成新存取令牌失敗 - 用戶ID: {UserId}", user.Id);
                    return (string.Empty, "生成新存取令牌失敗");
                }
                
                _logger.LogDebug("【JWT服務】新存取令牌生成成功 - 用戶ID: {UserId}", user.Id);
                
                // 只有在成功生成新 token 後，才將舊的 access token 加入黑名單
                if (!string.IsNullOrEmpty(oldAccessToken))
                {
                    _logger.LogDebug("【JWT服務】開始處理舊令牌黑名單 - 用戶ID: {UserId}", user.Id);
                    try
                    {
                        // 解析舊 token 獲取過期時間
                        var oldTokenHandler = new JsonWebTokenHandler();
                        var oldJsonToken = oldTokenHandler.ReadJsonWebToken(oldAccessToken);
                        var expiry = oldJsonToken.ValidTo;
                        
                        _logger.LogDebug("【JWT服務】解析舊令牌成功 - 用戶ID: {UserId}, 過期時間: {Expiry}", user.Id, expiry);
                        
                        // 無論 token 是否過期，都將其加入黑名單以確保安全
                        await _tokenStorageService.BlacklistAccessTokenAsync(oldAccessToken, expiry);
                        _logger.LogDebug("【JWT服務】已將舊的 access token 加入黑名單 - 用戶ID: {UserId}, 過期時間: {Expiry}", user.Id, expiry);
                    }
                    catch (Exception ex)
                    {
                        // 如果無法解析舊 token，記錄警告但不影響刷新流程
                        _logger.LogWarning(ex, "【JWT服務】無法解析舊 access token 以加入黑名單 - 用戶ID: {UserId}", user.Id);
                    }
                }
                else
                {
                    _logger.LogDebug("【JWT服務】無舊令牌需要加入黑名單 - 用戶ID: {UserId}", user.Id);
                }
                
                _logger.LogInformation("【JWT服務】成功刷新令牌 - 用戶ID: {UserId}, 租戶ID: '{TenantId}'", user.Id, tenantIdClaim);
                return (newAccessToken, string.Empty);
            }
            catch (SecurityTokenExpiredException ex)
            {
                _logger.LogWarning(ex, "【JWT服務】刷新令牌已過期");
                return (string.Empty, "刷新令牌已過期，請重新登入");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "【JWT服務】刷新令牌時發生錯誤");
                return (string.Empty, "刷新令牌時發生錯誤");
            }
        }

        // 使用KeyStoreService中的密鑰生成刷新令牌
        public async Task<string> GenerateRefreshTokenAsync(ApplicationUser user, string tenantId = "")
        {
            _logger.LogInformation("【JWT服務】開始生成刷新令牌 - 用戶ID: {UserId}, 用戶名: {UserName}, 租戶ID: '{TenantId}'", 
                user?.Id, user?.UserName, tenantId);

            if (user == null)
            {
                _logger.LogError("【JWT服務】生成刷新令牌失敗 - 用戶為空");
                throw new ArgumentNullException(nameof(user), "用戶不能為空");
            }

            _logger.LogDebug("【JWT服務】獲取RSA簽名密鑰 - 用戶ID: {UserId}", user.Id);
            var rsaKey = _keyStoreService.GetCurrentSigningKey();
            var keyId = _keyStoreService.GetCurrentKeyId();
            _logger.LogDebug("【JWT服務】獲得密鑰ID: {KeyId} - 用戶ID: {UserId}", keyId, user.Id);

            var credentials = new SigningCredentials(rsaKey, SecurityAlgorithms.RsaSha256)
            {
                CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false },
                Key = { KeyId = keyId }
            };

            // 獲取對應租戶的Audience
            _logger.LogDebug("【JWT服務】解析租戶Audience (刷新令牌) - 用戶ID: {UserId}, 租戶ID: '{TenantId}'", user.Id, tenantId);
            var audience = GetAudienceByTenantId(tenantId);
            _logger.LogDebug("【JWT服務】解析到Audience (刷新令牌): '{Audience}' - 用戶ID: {UserId}", audience, user.Id);

            // 刷新令牌只需要最低限度的聲明
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, _dataprotectionservice.Protect(user.Id.ToString())),
                new Claim("tokenType", "refresh"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iss, _configuration["JwtSettings:Issuer"] ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Aud, audience)
            };
            
            _logger.LogDebug("【JWT服務】建立刷新令牌基本Claims完成 - 用戶ID: {UserId}, Claims數量: {ClaimsCount}", user.Id, claims.Count);
            
            // 新增租戶 ID 到刷新令牌的 Claim 中
            if (!string.IsNullOrEmpty(tenantId))
            {
                claims.Add(new Claim("tenant_id", tenantId));
                _logger.LogDebug("【JWT服務】已將租戶 ID '{TenantId}' 添加到刷新令牌中 - 用戶ID: {UserId}", tenantId, user.Id);
            }
            
            // 較長的刷新令牌有效期限，例如7天或30天
            var refreshTokenExpiryDays = Convert.ToInt32(_configuration["JwtSettings:RefreshTokenExpiryInDays"] ?? "7"); // 默認7天
            _logger.LogDebug("【JWT服務】設定刷新令牌有效期限 - 用戶ID: {UserId}, 有效期限: {ExpiryDays}天", user.Id, refreshTokenExpiryDays);

            // 創建JWT描述
            var expiryTime = DateTime.UtcNow.AddDays(refreshTokenExpiryDays);
            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiryTime,
                SigningCredentials = credentials
            };

            _logger.LogDebug("【JWT服務】開始創建刷新令牌 - 用戶ID: {UserId}, 過期時間: {ExpiryTime}", user.Id, expiryTime);
            
            var handler = new JsonWebTokenHandler();
            var token = handler.CreateToken(descriptor);
            
            _logger.LogDebug("【JWT服務】刷新令牌創建成功，準備存儲到Redis - 用戶ID: {UserId}, 令牌長度: {TokenLength}", 
                user.Id, token.Length);
            
            // 重要：將刷新令牌存儲到 Redis 中
            try
            {
                await _tokenStorageService.StoreRefreshTokenAsync(user.Id, tenantId, token, expiryTime);
                _logger.LogInformation("【JWT服務】刷新令牌已成功存儲到Redis - 用戶ID: {UserId}, 租戶: '{TenantId}', 過期時間: {ExpiryTime}", 
                    user.Id, tenantId, expiryTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "【JWT服務】存儲刷新令牌到Redis失敗 - 用戶ID: {UserId}, 租戶: '{TenantId}', 錯誤: {ErrorMessage}", 
                    user.Id, tenantId, ex.Message);
                throw new InvalidOperationException($"無法存儲刷新令牌到Redis: {ex.Message}", ex);
            }
            
            _logger.LogInformation("【JWT服務】完整的刷新令牌生成流程成功 - 用戶ID: {UserId}, 租戶: '{TenantId}', Audience: '{Audience}', 有效期至: {ExpiryTime}", 
                user.Id, tenantId, audience, expiryTime);
                
            return token;
        }

        // 獲取JWKS (JSON Web Key Set)
        public JsonWebKeySet GetJsonWebKeySet()
        {
            _logger.LogInformation("【JWT服務】開始生成JWKS");
            
            _logger.LogDebug("【JWT服務】獲取當前簽名密鑰");
            var rsaKey = _keyStoreService.GetCurrentSigningKey();
            var keyId = _keyStoreService.GetCurrentKeyId();
            _logger.LogDebug("【JWT服務】獲得密鑰ID: {KeyId}", keyId);

            _logger.LogDebug("【JWT服務】匯出RSA公鑰參數");
            var rsa = rsaKey.Rsa;
            var rsaParameters = rsa.ExportParameters(includePrivateParameters: false); // 只匯出公鑰部分
            _logger.LogDebug("【JWT服務】RSA公鑰參數匯出成功 - Modulus長度: {ModulusLength}, Exponent長度: {ExponentLength}", 
                rsaParameters.Modulus?.Length, rsaParameters.Exponent?.Length);

            var jwk = new JsonWebKey
            {
                Kty = "RSA",
                Use = "sig",
                Kid = keyId,
                Alg = "RS256",
                N = Base64UrlEncoder.Encode(rsaParameters.Modulus),
                E = Base64UrlEncoder.Encode(rsaParameters.Exponent)
            };

            _logger.LogDebug("【JWT服務】JWK建立完成 - KeyId: {KeyId}, Algorithm: {Algorithm}, Use: {Use}", 
                jwk.Kid, jwk.Alg, jwk.Use);

            var jsonWebKeySet = new JsonWebKeySet();
            jsonWebKeySet.Keys.Add(jwk);

            _logger.LogInformation("【JWT服務】成功生成JWKS - 包含 {KeyCount} 個RSA公鑰, KeyId: {KeyId}", 
                jsonWebKeySet.Keys.Count, keyId);

            return jsonWebKeySet;
        }
    }
}