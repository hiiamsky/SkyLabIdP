using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos.User.Registration;
using SkyLabIdP.Application.SystemApps.Services.ServiceSettings;
using SkyLabIdP.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace SkyLabIdP.Application.SystemApps.Users.Commands.Writers;

public class SkyLabDocUserDetailWriter : BaseUserDetailWriter<SkyLabMgmUserRegistrationRequest, BaseUserRegistrationResponse, SkyLabDocUserDetail>
{
    public SkyLabDocUserDetailWriter(
        LoginUserInfoServiceSettings loginUserInfoServiceSettings,
        ILogger<SkyLabDocUserDetailWriter> logger)
        : base(loginUserInfoServiceSettings, logger)
    {
    }

    protected override SkyLabDocUserDetail CreateUserDetail(SkyLabMgmUserRegistrationRequest request, ApplicationUser user, string tenantGuid)
    {
        return new SkyLabDocUserDetail
        {
            FileId = request.FileId,
            UserName = user.UserName ?? string.Empty,
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

    protected override async Task AddUserDetailToContextAsync(SkyLabDocUserDetail userDetail, CancellationToken cancellationToken)
    {
        await _unitOfWork.SkyLabDocUserDetails.AddAsync(userDetail, cancellationToken);
    }
}
