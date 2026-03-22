using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;


namespace SkyLabIdP.Application.SystemApps.Users.Commands.ChangePassWord
{
    public class ChangePassWordCommandHandler : IRequestHandler<ChangePassWordCommand, OperationResult>
    {
        private readonly ITenantUserServiceFactory _tenantUserServiceFactory;
        private readonly ILogger<ChangePassWordCommandHandler> _logger;

        public ChangePassWordCommandHandler(ITenantUserServiceFactory tenantUserServiceFactory, ILogger<ChangePassWordCommandHandler> logger)
        {
            _tenantUserServiceFactory = tenantUserServiceFactory ?? throw new ArgumentNullException(nameof(tenantUserServiceFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async ValueTask<OperationResult> Handle(ChangePassWordCommand request, CancellationToken cancellationToken)
        {
            var userService = _tenantUserServiceFactory.GetCurrentTenantService();
            return await userService.HandleChangePassWordCommandAsync(request, cancellationToken);
        }
    }
}