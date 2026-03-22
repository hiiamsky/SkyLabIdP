using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos;
using SkyLabIdP.Application.Dtos.SysCode;
using SkyLabIdP.Application.Common.Mappings;

using SkyLabIdP.Domain.Entities;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SkyLabIdP.Application.SystemApps.SystemInfos.SysCodes.Queries
{
    public class SysCodeQueryHandler : IRequestHandler<SysCodeQuery, SysCodeQueryVM>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly SkyLabIdPMapper _mapper;

        private readonly IUserService _userService;

        private readonly ILogger<SysCodeQueryHandler> _logger;

        public SysCodeQueryHandler(IUnitOfWork unitOfWork, SkyLabIdPMapper mapper, ITenantUserServiceFactory tenantUserServiceFactory, ILogger<SysCodeQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userService = tenantUserServiceFactory.GetCurrentTenantService();
            _logger = logger;
        }


        public async ValueTask<SysCodeQueryVM> Handle(SysCodeQuery request, CancellationToken cancellationToken)
        {
            var loginUserInfo = await _userService.GetLoginUserInfoAsync(request.LoginUserId, cancellationToken);

            if (!loginUserInfo.IsUserEligible)
            {
                _logger.LogError("Failed to retrieve login user information.");
                return new SysCodeQueryVM
                {
                    SysCodes = [],
                    operationResult = new OperationResult(false, "無法取得使用者資訊.", StatusCodes.Status401Unauthorized)
                };
            }

            var SysCodes = (await _unitOfWork.SysCodes.QueryAsync(
                request.SysCodeRequestDto.Type,
                request.SysCodeRequestDto.Code,
                cancellationToken)).ToList();


            if (SysCodes == null || SysCodes.Count == 0)
            {
                return new SysCodeQueryVM
                {
                    SysCodes = [],
                    operationResult = new OperationResult(false, "沒有系統設定資料", StatusCodes.Status404NotFound)
                };


            }

            var dtos = _mapper.SysCodeListToDtoList(SysCodes);

            return new SysCodeQueryVM
            {
                SysCodes = dtos,
                operationResult = new OperationResult(true, "取系統設定資料成功", StatusCodes.Status200OK)
            };
        }
    }
}

