using System;
using SkyLabIdP.Application.Dtos.Captcha;
using Mediator;

namespace SkyLabIdP.Application.SystemApps.SystemInfos.Captcha.Commands;

public class CaptchaCommand:IRequest<CaptchaDto>
{
    public string LoginUserId { get; set; } = "";   
}
