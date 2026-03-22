using System.Data;
using Dapper;
using SkyLabIdP.Application.Common.Interfaces.Repositories;
using SkyLabIdP.Application.Dtos.LoginUserInfo;
using SkyLabIdP.Application.Dtos.SkyLabDocUserDetail;
using SkyLabIdP.Application.SystemApps.SystemAdministration.AcctMgmt.Accounts.Queries;
using SkyLabIdP.Domain.Entities;
using SkyLabIdP.Domain.Enums;

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

    public async Task<LoginUserInfoDto> GetTenantUserInfoAsync(string userId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT ud.UserId, au.IsActive, au.IsApproved, au.LockoutEnabled,
                   ud.BranchCode, ud.RegionCode, ud.SystemRole, ud.UserName, ud.OfficialEmail
            FROM [SkyLabDocUserDetail] ud
            JOIN [AspNetUsers] au ON au.Id = ud.UserId
            WHERE ud.UserId = @UserId
            """;

        var row = await _connection.QueryFirstOrDefaultAsync(
            sql,
            new { UserId = userId },
            _transaction);

        if (row == null) return new LoginUserInfoDto();

        return new LoginUserInfoDto
        {
            UserId = row.UserId ?? "",
            IsActive = row.IsActive,
            IsApproved = row.IsApproved,
            LockoutEnabled = row.LockoutEnabled,
            IsUserEligible = (bool)row.IsActive && !(bool)row.LockoutEnabled,
            BranchCode = row.BranchCode ?? "",
            RegionCode = row.RegionCode ?? "",
            SystemRole = row.SystemRole ?? "",
            UserName = row.UserName ?? "",
            OfficialEmail = row.OfficialEmail ?? ""
        };
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
        AccountQuery request, CancellationToken cancellationToken = default)
    {
        var whereClauses = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(request.FullName))
        {
            whereClauses.Add("ud.FullName LIKE '%' + @FullName + '%'");
            parameters.Add("FullName", request.FullName);
        }
        if (!string.IsNullOrEmpty(request.UserName))
        {
            whereClauses.Add("ud.UserName = @UserName");
            parameters.Add("UserName", request.UserName);
        }
        if (!string.IsNullOrEmpty(request.OfficialEmail))
        {
            whereClauses.Add("ud.OfficialEmail = @OfficialEmail");
            parameters.Add("OfficialEmail", request.OfficialEmail);
        }
        if (!string.IsNullOrEmpty(request.BranchCode))
        {
            whereClauses.Add("ud.BranchCode = @BranchCode");
            parameters.Add("BranchCode", request.BranchCode);
        }
        if (!string.IsNullOrEmpty(request.UserId))
        {
            whereClauses.Add("au.Id = @UserId");
            parameters.Add("UserId", request.UserId);
        }

        // Status filter based on UserInfo enum
        switch (request.Status)
        {
            case (int)UserInfo.StatusUnApprove:
                whereClauses.Add("au.IsApproved = 0");
                break;
            case (int)UserInfo.StatusIsActive:
                whereClauses.Add("au.IsActive = 1");
                break;
            case (int)UserInfo.StatusLockoutEnabled:
                whereClauses.Add("au.LockoutEnabled = 1");
                break;
            case (int)UserInfo.StatusUnActive:
                whereClauses.Add("au.IsActive = 0 AND au.IsApproved = 1");
                break;
            case (int)UserInfo.StatusIsApproved:
                whereClauses.Add("au.IsApproved = 1");
                break;
        }

        var whereClause = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";

        var countSql = $"""
            SELECT COUNT(1)
            FROM [SkyLabDocUserDetail] ud
            JOIN [AspNetUsers] au ON au.Id = ud.UserId
            JOIN [Branch] b ON b.BranchCode = ud.BranchCode
            JOIN [FileUpload] fu ON fu.FileId = ud.FileId
            {whereClause}
            """;

        var dataSql = $"""
            SELECT ud.UserId, ud.SystemRole, ud.FileId, fu.OriginalFileName, fu.FileExtension,
                   ud.UserName, ud.FullName, ud.BranchCode, b.BranchName,
                   ud.SubordinateUnit, ud.JobTitle, ud.OfficialEmail, ud.OfficialPhone,
                   au.IsApproved, au.LockoutEnabled, au.IsActive, ud.MoicaCardNumber
            FROM [SkyLabDocUserDetail] ud
            JOIN [AspNetUsers] au ON au.Id = ud.UserId
            JOIN [Branch] b ON b.BranchCode = ud.BranchCode
            JOIN [FileUpload] fu ON fu.FileId = ud.FileId
            {whereClause}
            ORDER BY ud.BranchCode, ud.UserName
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

    public async Task<SkyLabDocUserDetail?> GetByUserIdWithApprovalCheckAsync(string userId, bool requireUnapproved, CancellationToken cancellationToken = default)
    {
        var approvalCondition = requireUnapproved ? "AND au.IsApproved = 0" : "";
        var sql = $"""
            SELECT ud.SerialNo, ud.UserId, ud.SystemRole, ud.FileId, ud.UserName, ud.FullName,
                   ud.BranchCode, ud.RegionCode, ud.DepartmentName, ud.SubordinateUnit, ud.JobTitle,
                   ud.OfficialEmail, ud.OfficialPhone, ud.LastLoginDatetime, ud.CreateBy, ud.CreateDatetime,
                   ud.LastUpdatedBy, ud.LastUpdateDatetime, ud.MoicaCardNumber, ud.UserTenantGuid
            FROM [SkyLabDocUserDetail] ud
            JOIN [AspNetUsers] au ON au.Id = ud.UserId
            WHERE ud.UserId = @UserId {approvalCondition}
            """;

        return await _connection.QueryFirstOrDefaultAsync<SkyLabDocUserDetail>(
            sql,
            new { UserId = userId },
            _transaction);
    }

    public async Task<SkyLabDocUserDetail?> GetByUserIdWithActiveApprovedCheckAsync(string userId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT ud.SerialNo, ud.UserId, ud.SystemRole, ud.FileId, ud.UserName, ud.FullName,
                   ud.BranchCode, ud.RegionCode, ud.DepartmentName, ud.SubordinateUnit, ud.JobTitle,
                   ud.OfficialEmail, ud.OfficialPhone, ud.LastLoginDatetime, ud.CreateBy, ud.CreateDatetime,
                   ud.LastUpdatedBy, ud.LastUpdateDatetime, ud.MoicaCardNumber, ud.UserTenantGuid
            FROM [SkyLabDocUserDetail] ud
            JOIN [AspNetUsers] au ON au.Id = ud.UserId
            WHERE ud.UserId = @UserId AND au.IsApproved = 1 AND au.IsActive = 1 AND au.LockoutEnabled = 0
            """;

        return await _connection.QueryFirstOrDefaultAsync<SkyLabDocUserDetail>(
            sql,
            new { UserId = userId },
            _transaction);
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
