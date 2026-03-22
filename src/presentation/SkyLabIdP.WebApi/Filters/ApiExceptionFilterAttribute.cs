using SkyLabIdP.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
namespace SkyLabIdP.WebApi.Filters
{
    /// <summary>
    /// ApiExceptionFilter
    /// </summary>
   [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly IDictionary<Type, Action<ExceptionContext>> _exceptionHandlers;
        private readonly ILogger<ApiExceptionFilterAttribute> _logger;

        /// <summary>
        /// ApiExceptionFilter
        /// </summary>
        public ApiExceptionFilterAttribute(ILogger<ApiExceptionFilterAttribute> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            // Register known exception types and handlers.
            _exceptionHandlers = new Dictionary<Type, Action<ExceptionContext>>
                {
                    { typeof(ValidationException), HandleValidationException },
                    { typeof(NotFoundException), HandleNotFoundException },
                    { typeof(ApiException), HandleApiException },
                };
        }

        /// <summary>
        /// OnException
        /// </summary>
        /// <param name="context"></param>
        public override void OnException(ExceptionContext context)
        {
            HandleException(context);
            base.OnException(context);
        }

        private void HandleException(ExceptionContext context)
        {
            Type type = context.Exception.GetType();
            if (_exceptionHandlers.ContainsKey(type))
            {
                _exceptionHandlers[type].Invoke(context);
                return;
            }

            HandleUnknownException(context);
        }

        private void HandleUnknownException(ExceptionContext context)
        {
            try
            {
                // 記錄詳細的異常信息，但僅供伺服器端使用
                _logger.LogError(context.Exception, "發生未處理的異常: {Message}", context.Exception.Message);
                
                // 安全地取得 IP 地址和使用者 ID
                string ipAddress = "Unknown";
                if (context.HttpContext?.Items != null && context.HttpContext.Items.ContainsKey("IPAddress"))
                {
                    ipAddress = context.HttpContext.Items["IPAddress"]?.ToString() ?? "Unknown";
                }

                string userId = "Anonymous";
                if (context.HttpContext?.User?.Identity?.IsAuthenticated == true)
                {
                    userId = context.HttpContext.User.Identity.Name ?? "Authenticated User";
                }
                
                // 記錄額外的上下文信息
                _logger.LogError(
                    "未處理異常詳細資訊 - 路徑: {Path}, 方法: {Method}, IP: {IP}, 使用者: {User}",
                    context.HttpContext?.Request?.Path ?? "Unknown",
                    context.HttpContext?.Request?.Method ?? "Unknown",
                    ipAddress,
                    userId);
            }
            catch (Exception ex)
            {
                // 捕獲並記錄日誌處理過程中的錯誤，以免引起更多問題
                _logger.LogError(ex, "記錄未處理異常時出現錯誤");
            }
            
            var details = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "伺服器發生錯誤",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                // 不提供具體錯誤詳細信息給客戶端
            };

            context.Result = new ObjectResult(details)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };

            context.ExceptionHandled = true;
        }

        private void HandleValidationException(ExceptionContext context)
        {
            var exception = context.Exception as ValidationException;

            if (exception != null)
            {
                try
                {
                    // 使用新增加的 GetFormattedValidationErrors 方法獲取詳細格式化的驗證錯誤信息
                    string detailedErrors = exception.GetFormattedValidationErrors();
                    
                    // 記錄詳細的驗證錯誤
                    _logger.LogWarning("輸入驗證錯誤發生:\n{DetailedErrors}", detailedErrors);

                    // 安全地記錄其他上下文信息
                    string ipAddress = "Unknown";
                    if (context.HttpContext?.Items != null && context.HttpContext.Items.ContainsKey("IPAddress"))
                    {
                        ipAddress = context.HttpContext.Items["IPAddress"]?.ToString() ?? "Unknown";
                    }

                    string path = context.HttpContext?.Request?.Path ?? "Unknown";
                    string method = context.HttpContext?.Request?.Method ?? "Unknown";

                    // 記錄請求上下文信息，方便追蹤
                    _logger.LogInformation(
                        "驗證錯誤請求上下文 - 路徑: {Path}, 方法: {Method}, IP: {IP}",
                        path,
                        method,
                        ipAddress);
                }
                catch (Exception ex)
                {
                    // 捕獲並記錄日誌處理過程中的錯誤
                    _logger.LogError(ex, "記錄驗證錯誤時出現異常");
                }
                
                // 回應給客戶端的錯誤資訊，這部分可以保留一些基本的驗證錯誤資訊
                var details = new ValidationProblemDetails(exception.Errors)
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                };

                context.Result = new BadRequestObjectResult(details);
                context.ExceptionHandled = true;
            }
        }

        private void HandleNotFoundException(ExceptionContext context)
        {
            var exception = context.Exception as NotFoundException;

            try
            {
                // 記錄找不到資源的錯誤
                _logger.LogInformation(
                    "找不到資源: {Message}, 路徑: {Path}, 方法: {Method}",
                    exception?.Message ?? "Unknown error",
                    context.HttpContext?.Request?.Path ?? "Unknown",
                    context.HttpContext?.Request?.Method ?? "Unknown");
            }
            catch (Exception ex)
            {
                // 捕獲並記錄日誌處理過程中的錯誤
                _logger.LogError(ex, "記錄NotFoundException時出現錯誤");
            }
            
            var details = new ProblemDetails()
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "找不到請求的資源",
                // 僅提供最基本的信息給客戶端
                Detail = "找不到對應的資料"
            };

            context.Result = new NotFoundObjectResult(details);
            context.ExceptionHandled = true;
        }

        private void HandleApiException(ExceptionContext context)
        {
            var exception = context.Exception as ApiException;

            try
            {
                _logger.LogWarning("業務邏輯錯誤: {Message}, 路徑: {Path}, 方法: {Method}",
                    exception?.Message ?? "Unknown error",
                    context.HttpContext?.Request?.Path ?? "Unknown",
                    context.HttpContext?.Request?.Method ?? "Unknown");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "記錄ApiException時出現錯誤");
            }

            var details = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "無法處理的請求",
                Detail = exception?.Message
            };

            context.Result = new BadRequestObjectResult(details);
            context.ExceptionHandled = true;
        }
    }
}


