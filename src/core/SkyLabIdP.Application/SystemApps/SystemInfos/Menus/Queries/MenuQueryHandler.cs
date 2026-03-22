using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos;
using SkyLabIdP.Application.Dtos.FunctionGroup;
using SkyLabIdP.Domain.Entities;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SkyLabIdP.Application.SystemApps.SystemInfos.Menus.Queries
{
    public class MenuQueryHanndler : IRequestHandler<MenuQuery, MenuVM>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly SkyLabIdPMapper _mapper;

        private readonly IUserService _userService;

        private readonly ILogger<MenuQueryHanndler> _logger;

        public MenuQueryHanndler(IUnitOfWork unitOfWork, SkyLabIdPMapper mapper, ITenantUserServiceFactory tenantUserServiceFactory, ILogger<MenuQueryHanndler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userService = tenantUserServiceFactory.GetCurrentTenantService();
            _logger = logger;
        }
        public async ValueTask<MenuVM> Handle(MenuQuery request, CancellationToken cancellationToken)
        {
            var loginUserInfo = await _userService.GetLoginUserInfoAsync(request.LoginUserId, cancellationToken);
                if (!loginUserInfo.IsUserEligible)
                {
                    _logger.LogError("Failed to retrieve login user information.");
                    return new MenuVM
                    {
                        FunctionGroups = [],
                        OperationResult = new OperationResult(false, "無法取得使用者資訊.", StatusCodes.Status401Unauthorized)
                    };
                }

                var functionGroups = (await _unitOfWork.FunctionGroups
                    .GetFilteredWithFunctionsAsync(request.GroupID, request.FunctionID, cancellationToken))
                    .ToList();

                // Map to DTOs
                var functionGroupDtos = _mapper.FunctionGroupListToDtoList(functionGroups);

                return new MenuVM
                {
                    FunctionGroups = functionGroupDtos,
                    OperationResult = new OperationResult(true, "成功取得Menu.", 200)
                };
        }
    }
}

