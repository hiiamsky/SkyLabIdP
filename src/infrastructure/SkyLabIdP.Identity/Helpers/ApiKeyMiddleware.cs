using SkyLabIdP.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace SkyLabIdP.Identity.Helpers
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IApiKeyValidation _apiKeyValidation;

        // Google OAuth 重定向流程是瀏覽器行為，無法帶自訂 Header，必須排除
        private static readonly string[] _bypassPaths =
        [
            "/skylabidp/api/v1/ExternalAuth/login",
            "/skylabidp/api/v1/ExternalAuth/callback",
            "/skylabidp/api/v1/ExternalAuth/google-callback",
            "/skylabidp/api/v1/Jwks/.well-known/jwks.json"
        ];

        public ApiKeyMiddleware(RequestDelegate next, IApiKeyValidation apiKeyValidation)
        {
            _next = next;
            _apiKeyValidation = apiKeyValidation;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            if (Array.Exists(_bypassPaths, p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            if (string.IsNullOrWhiteSpace(context.Request.Headers["X-Api-Key"]))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }
            string userApiKey = context.Request.Headers["X-Api-Key"].ToString();
            if (!_apiKeyValidation.IsValidApiKey(userApiKey))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }
            await _next(context);
        }
    }

}
