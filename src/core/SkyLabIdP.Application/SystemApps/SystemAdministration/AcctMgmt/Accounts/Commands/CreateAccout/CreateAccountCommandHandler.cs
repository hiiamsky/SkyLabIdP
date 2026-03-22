
using SkyLabIdP.Application.Common.Interfaces;

using SkyLabIdP.Application.Dtos.SkyLabDocUserDetail;



using Mediator;

namespace SkyLabIdP.Application.SystemApps.SystemAdministration.AcctMgmt.Accounts.Commands.CreateAccout
{
    public class CreateAccoutCommandHandler : IRequestHandler<CreateAccoutCommand, SkyLabDocUserDetailResponse>
    {
        private readonly IUserService _userService;

        public CreateAccoutCommandHandler(ITenantUserServiceFactory tenantUserServiceFactory)
        {
            _userService = tenantUserServiceFactory.GetCurrentTenantService();
            }

        public async ValueTask<SkyLabDocUserDetailResponse> Handle(CreateAccoutCommand request, CancellationToken cancellationToken)
        {
            return await _userService.HandleCreateAccoutCommandAsync(request, cancellationToken);
        }
    }
}


