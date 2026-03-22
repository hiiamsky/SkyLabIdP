using SkyLabIdP.Domain.Entities;

namespace SkyLabIdP.Application.Common.Interfaces.Repositories;

public interface ISysCodeRepository
{
    Task<IEnumerable<SysCode>> QueryAsync(string? type, string? code, CancellationToken cancellationToken = default);
}
