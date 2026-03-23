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

                // 分別查詢 FunctionGroup 和 Function
                var functionGroups = (await _unitOfWork.FunctionGroups
                    .GetFilteredAsync(request.GroupID, cancellationToken))
                    .ToList();

                var groupIds = functionGroups.Select(g => g.GroupID);
                var functions = (await _unitOfWork.Functions.GetByGroupIdsAsync(groupIds, cancellationToken)).ToList();

                // 若有指定 FunctionID，過濾 functions
                if (!string.IsNullOrEmpty(request.FunctionID))
                {
                    functions = functions.Where(f => f.FunctionID == request.FunctionID).ToList();
                }

                // 只保留 IsDisplayInMenu 的 functions，並按 GroupID 分組
                var functionsByGroup = functions
                    .Where(f => f.IsDisplayInMenu)
                    .GroupBy(f => f.GroupID)
                    .ToDictionary(g => g.Key, g => g.OrderBy(f => f.FunctionOrder).ToList());

                // 組裝 Functions 到對應的 FunctionGroup
                foreach (var group in functionGroups)
                {
                    group.Functions = functionsByGroup.TryGetValue(group.GroupID, out var fns)
                        ? fns
                        : [];
                }

                // 若有指定 FunctionID，只回傳包含該 Function 的 Groups
                if (!string.IsNullOrEmpty(request.FunctionID))
                {
                    functionGroups = functionGroups.Where(g => g.Functions.Count > 0).ToList();
                }

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

