using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos;
using SkyLabIdP.Application.Dtos.SkyLabDocUserDetail;
using SkyLabIdP.Domain.Enums;
using Mediator;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SkyLabIdP.Application.SystemApps.SystemAdministration.AcctMgmt.Accounts.Queries
{
    public class AccountQueryHandler : IRequestHandler<AccountQuery, AccountQueryVM>
    {
        private readonly IUserService _userService;
        public AccountQueryHandler(ITenantUserServiceFactory tenantUserServiceFactory)
        {
            _userService = tenantUserServiceFactory.GetCurrentTenantService();
            }
        public async ValueTask<AccountQueryVM> Handle(AccountQuery request, CancellationToken cancellationToken)
        {
            return await _userService.HandleAccountQueryAsync(request, cancellationToken);

        }


    }
}

