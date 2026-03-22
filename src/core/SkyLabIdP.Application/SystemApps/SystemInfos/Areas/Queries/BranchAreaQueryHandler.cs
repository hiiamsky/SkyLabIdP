using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos;
using SkyLabIdP.Application.Dtos.BranchArea;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SkyLabIdP.Application.SystemApps.SystemInfos.Areas.Queries
{
    public class BranchAreaQueryHandler : IRequestHandler<BranchAreaQuery, BranchAreaQueryVM>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly SkyLabIdPMapper _mapper;
        private readonly ILogger<BranchAreaQueryHandler> _logger;
        private readonly IUserService _userService;

        public BranchAreaQueryHandler(IUnitOfWork unitOfWork, SkyLabIdPMapper mapper, ILogger<BranchAreaQueryHandler> logger, ITenantUserServiceFactory tenantUserServiceFactory)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _userService = tenantUserServiceFactory.GetCurrentTenantService();
            }

        public async ValueTask<BranchAreaQueryVM> Handle(BranchAreaQuery request, CancellationToken cancellationToken)
        {
            var loginUserInfo = await _userService.GetLoginUserInfoAsync(request.LoginUserId, cancellationToken);
                if (!loginUserInfo.IsUserEligible)
                {
                    _logger.LogError("Failed to retrieve login user information.");
                    return new BranchAreaQueryVM
                    {
                        BranchAreas = [],
                        OperationResult = new OperationResult(false, "無法取得使用者資訊.", StatusCodes.Status401Unauthorized)
                    };
                }

                var areas = (await _unitOfWork.BranchAreas.QueryAsync(
                    request.AreaID,
                    request.AreaName,
                    request.DstCode,
                    request.CityCode,
                    cancellationToken)).ToList();

                var areaDtos = _mapper.BranchAreaListToDtoList(areas);

                return new BranchAreaQueryVM
                {
                    BranchAreas = areaDtos,
                    OperationResult = new OperationResult(true, "查詢成功")
                };
        }
    }
}
