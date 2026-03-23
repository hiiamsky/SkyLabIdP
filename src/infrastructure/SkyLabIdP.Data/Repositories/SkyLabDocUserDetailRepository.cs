using System.Data;
using Dapper;
using SkyLabIdP.Application.Common.Interfaces.Repositories;
using SkyLabIdP.Application.Dtos.SkyLabDocUserDetail;
using SkyLabIdP.Application.SystemApps.SystemAdministration.AcctMgmt.Accounts.Queries;
using SkyLabIdP.Domain.Entities;

namespace SkyLabIdP.Data.Repositories;

public class SkyLabDocUserDetailRepository : ISkyLabDocUserDetailRepository
{
    private readonly IDbConnection _connection;
    private readonly IDbTransaction? _transaction;

    public SkyLabDocUserDetailRepository(IDbConnection connection, IDbTransaction? transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<SkyLabDocUserDetail?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT SerialNo, UserId, SystemRole, FileId, UserName, FullName,
                   BranchCode, RegionCode, DepartmentName, SubordinateUnit, JobTitle,
                   OfficialEmail, OfficialPhone, LastLoginDatetime, CreateBy, CreateDatetime,
                   LastUpdatedBy, LastUpdateDatetime, MoicaCardNumber, UserTenantGuid
            FROM [SkyLabDocUserDetail]
            WHERE UserId = @UserId
            """;

        return await _connection.QueryFirstOrDefaultAsync<SkyLabDocUserDetail>(
            sql,
            new { UserId = userId },
            _transaction);
    }

    public async Task UpdateLastLoginTimeAsync(string userId, DateTime loginTime, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE [SkyLabDocUserDetail]
            SET LastLoginDatetime = @LoginTime
            WHERE UserId = @UserId
            """;

        await _connection.ExecuteAsync(
            sql,
            new { UserId = userId, LoginTime = loginTime },
            _transaction);
    }

    public async Task AddAsync(SkyLabDocUserDetail entity, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO [SkyLabDocUserDetail]
                (UserId, SystemRole, FileId, UserName, FullName, BranchCode, RegionCode,
                 DepartmentName, SubordinateUnit, JobTitle, OfficialEmail, OfficialPhone,
                 CreateBy, CreateDatetime, LastUpdatedBy, LastUpdateDatetime,
                 MoicaCardNumber, UserTenantGuid)
            VALUES
                (@UserId, @SystemRole, @FileId, @UserName, @FullName, @BranchCode, @RegionCode,
                 @DepartmentName, @SubordinateUnit, @JobTitle, @OfficialEmail, @OfficialPhone,
                 @CreateBy, @CreateDatetime, @LastUpdatedBy, @LastUpdateDatetime,
                 @MoicaCardNumber, @UserTenantGuid)
            """;

        await _connection.ExecuteAsync(sql, new
        {
            entity.UserId,
            entity.SystemRole,
            entity.FileId,
            entity.UserName,
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
            entity.LastUpdatedBy,
            entity.LastUpdateDatetime,
            entity.MoicaCardNumber,
            entity.UserTenantGuid
        }, _transaction);
    }

    public async Task UpdateAsync(SkyLabDocUserDetail entity, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE [SkyLabDocUserDetail]
            SET FullName = @FullName, BranchCode = @BranchCode, RegionCode = @RegionCode,
                DepartmentName = @DepartmentName, SubordinateUnit = @SubordinateUnit,
                JobTitle = @JobTitle, OfficialEmail = @OfficialEmail, OfficialPhone = @OfficialPhone,
                LastUpdatedBy = @LastUpdatedBy, LastUpdateDatetime = @LastUpdateDatetime,
                MoicaCardNumber = @MoicaCardNumber, SystemRole = @SystemRole
            WHERE UserId = @UserId
            """;

        await _connection.ExecuteAsync(sql, new
        {
            entity.FullName,
            entity.BranchCode,
            entity.RegionCode,
            entity.DepartmentName,
            entity.SubordinateUnit,
            entity.JobTitle,
            entity.OfficialEmail,
            entity.OfficialPhone,
            entity.LastUpdatedBy,
            entity.LastUpdateDatetime,
            entity.MoicaCardNumber,
            entity.SystemRole,
            entity.UserId
        }, _transaction);
    }

    public async Task<(IEnumerable<SkyLabDocUserDetailDto> Items, int TotalCount)> GetAccountQueryAsync(
        AccountQuery request, IEnumerable<string>? userIds = null, CancellationToken cancellationToken = default)
    {
        var whereClauses = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(request.FullName))
        {
            whereClauses.Add("FullName LIKE '%' + @FullName + '%'");
            parameters.Add("FullName", request.FullName);
        }
        if (!string.IsNullOrEmpty(request.UserName))
        {
            whereClauses.Add("UserName = @UserName");
            parameters.Add("UserName", request.UserName);
        }
        if (!string.IsNullOrEmpty(request.OfficialEmail))
        {
            whereClauses.Add("OfficialEmail = @OfficialEmail");
            parameters.Add("OfficialEmail", request.OfficialEmail);
        }
        if (!string.IsNullOrEmpty(request.BranchCode))
        {
            whereClauses.Add("BranchCode = @BranchCode");
            parameters.Add("BranchCode", request.BranchCode);
        }
        if (!string.IsNullOrEmpty(request.UserId))
        {
            whereClauses.Add("UserId = @UserId");
            parameters.Add("UserId", request.UserId);
        }
        if (userIds != null)
        {
            whereClauses.Add("UserId IN @UserIds");
            parameters.Add("UserIds", userIds);
        }

        var whereClause = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";

        var countSql = $"""
            SELECT COUNT(1)
            FROM [SkyLabDocUserDetail]
            {whereClause}
            """;

        var dataSql = $"""
            SELECT UserId, SystemRole, FileId, UserName, FullName, BranchCode,
                   SubordinateUnit, JobTitle, OfficialEmail, OfficialPhone, MoicaCardNumber
            FROM [SkyLabDocUserDetail]
            {whereClause}
            ORDER BY BranchCode, UserName
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        parameters.Add("Offset", (request.PageNumber - 1) * request.PageSize);
        parameters.Add("PageSize", request.PageSize);

        var totalCount = await _connection.ExecuteScalarAsync<int>(countSql, parameters, _transaction);
        var items = await _connection.QueryAsync<SkyLabDocUserDetailDto>(dataSql, parameters, _transaction);

        return (items, totalCount);
    }

    public async Task<bool> ExistsByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT CASE WHEN EXISTS (SELECT 1 FROM [SkyLabDocUserDetail] WHERE UserName = @UserName) THEN 1 ELSE 0 END";
        return await _connection.ExecuteScalarAsync<bool>(sql, new { UserName = userName }, _transaction);
    }

    public async Task<bool> ExistsByOfficialEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT CASE WHEN EXISTS (SELECT 1 FROM [SkyLabDocUserDetail] WHERE OfficialEmail = @Email) THEN 1 ELSE 0 END";
        return await _connection.ExecuteScalarAsync<bool>(sql, new { Email = email }, _transaction);
    }

    public async Task<bool> ExistsByFileIdAsync(string fileId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT CASE WHEN EXISTS (SELECT 1 FROM [SkyLabDocUserDetail] WHERE FileId = @FileId) THEN 1 ELSE 0 END";
        return await _connection.ExecuteScalarAsync<bool>(sql, new { FileId = fileId }, _transaction);
    }

    public async Task<bool> ExistsByOfficialEmailExcludingUserAsync(string email, string excludeUserId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT CASE WHEN EXISTS (SELECT 1 FROM [SkyLabDocUserDetail] WHERE OfficialEmail = @Email AND UserId <> @ExcludeUserId) THEN 1 ELSE 0 END";
        return await _connection.ExecuteScalarAsync<bool>(sql, new { Email = email, ExcludeUserId = excludeUserId }, _transaction);
    }

    public async Task<bool> ExistsByUserIdAndFileIdAsync(string userId, string fileId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT CASE WHEN EXISTS (SELECT 1 FROM [SkyLabDocUserDetail] WHERE UserId = @UserId AND FileId = @FileId) THEN 1 ELSE 0 END";
        return await _connection.ExecuteScalarAsync<bool>(sql, new { UserId = userId, FileId = fileId }, _transaction);
    }

    public async Task UpdateFileIdAsync(string userId, string fileId, string lastUpdatedBy, DateTime lastUpdateDatetime, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE [SkyLabDocUserDetail]
            SET FileId = @FileId, LastUpdatedBy = @LastUpdatedBy, LastUpdateDatetime = @LastUpdateDatetime
            WHERE UserId = @UserId
            """;

        await _connection.ExecuteAsync(sql, new { UserId = userId, FileId = fileId, LastUpdatedBy = lastUpdatedBy, LastUpdateDatetime = lastUpdateDatetime }, _transaction);
    }

    public async Task UpdateOfficialPhoneAsync(string userId, string officialPhone, string lastUpdatedBy, DateTime lastUpdateDatetime, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE [SkyLabDocUserDetail]
            SET OfficialPhone = @OfficialPhone, LastUpdatedBy = @LastUpdatedBy, LastUpdateDatetime = @LastUpdateDatetime
            WHERE UserId = @UserId
            """;

        await _connection.ExecuteAsync(sql, new { UserId = userId, OfficialPhone = officialPhone, LastUpdatedBy = lastUpdatedBy, LastUpdateDatetime = lastUpdateDatetime }, _transaction);
    }
}
