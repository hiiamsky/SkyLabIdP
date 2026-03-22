using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkyLabIdP.Application.Common.Interfaces
{
    public interface ITokenStorageService
    {
        /// <summary>
        /// Store a refresh token for a specific user
        /// </summary>
        Task StoreRefreshTokenAsync(string userId,string tenantId, string refreshToken, DateTime expiryTime, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Get stored refresh token for a user
        /// </summary>
        Task<string> GetRefreshTokenAsync(string userId,string tenantId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Remove refresh token for a user
        /// </summary>
        Task RemoveRefreshTokenAsync(string userId,string tenantId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Validate if a refresh token exists and matches for a user
        /// </summary>
        Task<bool> ValidateRefreshTokenAsync(string userId,string tenantId, string refreshToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Add an access token to blacklist when user logs out
        /// </summary>
        Task BlacklistAccessTokenAsync(string accessToken, DateTime expiryTime, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if an access token is blacklisted
        /// </summary>
        Task<bool> IsAccessTokenBlacklistedAsync(string accessToken, CancellationToken cancellationToken = default);
    }
}