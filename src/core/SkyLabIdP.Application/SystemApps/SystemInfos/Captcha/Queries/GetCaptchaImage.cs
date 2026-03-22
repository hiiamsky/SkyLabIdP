using System;
using Mediator;

namespace SkyLabIdP.Application.SystemApps.SystemInfos.Captcha.Queries;

public class GetCaptchaImage: IRequest<byte[]>
{
    public string CaptchaId { get; set; } = "";

}
