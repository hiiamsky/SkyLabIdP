using System;
using SkyLabIdP.Application.Dtos.Captcha;
using Mediator;

namespace SkyLabIdP.Application.SystemApps.SystemInfos.Captcha.Queries;

public class GetCaptchaAudio : IRequest<byte[]>
{
    public string CaptchaId { get; set; } = "";
}
