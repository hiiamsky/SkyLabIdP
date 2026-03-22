using SkyLabIdP.Application.Dtos.LoginUserInfo;
using SkyLabIdP.Domain.Entities;

namespace SkyLabIdP.Application.Common.Interfaces.Repositories;

public interface ISkyLabDevelopUserDetailRepository
{
    Task<SkyLabDevelopUserDetail?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<LoginUserInfoDto> GetTenantUserInfoAsync(string userId, CancellationToken cancellationToken = default);
    Task UpdateLastLoginTimeAsync(string userId, DateTime loginTime, CancellationToken cancellationToken = default);
    Task AddAsync(SkyLabDevelopUserDetail entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(SkyLabDevelopUserDetail entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsByOfficialEmailAsync(string email, CancellationToken cancellationToken = default);
}
