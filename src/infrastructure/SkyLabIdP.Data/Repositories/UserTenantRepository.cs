using System.Data;
using Dapper;
using SkyLabIdP.Application.Common.Interfaces.Repositories;
using SkyLabIdP.Domain.Entities;

namespace SkyLabIdP.Data.Repositories;

public class UserTenantRepository : IUserTenantRepository
{
    private readonly IDbConnection _connection;
    private readonly IDbTransaction? _transaction;

    public UserTenantRepository(IDbConnection connection, IDbTransaction? transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<bool> ExistsAsync(string userId, string tenantId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CAST(CASE WHEN EXISTS (
                SELECT 1 FROM [UserTenant]
                WHERE UserId = @UserId AND TenantId = @TenantId
            ) THEN 1 ELSE 0 END AS BIT)
            """;

        return await _connection.ExecuteScalarAsync<bool>(
            sql,
            new { UserId = userId, TenantId = tenantId },
            _transaction);
    }

    public async Task AddAsync(UserTenant entity, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO [UserTenant] (TenantGuid, UserId, TenantId, CreateDateTime)
            VALUES (@TenantGuid, @UserId, @TenantId, @CreateDateTime)
            """;

        await _connection.ExecuteAsync(
            sql,
            new
            {
                entity.TenantGuid,
                entity.UserId,
                entity.TenantId,
                entity.CreateDateTime
            },
            _transaction);
    }
}
