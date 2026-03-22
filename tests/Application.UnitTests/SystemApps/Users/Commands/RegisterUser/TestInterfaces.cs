using System;
using System.Threading.Tasks;

namespace Application.UnitTests.SystemApps.Users.Commands.RegisterUser
{
    /// <summary>
    /// 測試用的 ICache 介面
    /// </summary>
    public interface ICache
    {
        Task<T?> GetAsync<T>(string key) where T : class;
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class;
        Task RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
    }

    /// <summary>
    /// 測試用的 ITokenStorageService 介面
    /// </summary>
    public interface ITokenStorageService
    {
        Task StoreTokenAsync(string userId, string token, TimeSpan expiry);
        Task<string?> GetTokenAsync(string userId);
        Task RemoveTokenAsync(string userId);
    }
}