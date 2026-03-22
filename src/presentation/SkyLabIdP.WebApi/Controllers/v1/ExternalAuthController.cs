using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Asp.Versioning;
using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos;
using SkyLabIdP.Application.Dtos.User.Authentication;
using SkyLabIdP.WebApi.Controllers.v1;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SkyLabIdP.WebApi.Controllers.v1
{
    /// <summary>
    /// 外部提供者登入控制器
    /// </summary>
    /// <param name="externalLoginHandler"></param>
    /// <param name="logger"></param>
    /// <param name="configuration"></param>
    /// <param name="provider"></param>
    [ApiVersion("1.0")]
    [Route("skylabidp/api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class ExternalAuthController(
        IExternalLoginHandler externalLoginHandler,
        ILogger<ExternalAuthController> logger,
        IConfiguration configuration,
        IDataProtectionService provider,
        Mediator.IMediator mediator) : ApiController(provider, mediator)
    {
        private readonly IExternalLoginHandler _externalLoginHandler = externalLoginHandler;
        private readonly ILogger<ExternalAuthController> _logger = logger;
        private readonly IConfiguration _configuration = configuration;

        /// <summary>
        /// 啟動外部提供者登入流程
        /// </summary>
        /// <param name="provider">提供者名稱 (google)</param>
        /// <param name="tenantId">租戶識別碼（因為是瀏覽器導航，無法帶 Header，改由 query param 傳入）</param>
        /// <returns>重定向到外部提供者</returns>
        [HttpGet("login/{provider}")]
        [AllowAnonymous]
        public IActionResult Login([FromRoute] string provider, [FromQuery] string tenantId = "")
        {
            _logger.LogInformation("開始外部登入流程，提供者: {Provider}", provider);

            if (string.IsNullOrEmpty(provider))
            {
                _logger.LogWarning("外部登入失敗: 未指定提供者");
                return BadRequest(new { message = "未指定提供者" });
            }

            // 定義登入成功後的回調URL
            var redirectUrl = Url.Action(nameof(Callback), new { provider });

            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUrl,
                Items =
                {
                    ["scheme"] = provider,
                    // 將 TenantId 存入 OAuth state，因為 callback 是瀏覽器重定向，無法帶 X-Tenant-Id Header
                    // 前端使用：window.location.href = '/skylabidp/api/v1/ExternalAuth/login/google?tenantId=skylabmgm'
                    ["tenantId"] = tenantId
                }
            };

            // 根據提供者名稱轉換為對應的認證方案
            string scheme;
                scheme = provider.ToLower() switch
                {
                    "google" => "Google",
                    _ => throw new ArgumentException($"不支援的提供者: {provider}")
                };
                _logger.LogInformation("已映射提供者 {Provider} 到認證方案 {Scheme}", provider, scheme);
             

            _logger.LogInformation("重定向到外部提供者 {Scheme} 進行認證", scheme);
            return Challenge(properties, scheme);
        }

        /// <summary>
        /// 處理外部提供者的回調
        /// </summary>
        /// <param name="error">錯誤資訊</param>
        /// <returns>重定向到前端應用程序</returns>
        [HttpGet("callback")]
        [AllowAnonymous]
        public async Task<IActionResult> Callback([FromQuery] string error = null)
        {
            _logger.LogInformation("收到外部登入回調請求");

            // 檢查是否有錯誤
            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogWarning("外部登入失敗: 錯誤: {Error}", error);
                return RedirectToFrontend("error", "external_login_failed");
            }

            // 獲取登入結果 - 使用 "Cookies" 認證方案
            var result = await HttpContext.AuthenticateAsync("Cookies");
            if (!result.Succeeded)
            {
                _logger.LogWarning("外部認證未成功: {FailureMessage}", result.Failure?.Message);
                return RedirectToFrontend("error", "authentication_failed");
            }

                // 獲取提供者名稱與 TenantId（從 OAuth state 還原，因為 callback 無 Header）
                var scheme = result.Properties.Items["scheme"] ?? "";
                var tenantIdFromState = result.Properties.Items["tenantId"] ?? "";
                if (!string.IsNullOrEmpty(tenantIdFromState))
                {
                    HttpContext.Items["Tenant"] = tenantIdFromState;
                }
                _logger.LogInformation("外部認證成功，提供者: {Scheme}", scheme);

                // 從認證結果獲取必要信息
                var externalUserId = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
                var email = result.Principal.FindFirstValue(ClaimTypes.Email) ?? "";
                var name = result.Principal.FindFirstValue(ClaimTypes.Name) ?? "";

                _logger.LogInformation("外部用戶信息: ID={ExternalUserId}, Email={Email}, Name={Name}",
                    externalUserId, email, name);

                // 記錄所有索取的聲明
                var allClaims = result.Principal.Claims.Select(c => $"{c.Type}={c.Value}");
                _logger.LogDebug("外部登入取得的所有聲明: {Claims}", string.Join(", ", allClaims));

                // 調用外部登入處理服務
                _logger.LogInformation("開始處理外部登入");
                var response = await _externalLoginHandler.HandleExternalLoginAsync(
                    externalUserId,
                    scheme,
                    result.Principal.Claims,
                    email,
                    name,
                    TenantId
                );

                if (!response.OperationResult.Success)
                {
                    _logger.LogWarning("外部登入處理失敗: {Message}", response.OperationResult.Messages[0]);
                    return RedirectToFrontend("error", "login_processing_failed");
                }

                // 如需要補充資料，重定向到資料補充頁面
                if (response.NeedsProfileCompletion)
                {
                    _logger.LogInformation("用戶需要補充資料，重定向到資料補充頁面，用戶ID: {UserId}", response.UserId);
                    return RedirectToFrontend("complete-profile", response.UserId);
                }

                // 登入成功：refresh token 已由 ExternalLoginService 存入 Redis，access token 放 fragment
                _logger.LogInformation("外部登入成功，用戶ID: {UserId}", response.UserId);
                await HttpContext.SignOutAsync("Cookies"); // 登出 Cookie 認證，因為我們使用 JWT

                return RedirectToFrontend("login-success", response.UserId, response.AccessToken);
             
        }

        /// <summary>
        /// 完成外部登入用戶的資料補充
        /// </summary>
        /// <param name="userId">用戶 ID</param>
        /// <param name="userDetails">用戶詳細資料</param>
        /// <returns>操作結果</returns>
        [HttpPost("complete-registration")]
        public async Task<ActionResult<OperationResult>> CompleteRegistration(
            [FromQuery] string userId,
            [FromBody] ExternalUserRegistrationDto userDetails)
        {
            _logger.LogInformation("開始完成用戶註冊流程，用戶ID: {UserId}", userId);

            if (string.IsNullOrEmpty(userId) || userDetails == null)
            {
                _logger.LogWarning("完成註冊失敗: 無效的請求參數，用戶ID: {UserId}", userId);
                return BadRequest(new OperationResult(false, "無效的請求參數", StatusCodes.Status400BadRequest));
            }

            _logger.LogDebug("註冊資料: 姓名={Name}, 組織={Organization}, 職稱={JobTitle}, 電話={Phone}",
                userDetails.FullName, userDetails.BranchCode, userDetails.JobTitle, userDetails.OfficialPhone);

                userDetails.TenantId = TenantId; // 設定租戶ID
                var result = await _externalLoginHandler.CompleteRegistrationAsync(userId, userDetails);

                if (!result.Success)
                {
                    _logger.LogWarning("完成註冊失敗: {Message}, 用戶ID: {UserId}", result.Messages[0], userId);
                    return BadRequest(result);
                }

                _logger.LogInformation("用戶註冊成功完成，用戶ID: {UserId}", userId);
                return Ok(result);
             
        }

        /// <summary>
        /// 重定向到前端：access token 放 URL fragment（不進 server log），refresh token 已存 Redis 不傳前端
        /// </summary>
        private IActionResult RedirectToFrontend(string status, string message, string token = "")
        {
            var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:3000";
            var redirectUrl = $"{frontendUrl}/auth-callback?status={Uri.EscapeDataString(status)}&message={Uri.EscapeDataString(message)}";

            if (!string.IsNullOrEmpty(token))
            {
                // Access token 放 fragment：不傳到 server、不進日誌、不帶在 Referrer header
                redirectUrl += $"#access_token={Uri.EscapeDataString(token)}";
            }

            _logger.LogInformation("重定向到前端: 狀態={Status}, 訊息={Message}, 包含令牌={HasToken}",
                status, message, !string.IsNullOrEmpty(token));

            return Redirect(redirectUrl);
        }
    }
}