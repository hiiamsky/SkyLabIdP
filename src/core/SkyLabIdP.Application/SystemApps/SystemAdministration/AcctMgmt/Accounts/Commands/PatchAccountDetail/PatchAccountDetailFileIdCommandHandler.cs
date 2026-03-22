using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos;
using SkyLabIdP.Application.Dtos.SkyLabDocUserDetail;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SkyLabIdP.Application.SystemApps.SystemAdministration.AcctMgmt.Accounts.Commands.PatchAccountDetail
{
    public class PatchAccountDetailFileIdCommandHandler : IRequestHandler<PatchAccountDetailFileIdCommand, SkyLabDocUserDetailResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IDataProtectionService _dataprotectionservice;

        private readonly ILogger<PatchAccountDetailFileIdCommandHandler> _logger;

        public PatchAccountDetailFileIdCommandHandler(IUnitOfWork unitOfWork, IDataProtectionService dataprotectionservice , ILogger<PatchAccountDetailFileIdCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;

            _dataprotectionservice = dataprotectionservice;

            _logger = logger;
        }
        public async ValueTask<SkyLabDocUserDetailResponse> Handle(PatchAccountDetailFileIdCommand request, CancellationToken cancellationToken)
        {
            var skylabDocUserDetail = await _unitOfWork.SkyLabDocUserDetails
                .GetByUserIdWithApprovalCheckAsync(request.UserId, requireUnapproved: true, cancellationToken);

            if (skylabDocUserDetail == null)
            {
                return new SkyLabDocUserDetailResponse
                {
                    operationResult = new OperationResult(false, "無使用者資訊或是已經通過審核", StatusCodes.Status404NotFound)
                };
            }
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var decryptedFileId = _dataprotectionservice.Unprotect(request.FileId);
                await _unitOfWork.SkyLabDocUserDetails.UpdateFileIdAsync(
                    skylabDocUserDetail.UserId, decryptedFileId, skylabDocUserDetail.UserId, DateTime.Now, cancellationToken);

                await _unitOfWork.CommitAsync(cancellationToken);
                return new SkyLabDocUserDetailResponse
                {
                    UserId = _dataprotectionservice.Protect(request.UserId),
                    UserName = skylabDocUserDetail.UserName,
                    FullName = skylabDocUserDetail.FullName,
                    OfficialEmail = skylabDocUserDetail.OfficialEmail,
                    BranchCode = skylabDocUserDetail.BranchCode,
                    FileId = _dataprotectionservice.Protect(decryptedFileId),
                    SystemRole = "",
                    SubordinateUnit = skylabDocUserDetail.SubordinateUnit,
                    OfficialPhone = skylabDocUserDetail.OfficialPhone,
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
