using SkyLabIdP.Domain.Entities;

namespace SkyLabIdP.Application.Common.Interfaces.Repositories;

public interface IPasswordHistoryRepository
{
    Task<PasswordHistory?> GetLatestByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PasswordHistory>> GetLastNByUserIdAsync(string userId, int count, CancellationToken cancellationToken = default);
    Task<int> GetCountByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task AddAsync(PasswordHistory entity, CancellationToken cancellationToken = default);
}
