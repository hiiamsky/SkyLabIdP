using System.Data;
using SkyLabIdP.Application.Common.Interfaces.Repositories;

namespace SkyLabIdP.Application.Common.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
    IUserTenantRepository UserTenants { get; }
    IPasswordHistoryRepository PasswordHistories { get; }
    ISkyLabDocUserDetailRepository SkyLabDocUserDetails { get; }
    ISkyLabDevelopUserDetailRepository SkyLabDevelopUserDetails { get; }
    IAuditLogRepository AuditLogs { get; }
    IBranchRepository Branches { get; }
    IFunctionGroupRepository FunctionGroups { get; }
    IFunctionRepository Functions { get; }
    IFileUploadRepository FileUploads { get; }
    ISysCodeRepository SysCodes { get; }
    IBranchAreaRepository BranchAreas { get; }

    IDbTransaction? CurrentTransaction { get; }

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
