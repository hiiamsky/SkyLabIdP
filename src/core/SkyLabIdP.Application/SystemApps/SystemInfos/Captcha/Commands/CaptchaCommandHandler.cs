using System;
using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos.Captcha;
using Mediator;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SkyLabIdP.Application.SystemApps.SystemInfos.Captcha.Commands;

public class CaptchaCommandHandler : IRequestHandler<CaptchaCommand, CaptchaDto>
{
    private readonly ICaptchaService _captchaService;
    private readonly IDataProtectionService _dataprotectionservice ;
    private readonly ILogger<CaptchaCommandHandler> _logger;

    public CaptchaCommandHandler(ICaptchaService captchaService, IConfiguration configuration, IDataProtectionService dataprotectionservice , ILogger<CaptchaCommandHandler> logger)
    {
        _captchaService = captchaService;
        _dataprotectionservice  = dataprotectionservice;
        _logger = logger;
    }

    public async ValueTask<CaptchaDto> Handle(CaptchaCommand request, CancellationToken cancellationToken)
    {
        var captchaCode = await _captchaService.GenerateRandomCaptchaCodeAsync(cancellationToken);
        _logger.LogInformation("產生驗證碼：CaptchaCode={CaptchaCode}", captchaCode);   
        var captchaId = Guid.NewGuid().ToString();
        await _captchaService.StoreCaptchaCodeAsync(captchaId, captchaCode, cancellationToken);

        return new CaptchaDto
        {
            CaptchaId = _dataprotectionservice .Protect(captchaId)
        };
    }
}
