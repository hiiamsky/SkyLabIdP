namespace SkyLabIdP.Application.Common.Interfaces
{
    /// <summary>
    /// Minimal EF Core context abstraction for infrastructure services (e.g. ExternalLoginService)
    /// that need transaction and save capabilities through the Identity DbContext.
    /// No EF-specific types are exposed — the Application layer is free of EF dependencies.
    /// </summary>
    public interface IApplicationDbContext
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        Task SaveChangesAsync();

        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
