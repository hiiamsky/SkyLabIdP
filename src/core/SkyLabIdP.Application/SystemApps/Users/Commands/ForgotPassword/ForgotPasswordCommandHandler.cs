using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Common.Security;
using SkyLabIdP.Application.Dtos;
using SkyLabIdP.Application.Dtos.Email;
using SkyLabIdP.Domain.Entities;
using Mediator;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SkyLabIdP.Application.SystemApps.Users.Commands.ForgotPassword
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, OperationResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDataProtectionService _dataprotectionservice ;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly IUrlWhitelistValidator _urlValidator;
        private readonly string ResetPasswordUrl = "";

        private readonly ILogger<ForgotPasswordCommandHandler> _logger;
        public ForgotPasswordCommandHandler(
            UserManager<ApplicationUser> userManager, 
            IEmailService emailService, 
            IConfiguration configuration, 
            IDataProtectionService dataprotectionservice,
            IUrlWhitelistValidator urlValidator,
            ILogger<ForgotPasswordCommandHandler> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _configuration = configuration;
            _dataprotectionservice  = dataprotectionservice;
            _urlValidator = urlValidator;
            ResetPasswordUrl = configuration["ResetPasswordUrl"] ?? "";
            _logger = logger;
        }

        public async ValueTask<OperationResult> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByNameAsync(request.Username);

                if (user == null || user.Email != request.Email)
                {
                    return new OperationResult(false, "使用者資訊錯誤，帳號或是email錯誤", StatusCodes.Status404NotFound);
                }

                // 🔒 根據租戶選擇對應的重設密碼 URL 並驗證
                var resetPasswordUrl = GetTenantResetPasswordUrl(request.TenantId);
                
                // 驗證 URL 是否在白名單中 (SSRF 防護)
                var (isValid, errorMessage) = _urlValidator.ValidateUrl(resetPasswordUrl);
                if (!isValid)
                {
                    _logger.LogError("重設密碼 URL 驗證失敗：{Error}, URL: {Url}, TenantId: {TenantId}", 
                        errorMessage, resetPasswordUrl, request.TenantId);
                    return new OperationResult(false, "系統配置錯誤，請聯繫管理員", StatusCodes.Status500InternalServerError);
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var encryptedToken = _dataprotectionservice .Protect(token);
                
                var callbackUrl = $"{resetPasswordUrl}?userId={_dataprotectionservice .Protect(user.Id)}&Token={Uri.EscapeDataString(encryptedToken)}&tenantId={request.TenantId}";

                var emailDto = new EmailDto
                {
                    To = new List<string> {  "skyhsieh@skylab.com.tw" },
                    From = "skyhsieh@skylab.com.tw",
                    Subject = "[SkyLab查詢系統]重設密碼",
                    Body = $"帳號：{user.UserName}使用者您好,<br /> <a href='{callbackUrl}'>請點選此連結進行重設密碼</a><br />如無法點選請複製下方字串貼入網址列<br />{callbackUrl}"
                };

                await _emailService.SendAsync(emailDto);

                return new OperationResult(true, "重設密碼信件已寄出", StatusCodes.Status200OK);
        }

        /// <summary>
        /// 根據租戶ID獲取對應的重設密碼URL
        /// </summary>
        /// <param name="tenantId">租戶ID</param>
        /// <returns>租戶專屬的重設密碼URL</returns>
        private string GetTenantResetPasswordUrl(string tenantId)
        {
            if (string.IsNullOrEmpty(tenantId))
            {
                return _configuration["ResetPasswordUrl"] ?? "https://idp.skylab.com.tw/default/reset-password";
            }

            var tenantUrls = _configuration.GetSection("TenantResetPasswordUrls");
            var tenantUrl = tenantUrls[tenantId];
            
            if (!string.IsNullOrEmpty(tenantUrl))
            {
                return tenantUrl;
            }
            
            // 回退到預設 URL
            return _configuration["ResetPasswordUrl"] ?? "https://idp.skylab.com.tw/default/reset-password";
        }
    }
}


