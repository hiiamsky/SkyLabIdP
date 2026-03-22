using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;


namespace SkyLabIdP.Application.SystemApps.Users.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, OperationResult>
    {
        private readonly ITenantUserServiceFactory _tenantUserServiceFactory;
        private readonly ILogger<ResetPasswordCommandHandler> _logger;

        public ResetPasswordCommandHandler(ITenantUserServiceFactory tenantUserServiceFactory, ILogger<ResetPasswordCommandHandler> logger)
        {
            _tenantUserServiceFactory = tenantUserServiceFactory ?? throw new ArgumentNullException(nameof(tenantUserServiceFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async ValueTask<OperationResult> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var userService = _tenantUserServiceFactory.GetCurrentTenantService();
            return await userService.HandleResetPasswordAsync(request, cancellationToken);
        }
    }
}