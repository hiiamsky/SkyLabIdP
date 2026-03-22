using SkyLabIdP.Application.SystemApps.Users.Commands.LoginUser;
using Mediator;
using Microsoft.Extensions.Logging;

namespace SkyLabIdP.Application.Common.Behaviors
{
    public class UnhandledExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> _logger;

        public UnhandledExceptionBehavior(ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                return await next(request, cancellationToken);
            }
            catch (Exception ex)
            {
                var requestName = typeof(TRequest).Name;
                // 創建一個匿名物件，排除敏感資訊
                object safeRequest = request;

                if (request is LoginUserCommand loginUserCommand)
                {
                    safeRequest = new
                    {
                        loginUserCommand.UserName,
                        // 不包含 Password 屬性，或者將其設為 null 或遮蔽
                        Password = "****"
                    };
                }

                _logger.LogError(ex, "SkyLabIdP Request: Unhandled Exception for Request {@Name} {@Request}", requestName, safeRequest);
                throw;
            }
        }
    }
}

