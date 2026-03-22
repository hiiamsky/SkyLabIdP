using SkyLabIdP.Domain.Entities;

namespace SkyLabIdP.Application.Common.Interfaces.Repositories;

public interface IUserTenantRepository
{
    Task<bool> ExistsAsync(string userId, string tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(UserTenant entity, CancellationToken cancellationToken = default);
}
