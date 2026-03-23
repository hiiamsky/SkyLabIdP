﻿using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos.User.Authentication;
using SkyLabIdP.Application.SystemApps.Users.Commands.LoginUser.Services;
using Mediator;
using System.Threading;
using System.Threading.Tasks;

namespace SkyLabIdP.Application.SystemApps.Users.Commands.LoginUser
{
    /// <summary>
    /// 使用者登入命令處理器
    /// </summary>
    public class LoginUserCommandHandler(
        ITenantUserServiceFactory tenantUserServiceFactory) : IRequestHandler<LoginUserCommand, AuthenticateResponse>
    {
        private readonly ITenantUserServiceFactory _tenantUserServiceFactory = tenantUserServiceFactory;

        public async ValueTask<AuthenticateResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var loginService = _tenantUserServiceFactory.GetServiceByTenantId(request.TenantId ?? string.Empty);
            return await loginService.HandleLoginUserCommandAsync(request, cancellationToken);
        }
    }
}
