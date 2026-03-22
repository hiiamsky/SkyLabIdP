using System;
using SkyLabIdP.Application.Common.Interfaces;
using Mediator;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SkyLabIdP.Application.SystemApps.SystemInfos.Captcha.Queries;

public class GetCaptchaImageHandler : IRequestHandler<GetCaptchaImage, byte[]>
{
    private readonly ICaptchaService _captchaService;
    private readonly IDataProtectionService _dataprotectionservice ;
    private readonly ILogger<GetCaptchaImageHandler> _logger;

    public GetCaptchaImageHandler(ICaptchaService captchaService, IConfiguration configuration, IDataProtectionService dataprotectionservice , ILogger<GetCaptchaImageHandler> logger)
    {
        _captchaService = captchaService;
        _dataprotectionservice  = dataprotectionservice;
        _logger = logger;
    }    
    public async ValueTask<byte[]> Handle(GetCaptchaImage request, CancellationToken cancellationToken)
    {
        var captchaId = _dataprotectionservice .Unprotect(request.CaptchaId);
        var (success, captchaCode) = await _captchaService.TryGetCaptchaCodeAsync(captchaId, cancellationToken);
        if (!success)
        {
            _logger.LogWarning("無法取得驗證碼：CaptchaId={CaptchaId}", captchaId);
            return [];
        }
        var captchaImage = await _captchaService.GenerateCaptchaImageAsync(captchaCode, cancellationToken);
        return captchaImage;
    }
}
