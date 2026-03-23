using System.Data;
using Dapper;
using SkyLabIdP.Application.Common.Interfaces.Repositories;
using SkyLabIdP.Domain.Entities;

namespace SkyLabIdP.Data.Repositories;

public class SkyLabDevelopUserDetailRepository : ISkyLabDevelopUserDetailRepository
{
    private readonly IDbConnection _connection;
    private readonly IDbTransaction? _transaction;

    public SkyLabDevelopUserDetailRepository(IDbConnection connection, IDbTransaction? transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<SkyLabDevelopUserDetail?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT SerialNo, UserId, FullName, BranchCode, RegionCode, DepartmentName,
                   SubordinateUnit, JobTitle, OfficialEmail, OfficialPhone,
                   CreateBy, CreateDatetime, LastLoginDatetime, LastUpdateDatetime,
                   LastUpdatedBy, UserTenantGuid
            FROM [SkyLabDevelopUserDetail]
            WHERE UserId = @UserId
            """;

        return await _connection.QueryFirstOrDefaultAsync<SkyLabDevelopUserDetail>(
            sql,
            new { UserId = userId },
            _transaction);
    }

    public async Task UpdateLastLoginTimeAsync(string userId, DateTime loginTime, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE [SkyLabDevelopUserDetail]
            SET LastLoginDatetime = @LoginTime
            WHERE UserId = @UserId
            """;

        await _connection.ExecuteAsync(
            sql,
            new { UserId = userId, LoginTime = loginTime },
            _transaction);
    }

    public async Task AddAsync(SkyLabDevelopUserDetail entity, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO [SkyLabDevelopUserDetail]
                (UserId, FullName, BranchCode, RegionCode, DepartmentName, SubordinateUnit,
                 JobTitle, OfficialEmail, OfficialPhone, CreateBy, CreateDatetime,
                 LastLoginDatetime, LastUpdateDatetime, LastUpdatedBy, UserTenantGuid)
            VALUES
                (@UserId, @FullName, @BranchCode, @RegionCode, @DepartmentName, @SubordinateUnit,
                 @JobTitle, @OfficialEmail, @OfficialPhone, @CreateBy, @CreateDatetime,
                 @LastLoginDatetime, @LastUpdateDatetime, @LastUpdatedBy, @UserTenantGuid)
            """;

        await _connection.ExecuteAsync(sql, new
        {
            entity.UserId,
            entity.FullName,
            entity.BranchCode,
            entity.RegionCode,
            entity.DepartmentName,
            entity.SubordinateUnit,
            entity.JobTitle,
            entity.OfficialEmail,
            entity.OfficialPhone,
            entity.CreateBy,
            entity.CreateDatetime,
            entity.LastLoginDatetime,
            entity.LastUpdateDatetime,
            entity.LastUpdatedBy,
            entity.UserTenantGuid
        }, _transaction);
    }

    public async Task UpdateAsync(SkyLabDevelopUserDetail entity, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE [SkyLabDevelopUserDetail]
            SET FullName = @FullName, BranchCode = @BranchCode, RegionCode = @RegionCode,
                SubordinateUnit = @SubordinateUnit, JobTitle = @JobTitle,
                OfficialEmail = @OfficialEmail, OfficialPhone = @OfficialPhone,
                LastUpdatedBy = @LastUpdatedBy, LastUpdateDatetime = @LastUpdateDatetime
            WHERE UserId = @UserId
            """;

        await _connection.ExecuteAsync(sql, new
        {
            entity.FullName,
            entity.BranchCode,
            entity.RegionCode,
            entity.SubordinateUnit,
            entity.JobTitle,
            entity.OfficialEmail,
            entity.OfficialPhone,
            entity.LastUpdatedBy,
            entity.LastUpdateDatetime,
            entity.UserId
        }, _transaction);
    }

    public async Task<bool> ExistsByOfficialEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT CASE WHEN EXISTS (SELECT 1 FROM [SkyLabDevelopUserDetail] WHERE OfficialEmail = @Email) THEN 1 ELSE 0 END";
        return await _connection.ExecuteScalarAsync<bool>(sql, new { Email = email }, _transaction);
    }
}
