using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos;
using SkyLabIdP.Application.Dtos.SkyLabDocUserDetail;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SkyLabIdP.Application.SystemApps.SystemAdministration.AcctMgmt.Accounts.Commands.PatchAccountDetail
{

    public class PatchAccountDetailOfficialPhoneCommandHandler : IRequestHandler<PatchAccountDetailOfficialPhoneCommand, SkyLabDocUserDetailResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IDataProtectionService _dataprotectionservice ;

        private readonly ILogger<PatchAccountDetailOfficialPhoneCommandHandler> _logger;

        public PatchAccountDetailOfficialPhoneCommandHandler(IUnitOfWork unitOfWork, IDataProtectionService dataprotectionservice , ILogger<PatchAccountDetailOfficialPhoneCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;

            _dataprotectionservice  = dataprotectionservice;

            _logger = logger;
        }

        public async ValueTask<SkyLabDocUserDetailResponse> Handle(PatchAccountDetailOfficialPhoneCommand request, CancellationToken cancellationToken)
        {
            var skylabDocUserDetail = await _unitOfWork.SkyLabDocUserDetails
                            .GetByUserIdWithActiveApprovedCheckAsync(request.UserId, cancellationToken);

            if (skylabDocUserDetail == null)
            {
                return new SkyLabDocUserDetailResponse
                {
                    operationResult = new OperationResult(false, "無使用者資訊，請確認是否未通過審核或停用中或鎖定中", StatusCodes.Status404NotFound)
                };
            }
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                await _unitOfWork.SkyLabDocUserDetails.UpdateOfficialPhoneAsync(
                    skylabDocUserDetail.UserId, request.OfficialPhone, skylabDocUserDetail.UserId, DateTime.Now, cancellationToken);

                await _unitOfWork.CommitAsync(cancellationToken);
                return new SkyLabDocUserDetailResponse
                {
                    UserId = _dataprotectionservice .Protect(skylabDocUserDetail.UserId),
                    UserName = skylabDocUserDetail.UserName,
                    FullName = skylabDocUserDetail.FullName,
                    OfficialEmail = skylabDocUserDetail.OfficialEmail,
                    BranchCode = skylabDocUserDetail.BranchCode,
                    FileId = _dataprotectionservice .Protect(skylabDocUserDetail.FileId),
                    SystemRole = "",
                    SubordinateUnit = skylabDocUserDetail.SubordinateUnit,
                    OfficialPhone = request.OfficialPhone,
                    JobTitle = skylabDocUserDetail.JobTitle,
                    operationResult = new OperationResult(true, "SkyLabDocUserDetail updated successfully.", StatusCodes.Status200OK)
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "An error occurred while updating SkyLabDocUserDetail.");
                throw;
            }
        }
    }
}
