using Mediator;
using Microsoft.Extensions.Logging;

namespace SkyLabIdP.Application.Common.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IMessage
    {
        private readonly ILogger _logger;
        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            
            _logger.LogInformation(" SkyLabIdP Request:{@RequestName} {@Request}",
             requestName, request);

            return await next(request, cancellationToken);
        }
    }
}

