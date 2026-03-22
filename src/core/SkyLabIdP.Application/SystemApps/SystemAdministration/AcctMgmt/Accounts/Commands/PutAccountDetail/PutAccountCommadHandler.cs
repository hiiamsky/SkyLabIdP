using SkyLabIdP.Application.Common.Interfaces;

using SkyLabIdP.Application.SystemApps.SystemAdministration.AcctMgmt.Accounts.Queries;

using Mediator;


namespace SkyLabIdP.Application.SystemApps.SystemAdministration.AcctMgmt.Accounts.Commands.PutAccountDetail
{

    public class PutAccountCommadHandler : IRequestHandler<PutAccountCommad, AcctMaintainQueryVM>
    {
        private readonly IUserService _userService;
  
        public PutAccountCommadHandler(ITenantUserServiceFactory tenantUserServiceFactory)
        {
            _userService = tenantUserServiceFactory.GetCurrentTenantService();
            }
        /// <summary>
        /// 更新使用者資料
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async ValueTask<AcctMaintainQueryVM> Handle(PutAccountCommad request, CancellationToken cancellationToken)
        {
            return await _userService.HandlePutAccountCommandAsync(request, cancellationToken);
        }
    }
}


