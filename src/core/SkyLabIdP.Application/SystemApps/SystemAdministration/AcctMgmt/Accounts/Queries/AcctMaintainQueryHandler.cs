using SkyLabIdP.Application.Common.Interfaces;

using Mediator;


namespace SkyLabIdP.Application.SystemApps.SystemAdministration.AcctMgmt.Accounts.Queries
{
    public class AcctMaintainQueryHandler : IRequestHandler<AcctMaintainQuery, AcctMaintainQueryVM>
    {
        private readonly IUserService _userService;

        public AcctMaintainQueryHandler(ITenantUserServiceFactory tenantUserServiceFactory)
        {
            _userService = tenantUserServiceFactory.GetCurrentTenantService();
            }
        public async ValueTask<AcctMaintainQueryVM> Handle(AcctMaintainQuery request, CancellationToken cancellationToken)
        {
            return await _userService.HandleAcctMaintainQueryAsync(request, cancellationToken);
        }
        
    }

}


