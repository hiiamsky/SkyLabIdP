using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SkyLabIdP.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace SkyLabIdP.Shared.Services
{
    /// <summary>
    /// Redis implementation of the token storage service
    /// </summary>
    public class RedisTokenStorageService : ITokenStorageService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<RedisTokenStorageService> _logger;
        private const string TOKEN_PREFIX = "refresh_token:";
        private const string BLACKLIST_PREFIX = "blacklisted_token:";

        public RedisTokenStorageService(IDistributedCache distributedCache, ILogger<RedisTokenStorageService> logger)
        {
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Store a refresh token in Redis
        /// </summary>
        public async Task StoreRefreshTokenAsync(string userId,string tenantId, string refreshToken, DateTime expiryTime, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("【Redis服務】開始存儲刷新令牌 - 用戶ID: {UserId}, 租戶ID: '{TenantId}', 過期時間: {ExpiryTime}", 
                userId, tenantId, expiryTime);
                
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogError("【Redis服務】存儲刷新令牌失敗 - 用戶ID為空");
                    throw new ArgumentException("用戶ID不能為空", nameof(userId));
                }

                if (string.IsNullOrEmpty(refreshToken))
                {
                    _logger.LogError("【Redis服務】存儲刷新令牌失敗 - 令牌為空, 用戶ID: {UserId}", userId);
                    throw new ArgumentException("刷新令牌不能為空", nameof(refreshToken));
                }

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = expiryTime
                };

                string key = $"{TOKEN_PREFIX}{tenantId}{userId}";
                _logger.LogDebug("【Redis服務】生成Redis密鑰 - 用戶ID: {UserId}, 租戶ID: '{TenantId}', Redis鍵: '{RedisKey}'", 
                    userId, tenantId, key);

                var tokenBytes = Encoding.UTF8.GetBytes(refreshToken);
                _logger.LogDebug("【Redis服務】開始寫入Redis - 用戶ID: {UserId}, 令牌長度: {TokenLength}字節", 
                    userId, tokenBytes.Length);

                await _distributedCache.SetAsync(key, tokenBytes, options, cancellationToken);
                
                _logger.LogInformation("【Redis服務】成功存儲刷新令牌 - 用戶ID: {UserId}, 租戶ID: '{TenantId}', 過期時間: {ExpiryTime}", 
                    userId, tenantId, expiryTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "【Redis服務】存儲刷新令牌失敗 - 用戶ID: {UserId}, 租戶ID: '{TenantId}', 錯誤: {ErrorMessage}", 
                    userId, tenantId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get a refresh token from Redis
        /// </summary>
        public async Task<string> GetRefreshTokenAsync(string userId,string tenantId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("【Redis服務】開始獲取刷新令牌 - 用戶ID: {UserId}, 租戶ID: '{TenantId}'", 
                userId, tenantId);
                
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogError("【Redis服務】獲取刷新令牌失敗 - 用戶ID為空");
                    throw new ArgumentException("用戶ID不能為空", nameof(userId));
                }

                string key = $"{TOKEN_PREFIX}{tenantId}{userId}";
                _logger.LogDebug("【Redis服務】生成Redis查詢密鑰 - 用戶ID: {UserId}, 租戶ID: '{TenantId}', Redis鍵: '{RedisKey}'", 
                    userId, tenantId, key);

                _logger.LogDebug("【Redis服務】開始從Redis查詢令牌 - 用戶ID: {UserId}", userId);
                var tokenBytes = await _distributedCache.GetAsync(key, cancellationToken);
                
                if (tokenBytes == null || tokenBytes.Length == 0)
                {
                    _logger.LogInformation("【Redis服務】未找到刷新令牌 - 用戶ID: {UserId}, 租戶ID: '{TenantId}'", userId, tenantId);
                    return string.Empty;
                }
                
                _logger.LogDebug("【Redis服務】成功從Redis讀取令牌 - 用戶ID: {UserId}, 令牌長度: {TokenLength}字節", 
                    userId, tokenBytes.Length);
                
                var token = Encoding.UTF8.GetString(tokenBytes);
                
                _logger.LogInformation("【Redis服務】成功獲取刷新令牌 - 用戶ID: {UserId}, 租戶ID: '{TenantId}'", userId, tenantId);
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "【Redis服務】獲取刷新令牌失敗 - 用戶ID: {UserId}, 租戶ID: '{TenantId}', 錯誤: {ErrorMessage}", 
                    userId, tenantId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Remove a refresh token from Redis
        /// </summary>
        public async Task RemoveRefreshTokenAsync(string userId,string tenantId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("【Redis服務】開始移除刷新令牌 - 用戶ID: {UserId}, 租戶ID: '{TenantId}'", 
                userId, tenantId);
                
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogError("【Redis服務】移除刷新令牌失敗 - 用戶ID為空");
                    throw new ArgumentException("用戶ID不能為空", nameof(userId));
                }

                string key = $"{TOKEN_PREFIX}{tenantId}{userId}";
                _logger.LogDebug("【Redis服務】生成Redis移除密鑰 - 用戶ID: {UserId}, 租戶ID: '{TenantId}', Redis鍵: '{RedisKey}'", 
                    userId, tenantId, key);

                _logger.LogDebug("【Redis服務】開始從Redis移除令牌 - 用戶ID: {UserId}", userId);
                await _distributedCache.RemoveAsync(key, cancellationToken);
                
                _logger.LogInformation("【Redis服務】成功移除刷新令牌 - 用戶ID: {UserId}, 租戶ID: '{TenantId}'", userId, tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "【Redis服務】移除刷新令牌失敗 - 用戶ID: {UserId}, 租戶ID: '{TenantId}', 錯誤: {ErrorMessage}", 
                    userId, tenantId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Validate a refresh token against the one stored in Redis
        /// </summary>
        public async Task<bool> ValidateRefreshTokenAsync(string userId,string tenantId, string refreshToken, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("【Redis服務】開始驗證刷新令牌 - 用戶ID: {UserId}, 租戶ID: '{TenantId}'", 
                userId, tenantId);

            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning("【Redis服務】令牌驗證失敗 - 用戶ID: {UserId}, 原因: 令牌為空", userId);
                return false;
            }

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("【Redis服務】令牌驗證失敗 - 原因: 用戶ID為空");
                return false;
            }

            _logger.LogDebug("【Redis服務】獲取存儲的令牌進行比較 - 用戶ID: {UserId}", userId);
            var storedToken = await GetRefreshTokenAsync(userId, tenantId, cancellationToken);
            
            if (string.IsNullOrEmpty(storedToken))
            {
                _logger.LogWarning("【Redis服務】令牌驗證失敗 - 用戶ID: {UserId}, 租戶ID: '{TenantId}', 原因: 未找到存儲的令牌", 
                    userId, tenantId);
                return false;
            }
            
            _logger.LogDebug("【Redis服務】開始比較令牌 - 用戶ID: {UserId}, 存儲令牌長度: {StoredLength}, 輸入令牌長度: {InputLength}", 
                userId, storedToken.Length, refreshToken.Length);
            
            bool isValid = storedToken.Equals(refreshToken, StringComparison.Ordinal);
            
            if (isValid)
            {
                _logger.LogInformation("【Redis服務】令牌驗證成功 - 用戶ID: {UserId}, 租戶ID: '{TenantId}'", userId, tenantId);
            }
            else
            {
                _logger.LogWarning("【Redis服務】令牌驗證失敗 - 用戶ID: {UserId}, 租戶ID: '{TenantId}', 原因: 令牌不匹配", 
                    userId, tenantId);
            }
            
            return isValid;
        }

        /// <summary>
        /// Add an access token to the blacklist
        /// </summary>
        public async Task BlacklistAccessTokenAsync(string accessToken, DateTime expiryTime, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("【Redis服務】開始將存取令牌加入黑名單 - 過期時間: {ExpiryTime}", expiryTime);
            
            try
            {
                if (string.IsNullOrEmpty(accessToken))
                {
                    _logger.LogError("【Redis服務】加入黑名單失敗 - 存取令牌為空");
                    throw new ArgumentException("存取令牌不能為空", nameof(accessToken));
                }

                // 計算 token 的雜湊值作為鍵，避免在 Redis 中存儲整個 token
                _logger.LogDebug("【Redis服務】計算令牌雜湊值 - 令牌長度: {TokenLength}", accessToken.Length);
                string tokenHash = ComputeTokenHash(accessToken);
                string key = $"{BLACKLIST_PREFIX}{tokenHash}";
                
                _logger.LogDebug("【Redis服務】生成黑名單Redis鍵 - 雜湊值: {TokenHash}, Redis鍵: '{RedisKey}'", 
                    tokenHash[..8] + "...", key); // 只顯示部分雜湊值以保護隱私

                // 使用 token 的到期時間作為黑名單項的到期時間
                // 這樣當 token 自然過期時，黑名單項也會被自動清理
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = expiryTime
                };

                _logger.LogDebug("【Redis服務】設定黑名單項過期時間 - 過期時間: {ExpiryTime}", expiryTime);

                // 在黑名單中設置此 token，值不重要，僅標記為已被撤銷
                var revokedBytes = Encoding.UTF8.GetBytes("revoked");
                _logger.LogDebug("【Redis服務】開始寫入黑名單到Redis");
                
                await _distributedCache.SetAsync(key, revokedBytes, options, cancellationToken);
                
                _logger.LogInformation("【Redis服務】成功將令牌加入黑名單 - 雜湊值: {TokenHash}, 過期時間: {ExpiryTime}", 
                    tokenHash[..8] + "...", expiryTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "【Redis服務】將令牌加入黑名單失敗 - 錯誤: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Check if an access token is blacklisted
        /// </summary>
        public async Task<bool> IsAccessTokenBlacklistedAsync(string accessToken, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("【Redis服務】開始檢查令牌是否在黑名單中");
            
            try
            {
                if (string.IsNullOrEmpty(accessToken))
                {
                    _logger.LogWarning("【Redis服務】黑名單檢查失敗 - 存取令牌為空");
                    return false;
                }

                _logger.LogDebug("【Redis服務】計算令牌雜湊值進行黑名單查詢 - 令牌長度: {TokenLength}", accessToken.Length);
                string tokenHash = ComputeTokenHash(accessToken);
                string key = $"{BLACKLIST_PREFIX}{tokenHash}";
                
                _logger.LogDebug("【Redis服務】生成黑名單查詢鍵 - 雜湊值: {TokenHash}, Redis鍵: '{RedisKey}'", 
                    tokenHash[..8] + "...", key);
                
                // 檢查 token 是否在黑名單中
                _logger.LogDebug("【Redis服務】開始從Redis查詢黑名單狀態");
                var valueBytes = await _distributedCache.GetAsync(key, cancellationToken);
                bool isBlacklisted = valueBytes != null && valueBytes.Length > 0;
                
                if (isBlacklisted)
                {
                    _logger.LogWarning("【Redis服務】令牌已在黑名單中 - 雜湊值: {TokenHash}", tokenHash[..8] + "...");
                }
                else
                {
                    _logger.LogDebug("【Redis服務】令牌不在黑名單中 - 雜湊值: {TokenHash}", tokenHash[..8] + "...");
                }
                
                _logger.LogInformation("【Redis服務】黑名單檢查完成 - 結果: {IsBlacklisted}", isBlacklisted ? "在黑名單中" : "不在黑名單中");
                return isBlacklisted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "【Redis服務】檢查令牌黑名單狀態失敗 - 錯誤: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 計算 token 的雜湊值，用於在 Redis 中存儲
        /// </summary>
        private string ComputeTokenHash(string token)
        {
            _logger.LogDebug("【Redis服務】開始計算令牌SHA256雜湊值");
            
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(token);
                var hash = sha.ComputeHash(bytes);
                var base64Hash = Convert.ToBase64String(hash);
                
                _logger.LogDebug("【Redis服務】令牌雜湊值計算完成 - 雜湊值長度: {HashLength}", base64Hash.Length);
                return base64Hash;
            }
        }
    }
}