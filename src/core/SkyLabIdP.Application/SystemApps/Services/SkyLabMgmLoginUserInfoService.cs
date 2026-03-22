using System;
using SkyLabIdP.Application.Dtos;
using SkyLabIdP.Application.Dtos.LoginUserInfo;
using SkyLabIdP.Application.Dtos.User.Authentication;
using SkyLabIdP.Application.SystemApps.Services.ServiceSettings;
using SkyLabIdP.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace SkyLabIdP.Application.SystemApps.Services;

public class SkyLabMgmLoginUserInfoService(LoginUserInfoServiceSettings loginUserInfoServiceSettings) : AbstractLoginUserInfoService(loginUserInfoServiceSettings)
{
    protected override async Task<LoginUserInfoDto> GetTenantUserInfoAsync(string loginUserId)
    {
        if (string.IsNullOrEmpty(loginUserId))
        {
            return new LoginUserInfoDto();
        }

        return await _unitOfWork.SkyLabDocUserDetails.GetTenantUserInfoAsync(loginUserId);
    }

    protected override async Task<object> GetUserDetailAsync(string userId, CancellationToken cancellationToken)
    {
        return await _unitOfWork.SkyLabDocUserDetails.GetByUserIdAsync(userId, cancellationToken) ?? new SkyLabDocUserDetail();
    }

    protected override async Task UpdateLastLoginTimeAsync(string userId, CancellationToken cancellationToken)
    {
        await _unitOfWork.SkyLabDocUserDetails.UpdateLastLoginTimeAsync(userId, DateTime.Now, cancellationToken);
    }

    public override async Task CreateExternalUserDetailAsync(
            string userId,
            string username,
            string fullName,
            string email,
            string tenantId,
            CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new user detail:{table} for userId: {UserId}, username: {Username}, fullName: {FullName}, email: {Email}","SkyLabDocUserDetail" ,userId, username, fullName, email);
        var newUserDetail = new SkyLabDocUserDetail
        {
            UserId = userId,
            UserName = username,
            FullName = fullName,
            OfficialEmail = email,
            BranchCode = "", // 需要補充
            CreateBy = userId,
            CreateDatetime = DateTime.Now,
            LastUpdatedBy = userId,
            LastUpdateDatetime = DateTime.Now
        };

        await _unitOfWork.SkyLabDocUserDetails.AddAsync(newUserDetail, cancellationToken);

        var isInUserTenant = await _unitOfWork.UserTenants.ExistsAsync(userId, tenantId, cancellationToken);
        if (!isInUserTenant)
        {
            var userTenant = new UserTenant
            {
                UserId = userId,
                TenantId = tenantId,
                CreateDateTime = DateTime.UtcNow
            };
            await _unitOfWork.UserTenants.AddAsync(userTenant, cancellationToken);
        }
    }

    protected override async Task<object> GetUserDetailByTenantAsync(string userId,  CancellationToken cancellationToken)
    {
        // SkyLabMgm 特定邏輯
        return await _unitOfWork.SkyLabDocUserDetails.GetByUserIdAsync(userId, cancellationToken) ?? new SkyLabDocUserDetail();
    }
    
    protected override async Task UpdateUserDetailByTenantAsync(
        object userDetail, 
        ApplicationUser user, 
        ExternalUserRegistrationDto userDetails,
        CancellationToken cancellationToken)
    {
        if (userDetail is SkyLabDocUserDetail detail)
        {
            _logger.LogInformation("更新用戶詳情，用戶ID: {UserId}, 原姓名: {OldName} -> 新姓名: {NewName}",
                user.Id, detail.FullName, userDetails.FullName);

            // 更新資料
            detail.FullName = userDetails.FullName;
            detail.BranchCode = userDetails.BranchCode;
            detail.OfficialPhone = userDetails.OfficialPhone;
            detail.SubordinateUnit = userDetails.SubordinateUnit;
            detail.JobTitle = userDetails.JobTitle;
            detail.LastUpdatedBy = user.Id;
            detail.LastUpdateDatetime = DateTime.Now;

            _logger.LogDebug("更新的詳細資料: 分公司代碼={BranchCode}, 電話={Phone}, 單位={Unit}, 職稱={Title}",
                userDetails.BranchCode, userDetails.OfficialPhone,
                userDetails.SubordinateUnit, userDetails.JobTitle);

            await _unitOfWork.SkyLabDocUserDetails.UpdateAsync(detail, cancellationToken);
        }
    }
}
