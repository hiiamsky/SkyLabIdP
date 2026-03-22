using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace SkyLabIdP.WebApi.Helpers.Middleware
{
    /// <summary>
    /// ForbiddenMiddleware
    /// </summary>
    public class ForbiddenMiddleware
    {
        private const int ForbiddenStatusCode = StatusCodes.Status403Forbidden;
        private const int CustomErrorStatusCode = StatusCodes.Status403Forbidden;
        private readonly RequestDelegate _next;
        private readonly ILogger<ForbiddenMiddleware> _logger;

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="next"></param>
        /// <param name="logger"></param>
        public ForbiddenMiddleware(RequestDelegate next, ILogger<ForbiddenMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// 呼叫中介軟體
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            if (context.Response.StatusCode == ForbiddenStatusCode)
            {
                var username = context.User.Identity?.Name ?? "未知用戶"; // 如果無法取得使用者名稱，則顯示 "未知用戶"
                _logger.LogWarning("403 Forbidden - 沒有使用該功能的權限。用戶名：{Username}，用戶IP：{IP}，請求路徑：{Path}", username, context.Connection.RemoteIpAddress, context.Request.Path);
                await HandleForbiddenResponseAsync(context);
            }
        }

        private static async Task HandleForbiddenResponseAsync(HttpContext context)
        {
            context.Response.Clear();
            context.Response.StatusCode = CustomErrorStatusCode;
            await context.Response.WriteAsJsonAsync(new
            {
                OperationResult = new
                {
                    Success = false,
                    Message = "沒有使用該功能的權限",
                    StatusCode = CustomErrorStatusCode
                }
            });
        }
    }
}