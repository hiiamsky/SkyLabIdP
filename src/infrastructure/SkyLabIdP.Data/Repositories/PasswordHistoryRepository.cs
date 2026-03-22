using System.Data;
using Dapper;
using SkyLabIdP.Application.Common.Interfaces.Repositories;
using SkyLabIdP.Domain.Entities;

namespace SkyLabIdP.Data.Repositories;

public class PasswordHistoryRepository : IPasswordHistoryRepository
{
    private readonly IDbConnection _connection;
    private readonly IDbTransaction? _transaction;

    public PasswordHistoryRepository(IDbConnection connection, IDbTransaction? transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<PasswordHistory?> GetLatestByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT TOP 1 SerialNo, UserId, HashedPassword, PasswordSalt, PasswordChangeDate
            FROM [PasswordHistory]
            WHERE UserId = @UserId
            ORDER BY PasswordChangeDate DESC
            """;

        return await _connection.QueryFirstOrDefaultAsync<PasswordHistory>(
            sql,
            new { UserId = userId },
            _transaction);
    }

    public async Task<IEnumerable<PasswordHistory>> GetLastNByUserIdAsync(string userId, int count, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT TOP (@Count) SerialNo, UserId, HashedPassword, PasswordSalt, PasswordChangeDate
            FROM [PasswordHistory]
            WHERE UserId = @UserId
            ORDER BY PasswordChangeDate DESC
            """;

        return await _connection.QueryAsync<PasswordHistory>(
            sql,
            new { UserId = userId, Count = count },
            _transaction);
    }

    public async Task<int> GetCountByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT COUNT(1) FROM [PasswordHistory] WHERE UserId = @UserId";

        return await _connection.ExecuteScalarAsync<int>(
            sql,
            new { UserId = userId },
            _transaction);
    }

    public async Task AddAsync(PasswordHistory entity, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO [PasswordHistory] (UserId, HashedPassword, PasswordSalt, PasswordChangeDate)
            VALUES (@UserId, @HashedPassword, @PasswordSalt, @PasswordChangeDate)
            """;

        await _connection.ExecuteAsync(
            sql,
            new
            {
                entity.UserId,
                entity.HashedPassword,
                entity.PasswordSalt,
                entity.PasswordChangeDate
            },
            _transaction);
    }
}
