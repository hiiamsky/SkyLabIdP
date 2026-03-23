using SkyLabIdP.Domain.Entities;

namespace SkyLabIdP.Application.Common.Interfaces.Repositories;

public interface IBranchRepository
{
    Task<Branch?> GetByCodeAsync(string branchCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<Branch>> GetByCodesAsync(IEnumerable<string> codes, CancellationToken cancellationToken = default);
    Task<IEnumerable<Branch>> GetAllAsync(CancellationToken cancellationToken = default);
}
