using SkyLabIdP.Application.Dtos.LoginUserInfo;
using SkyLabIdP.Application.Dtos.SkyLabDocUserDetail;
using SkyLabIdP.Application.SystemApps.SystemAdministration.AcctMgmt.Accounts.Queries;
using SkyLabIdP.Domain.Entities;

namespace SkyLabIdP.Application.Common.Interfaces.Repositories;

public interface ISkyLabDocUserDetailRepository
{
    Task<SkyLabDocUserDetail?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<LoginUserInfoDto> GetTenantUserInfoAsync(string userId, CancellationToken cancellationToken = default);
    Task UpdateLastLoginTimeAsync(string userId, DateTime loginTime, CancellationToken cancellationToken = default);
    Task AddAsync(SkyLabDocUserDetail entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(SkyLabDocUserDetail entity, CancellationToken cancellationToken = default);
    Task<(IEnumerable<SkyLabDocUserDetailDto> Items, int TotalCount)> GetAccountQueryAsync(AccountQuery request, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<bool> ExistsByOfficialEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByFileIdAsync(string fileId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByOfficialEmailExcludingUserAsync(string email, string excludeUserId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUserIdAndFileIdAsync(string userId, string fileId, CancellationToken cancellationToken = default);
    Task<SkyLabDocUserDetail?> GetByUserIdWithApprovalCheckAsync(string userId, bool requireUnapproved, CancellationToken cancellationToken = default);
    Task<SkyLabDocUserDetail?> GetByUserIdWithActiveApprovedCheckAsync(string userId, CancellationToken cancellationToken = default);
    Task UpdateFileIdAsync(string userId, string fileId, string lastUpdatedBy, DateTime lastUpdateDatetime, CancellationToken cancellationToken = default);
    Task UpdateOfficialPhoneAsync(string userId, string officialPhone, string lastUpdatedBy, DateTime lastUpdateDatetime, CancellationToken cancellationToken = default);
}
