
namespace SkyLabIdP.WebApi.Helpers.Middleware
{
    /// <summary>
    /// 取得並存儲 IP 地址
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="next"></param>
        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        /// <summary>
        /// 呼叫中介軟體
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            // 取得 IP 地址
            var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = context.Connection.RemoteIpAddress?.ToString();
            }

            // 將 IP 地址存儲在 HttpContext.Items 中
            context.Items["IPAddress"] = ipAddress;

            // 呼叫下一個中介軟體
            await _next(context);
        }
    }
}