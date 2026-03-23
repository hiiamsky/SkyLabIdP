using System;
using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos.User.Registration;
using SkyLabIdP.Application.SystemApps.Services.ServiceSettings;
using SkyLabIdP.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace SkyLabIdP.Application.SystemApps.Users.Commands.Writers;

public class SkyLabDevelopUserDetailWriter : 
    BaseUserDetailWriter<SkyLabDevelopUserRegistrationRequest, SkyLabDevelopUserRegistrationResponse, SkyLabDevelopUserDetail>,
    IUserDetailWriter<SkyLabDevelopUserRegistrationRequest, SkyLabDevelopUserRegistrationResponse>
{
    public SkyLabDevelopUserDetailWriter(
        LoginUserInfoServiceSettings loginUserInfoServiceSettings,
        ILogger<SkyLabDevelopUserDetailWriter> logger)
        : base(loginUserInfoServiceSettings, logger)
    {
    }

    protected override SkyLabDevelopUserDetail CreateUserDetail(SkyLabDevelopUserRegistrationRequest request, ApplicationUser user, string tenantGuid)
    {
        return new SkyLabDevelopUserDetail
        {
            FullName = request.FullName,
            BranchCode = request.BranchCode,
            SubordinateUnit = request.SubordinateUnit,
            JobTitle = request.JobTitle,
            OfficialEmail = user.Email ?? string.Empty,
            OfficialPhone = request.OfficialPhone,
            UserId = user.Id,
            CreateBy = user.Id,
            LastUpdatedBy = user.Id,
            CreateDatetime = DateTime.UtcNow,
            LastUpdateDatetime = DateTime.UtcNow,
            UserTenantGuid = tenantGuid,
        };
    }

    protected override async Task AddUserDetailToContextAsync(SkyLabDevelopUserDetail userDetail, CancellationToken cancellationToken)
    {
        await _unitOfWork.SkyLabDevelopUserDetails.AddAsync(userDetail, cancellationToken);
    }

    public override async Task<SkyLabDevelopUserRegistrationResponse> WriteAsync(SkyLabDevelopUserRegistrationRequest request, CancellationToken cancellationToken)
    {
        // 將 SkyLabDevelopUserRegistrationRequest 轉換為 UserRegistrationRequest
        var newSkyLabDevelopUserRegistrationRequest = new SkyLabDevelopUserRegistrationRequest
        {
            Email = request.Email,
            FullName = request.FullName,
            BranchCode = request.BranchCode,
            TenantId = request.TenantId
        };

        // 調用基礎類別的 WriteAsync 方法
        var baseResponse = await base.WriteAsync(newSkyLabDevelopUserRegistrationRequest, cancellationToken);

        // 將 UserRegistrationResponse 轉換為 SkyLabDevelopUserRegistrationResponse
        return new SkyLabDevelopUserRegistrationResponse
        {
            UserId = baseResponse.UserId,
            Email = baseResponse.Email,
            UserName = baseResponse.UserName,
            FirstName = baseResponse.FirstName,
            LastName = baseResponse.LastName,
            operationResult = baseResponse.operationResult
        };
    }
}
