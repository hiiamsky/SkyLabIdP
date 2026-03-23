using SkyLabIdP.Domain.Entities;

namespace SkyLabIdP.Application.Common.Interfaces.Repositories;

public interface IFunctionRepository
{
    Task<IEnumerable<Function>> GetByGroupIdsAsync(IEnumerable<string> groupIds, CancellationToken cancellationToken = default);
    Task<Function?> GetByIdAsync(string functionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Function>> GetAllAsync(CancellationToken cancellationToken = default);
}
