using SkyLabIdP.Domain.Entities;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace SkyLabIdP.Application.Common.Interfaces;

public interface IJwtService
{
    Task<string> GenerateTokenAsync(ApplicationUser user);
    Task<string> GenerateAccessTokenAsync(ApplicationUser user, string tenantId = "");
    Task<string> GenerateRefreshTokenAsync(ApplicationUser user, string tenantId= "");
    Task<(string accessToken, string error)> RefreshAccessTokenAsync(string encryptedUserId, string refreshToken, string tenantId = "", string oldAccessToken = "");
    JsonWebKeySet GetJsonWebKeySet();
}
