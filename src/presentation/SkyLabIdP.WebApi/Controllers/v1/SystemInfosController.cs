using Asp.Versioning;
using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos.Captcha;
using SkyLabIdP.Application.Dtos.SysCode;

using SkyLabIdP.Application.SystemApps.SystemInfos.Areas.Queries;
using SkyLabIdP.Application.SystemApps.SystemInfos.Branches.Queries;
using SkyLabIdP.Application.SystemApps.SystemInfos.Captcha.Commands;
using SkyLabIdP.Application.SystemApps.SystemInfos.Captcha.Queries;
using SkyLabIdP.Application.SystemApps.SystemInfos.Menus.Queries;

using SkyLabIdP.Application.SystemApps.SystemInfos.SysCodes.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace SkyLabIdP.WebApi.Controllers.v1
{
    /// <summary>
    /// 系統資訊
    /// </summary>
    [ApiVersion("1.0")]
    [Route("skylabidp/api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class SystemInfosController(IDataProtectionService provider, Mediator.IMediator mediator): ApiController(provider, mediator)
    {


        /// <summary>
        /// 取得分公司清單
        /// </summary>
        /// <returns></returns>
        /// <response code="200">成功取得分公司清單</response>
        /// <response code="404">取得分公司清單失敗</response>
        [AllowAnonymous]
        [HttpGet("branch")]
        [OutputCache(Duration = 120)]
        [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any, NoStore = false)]
        public async Task<IActionResult> GetBranches()
        {
            LogAction("開始取得分公司清單");
            var query = new GetBranchesQuery();
            var result = await Mediator.Send(query);
            LogAction("取得分公司清單結束");
            if (result.OperationResult.Success)
                return Ok(result);
            else
                return NotFound(result);
        }

        /// <summary>
        /// 取得MENU
        /// </summary>
        /// <param name="menuQuery"></param>
        /// <returns></returns>
        /// <response code="200">成功取得MENU</response>
        /// <response code="404">取得MENU失敗</response>
        /// <remarks>需要登入</remarks>

        [HttpGet("menu")]
        [OutputCache(Duration = 120)]
        [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any, NoStore = false)]
        public async Task<IActionResult> GetMenu([FromQuery] MenuQuery menuQuery)
        {
            LogAction("開始取得MENU");
            menuQuery.LoginUserId = LoginUserId;
            var result = await Mediator.Send(menuQuery);
            LogAction("取得MENU結束");
            if (result.OperationResult.Success)
            {
                LogAction("取得MENU成功");
                return Ok(result);
            }
            else
            {
                LogAction("取MENU發生錯誤");
                return NotFound(result);
            }
        }



        private async Task<SysCodeQueryVM> GetSysCodeByType(string sysCodeType)
        {
            var sysCodeQuery = new SysCodeQuery { SysCodeRequestDto = new SysCodeRequestDto { Type = sysCodeType }, LoginUserId = LoginUserId };
            var result = await Mediator.Send(sysCodeQuery);
            return result;
        }

        private async Task<SysCodeQueryVM> GetSysCodeByTypeAndCode(string sysCodeType, string code)
        {
            var sysCodeQuery = new SysCodeQuery { SysCodeRequestDto = new SysCodeRequestDto { Type = sysCodeType, Code = code }, LoginUserId = LoginUserId };
            var result = await Mediator.Send(sysCodeQuery);
            return result;
        }




        
       
        /// <summary>
        /// 圖形驗證碼
        /// </summary>
        [AllowAnonymous]
        [HttpGet("generate-captcha")]
        public async Task<IActionResult> GenerateCaptchaAsync(CancellationToken cancellationToken)
        {

                LogAction("開始生成新的驗證碼");
                var command = new CaptchaCommand
                {
                    LoginUserId = LoginUserId // 可根據需求設置或透過請求資訊取得用戶 ID
                };

                CaptchaDto captchaDto = await Mediator.Send(command, cancellationToken);
                LogAction($"驗證碼生成成功：CaptchaId={captchaDto.CaptchaId}");

                return Ok(captchaDto);
             
        }

        /// <summary>
        /// 取得驗證碼音訊
        /// </summary>
        /// <param name="captchaId">驗證碼的 ID（經過保護）</param>
        /// <param name="cancellationToken">取消請求的 token</param>
        /// <returns>包含驗證碼音訊的 byte array</returns>
        [HttpGet("get-captcha-audio/{captchaId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCaptchaAudioAsync(string captchaId, CancellationToken cancellationToken)
        {
                LogAction($"開始取得驗證碼音訊：CaptchaId={captchaId}");


                var query = new GetCaptchaAudio
                {
                    CaptchaId = captchaId
                };

                byte[] captchaAudio = await Mediator.Send(query, cancellationToken);

                if (captchaAudio == null || captchaAudio.Length == 0)
                {
                    LogAction($"驗證碼音訊無法生成或不存在：CaptchaId={captchaId}");

                    return NotFound("無法取得驗證碼音訊");
                }
                LogAction($"驗證碼音訊生成成功：CaptchaId={captchaId}");
                // 防止瀏覽器cache
                Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                Response.Headers["Pragma"] = "no-cache";
                Response.Headers["Expires"] = "0";

                return File(captchaAudio, "audio/wav", "captcha-audio.wav");
             
        }
        /// <summary>
        /// 取得驗證碼圖像
        /// </summary>
        /// <param name="captchaId">驗證碼的 ID（經過保護）</param>
        /// <param name="cancellationToken">取消請求的 token</param>
        /// <returns>包含驗證碼音訊的 byte array</returns>
        [HttpGet("get-captcha-image/{captchaId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCaptchaImageAsync(string captchaId, CancellationToken cancellationToken)
        {
                LogAction($"開始取得驗證碼圖像：CaptchaId={captchaId}");


                var query = new GetCaptchaImage
                {
                    CaptchaId = captchaId
                };

                byte[] captchaImage = await Mediator.Send(query, cancellationToken);

                if (captchaImage == null || captchaImage.Length == 0)
                {
                    LogAction($"驗證碼圖像無法生成或不存在：CaptchaId={captchaId}");

                    return NotFound("無法取得驗證碼圖像");
                }
                LogAction($"驗證碼圖像生成成功：CaptchaId={captchaId}");
                // 防止瀏覽器cache
                Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                Response.Headers["Pragma"] = "no-cache";
                Response.Headers["Expires"] = "0";

                return File(captchaImage, "image/png", "captcha-image.png");
             
        }


    }

}
