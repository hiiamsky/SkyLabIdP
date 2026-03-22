using System;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SkyLabIdP.Application.SystemApps.Users.Commands.LoginUser;

public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    private readonly ILogger<LoginUserCommandValidator> _logger;

    public LoginUserCommandValidator(IConfiguration configuration, ILogger<LoginUserCommandValidator> logger)
    {
        _logger = logger;
        var enableCaptcha = configuration.GetSection("Captcha:EnableCaptcha").Value == "true";

        _logger.LogInformation("Captcha 啟用狀態: {EnableCaptcha}", enableCaptcha);

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("帳號是必填的。")
            .Custom((value, context) => {
                if (string.IsNullOrEmpty(value))
                {
                    _logger.LogWarning("驗證失敗: 帳號未輸入。");
                }
            });
        

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密碼是必填的。")
            .Custom((value, context) => {
                if (string.IsNullOrEmpty(value))
                {
                    _logger.LogWarning("驗證失敗: 密碼未輸入。");
                }
            });

        // 若 EnableCaptcha 為 true，則驗證 CaptchaIdId 和 CaptchaCode 是否有輸入
        if (enableCaptcha)
        {
            _logger.LogInformation("啟用了驗證碼驗證，檢查 驗證碼Id 和 驗證碼 是否有輸入。");

            RuleFor(x => x.CaptchaId)
                .NotEmpty().WithMessage("驗證碼ID是必填的。")
                .Custom((value, context) => {
                    if (string.IsNullOrEmpty(value))
                    {
                        _logger.LogWarning("驗證失敗: CaptchaIdId 未輸入。");
                    }
                });

            RuleFor(x => x.CaptchaCode)
                .NotEmpty().WithMessage("驗證碼是必填的。")
                .Custom((value, context) => {
                    if (string.IsNullOrEmpty(value))
                    {
                        _logger.LogWarning("驗證失敗: CaptchaCode 未輸入。");
                    }
                });
        }
    }
}