namespace SkyLabIdP.WebApi.Controllers.v1
{
    using Asp.Versioning;
    using SkyLabIdP.Application.Common.Extensions;
    using SkyLabIdP.Application.Common.Interfaces;
    using Mediator;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Mvc;
    using Serilog;
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Security.Claims;

    /// <summary>
    /// API控制器
    /// </summary>
    /// <remarks>
    /// 建構子
    /// </remarks>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("skylabidp/api/v{version:apiVersion}/[controller]")]
    public class ApiController(IDataProtectionService dataProtectionService, IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;
        /// <summary>
        /// 中介者
        /// </summary>
        protected IMediator Mediator => _mediator;

        /// <summary>
        /// 資料保護
        /// </summary>
        protected readonly IDataProtectionService DataProtectionService = dataProtectionService;


        /// <summary>
        /// 取得登入使用者ID
        /// </summary>
        protected string LoginUserId
        {
            get
            {
                if (User.Identity != null && User.Identity.IsAuthenticated)
                {
                    string protectedUserId = User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
                    if (!string.IsNullOrEmpty(protectedUserId))
                    {
                        try
                        {
                            return dataProtectionService.Unprotect(protectedUserId);
                        }
                        catch (Exception ex)
                        {
                            LogAction($"解密失敗。失敗原因:{ex.Message}", LogLevel.Error);

                            return "Anonymous";
                        }
                    }
                }
                return "Anonymous";
            }
        }
        /// <summary>
        /// 紀錄動作
        /// </summary>
        protected void LogAction(string message, string loginUserName = "", [CallerMemberName] string memberName = "")
        {
            LogAction(message, LogLevel.Information, loginUserName, memberName);
        }

        /// <summary>
        /// 紀錄動作
        /// </summary>
        protected void LogAction(string message, LogLevel logLevel, string loginUserName = "", [CallerMemberName] string memberName = "")
        {
            string ipAddress = ClientIpAddress;
            
            if (string.IsNullOrEmpty(loginUserName))
            {
                loginUserName = LoginUserId;
            }
            var log = Log.ForContext("IP", ipAddress)
                         .ForContext("CreationTime", DateTime.UtcNow) // Use UtcNow for consistency across different time zones
                         .ForContext("LoggedInUser", loginUserName)
                         .ForContext("Action", memberName);

            switch (logLevel)
            {
                case LogLevel.Debug:
                    log.Debug(message);
                    break;
                case LogLevel.Error:
                    log.Error(message);
                    break;
                case LogLevel.Warning:
                    log.Warning(message);
                    break;
                default:
                    log.Information(message);
                    break;
            }
        }
        
        /// <summary>
        /// 從 HttpContext 取得目前的租戶代碼 (x-tenant-id)
        /// </summary>
        protected string TenantId => HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;

        /// <summary>
        /// 從 HttpContext 取得客戶端 IP 地址
        /// 優先級：HttpContext.Items["IPAddress"] > RemoteIpAddress > "Unknown"
        /// </summary>
        protected string ClientIpAddress
        {
            get
            {
                string ipAddress = HttpContext.Items["IPAddress"] as string ?? "Unknown";
                
                if (string.IsNullOrEmpty(ipAddress) || ipAddress == "Unknown")
                {
                    ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                }
                
                return ipAddress;
            }
        }

        /// <summary>
        /// 處理驗證錯誤 - 只記錄錯誤，返回簡單的錯誤響應
        /// </summary>
        /// <param name="validationException">驗證例外</param>
        /// <param name="actionName">動作名稱（用於日誌記錄）</param>
        /// <param name="userName">使用者名稱（用於日誌記錄）</param>
        /// <returns>簡單的 BadRequest 響應</returns>
        protected ActionResult<T> HandleValidationError<T>(
            SkyLabIdP.Application.Common.Exceptions.ValidationException validationException, 
            string actionName = "", 
            string userName = "")
        {
            // 只記錄詳細的驗證錯誤供開發人員除錯，不暴露給前端
            var errorMessage = validationException.GetFormattedValidationErrors();
            LogAction($"{actionName}失敗。詳細驗證錯誤: {errorMessage}", LogLevel.Error, userName);
            
            // 返回簡單的錯誤響應，不暴露技術細節
            return BadRequest(StatusCodes.Status400BadRequest);
        }

    }
}


