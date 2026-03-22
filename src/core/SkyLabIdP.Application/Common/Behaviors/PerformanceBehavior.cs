using SkyLabIdP.Application.SystemApps.Users.Commands.LoginUser;
using Mediator;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace SkyLabIdP.Application.Common.Behaviors
{
  internal class PerformanceBehavior<TRequest, TResponse>(
    ILogger<PerformanceBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
  {
    private readonly Stopwatch _timer = new Stopwatch();
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger = logger;

        public async ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
      _timer.Start();
      var response = await next(request, cancellationToken);
      _timer.Stop();

      var elapsedMilliseconds = _timer.ElapsedMilliseconds;
      if (elapsedMilliseconds <= 500) return response;

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

      _logger.LogWarning("SkyLabIdP Long Running Request: {@Name} ({@ElapsedMilliseconds} milliseconds) {@Request}",
        requestName, elapsedMilliseconds, safeRequest);

      return response;
    }
  }
}

