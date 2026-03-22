using System.Data;
using System.Text;
using System.Text.Json;
using Dapper;
using SkyLabIdP.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace SkyLabIdP.Data.Services;

/// <summary>
/// 權限提供者服務實作
/// 負責從資料庫查詢使用者權限並提供 Redis 快取機制，取代從 JWT Claims 讀取敏感權限資料
/// </summary>
public class PermissionProvider(
    IDbConnection connection,
    IDistributedCache cache,
    ILogger<PermissionProvider> logger) : IPermissionProvider
{
    private readonly IDbConnection _connection = connection;
    private readonly IDistributedCache _cache = cache;
    private readonly ILogger<PermissionProvider> _logger = logger;

    // 🧹 快取過期時間設定 - 30分鐘
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
    };

    public async Task<int> GetUserPermissionAsync(string userId, string functionName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(functionName))
        {
            _logger.LogWarning("⚠️ 使用者 ID 或功能名稱為空值，返回無權限");
            return 0;
        }

        var cacheKey = $"user_permissions:{userId}:{functionName}";

        // 🔄 嘗試從快取取得
        try
        {
            var cachedBytes = await _cache.GetAsync(cacheKey, cancellationToken);
            if (cachedBytes != null)
            {
                var cachedValue = Encoding.UTF8.GetString(cachedBytes);
                if (int.TryParse(cachedValue, out int cachedPermission))
                {
                    _logger.LogDebug("📋 從快取取得權限 - 使用者: {UserId}, 功能: {FunctionName}, 權限: {Permission}",
                        userId, functionName, cachedPermission);
                    return cachedPermission;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Redis 快取讀取失敗，將查詢資料庫 - 使用者: {UserId}, 功能: {FunctionName}", userId, functionName);
        }

        // 💾 從資料庫查詢權限 - 直接查詢 AspNetUserClaims 表
        try
        {
            var claimType = $"{functionName}.Permissions";
            const string sql = """
                SELECT TOP 1 ClaimValue
                FROM [AspNetUserClaims]
                WHERE UserId = @UserId AND ClaimType = @ClaimType
                """;

            var claimValue = await _connection.QueryFirstOrDefaultAsync<string>(
                sql,
                new { UserId = userId, ClaimType = claimType });

            var permissionValue = 0;
            if (claimValue != null && int.TryParse(claimValue, out int value))
            {
                permissionValue = value;
            }

            // 🧹 嘗試存入快取
            try
            {
                var permissionBytes = Encoding.UTF8.GetBytes(permissionValue.ToString());
                await _cache.SetAsync(cacheKey, permissionBytes, CacheOptions, cancellationToken);
            }
            catch (Exception cacheEx)
            {
                _logger.LogWarning(cacheEx, "⚠️ Redis 快取寫入失敗 - 使用者: {UserId}, 功能: {FunctionName}", userId, functionName);
            }

            _logger.LogInformation("💾 從資料庫查詢權限 - 使用者: {UserId}, 功能: {FunctionName}, 權限: {Permission}",
                userId, functionName, permissionValue);

            return permissionValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ 查詢使用者權限失敗 - 使用者: {UserId}, 功能: {FunctionName}", userId, functionName);
            return 0; // 發生錯誤時回傳無權限，確保安全性
        }
    }

    public async Task<Dictionary<string, int>> GetUserAllPermissionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("⚠️ 使用者 ID 為空值，返回空權限字典");
            return new Dictionary<string, int>();
        }

        var cacheKey = $"user_all_permissions:{userId}";

        // 🔄 嘗試從快取取得
        try
        {
            var cachedBytes = await _cache.GetAsync(cacheKey, cancellationToken);
            if (cachedBytes != null)
            {
                var cachedValue = Encoding.UTF8.GetString(cachedBytes);
                var cachedPermissions = JsonSerializer.Deserialize<Dictionary<string, int>>(cachedValue);
                if (cachedPermissions != null)
                {
                    _logger.LogDebug("📋 從快取取得所有權限 - 使用者: {UserId}, 權限數量: {Count}",
                        userId, cachedPermissions.Count);
                    return cachedPermissions;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Redis 快取讀取失敗，將查詢資料庫 - 使用者: {UserId}", userId);
        }

        // 💾 從資料庫查詢所有權限
        try
        {
            const string sql = """
                SELECT ClaimType, ClaimValue
                FROM [AspNetUserClaims]
                WHERE UserId = @UserId AND ClaimType IS NOT NULL AND ClaimType LIKE '%.Permissions'
                """;

            var userClaims = await _connection.QueryAsync<(string ClaimType, string ClaimValue)>(
                sql,
                new { UserId = userId });

            var permissions = new Dictionary<string, int>();
            foreach (var claim in userClaims)
            {
                if (!string.IsNullOrEmpty(claim.ClaimType))
                {
                    var claimFunctionName = claim.ClaimType.Replace(".Permissions", "");
                    if (int.TryParse(claim.ClaimValue, out int value))
                    {
                        permissions[claimFunctionName] = value;
                    }
                }
            }

            // 🧹 嘗試存入快取
            try
            {
                var serializedData = JsonSerializer.Serialize(permissions);
                var serializedBytes = Encoding.UTF8.GetBytes(serializedData);
                await _cache.SetAsync(cacheKey, serializedBytes, CacheOptions, cancellationToken);
            }
            catch (Exception cacheEx)
            {
                _logger.LogWarning(cacheEx, "⚠️ Redis 快取寫入失敗 - 使用者: {UserId}", userId);
            }

            return permissions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ 查詢使用者所有權限失敗 - 使用者: {UserId}", userId);
            return new Dictionary<string, int>();
        }
    }

    public async Task InvalidateUserPermissionCacheAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId)) return;

        try
        {
            var allPermissionsCacheKey = $"user_all_permissions:{userId}";
            await _cache.RemoveAsync(allPermissionsCacheKey, cancellationToken);
            _logger.LogInformation("🧹 清除使用者權限快取 - 使用者: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ 清除使用者權限快取失敗 - 使用者: {UserId}", userId);
        }
    }

    public async Task InvalidateFunctionPermissionCacheAsync(string userId, string functionName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(functionName)) return;

        try
        {
            var cacheKey = $"user_permissions:{userId}:{functionName}";
            await _cache.RemoveAsync(cacheKey, cancellationToken);

            var allPermissionsCacheKey = $"user_all_permissions:{userId}";
            await _cache.RemoveAsync(allPermissionsCacheKey, cancellationToken);

            _logger.LogInformation("🧹 清除功能權限快取 - 使用者: {UserId}, 功能: {FunctionName}", userId, functionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ 清除功能權限快取失敗 - 使用者: {UserId}, 功能: {FunctionName}", userId, functionName);
        }
    }
}
