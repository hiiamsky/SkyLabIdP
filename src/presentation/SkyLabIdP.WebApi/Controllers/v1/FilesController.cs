using Asp.Versioning;
using SkyLabIdP.Application.Common.Exceptions;
using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos.File;

using SkyLabIdP.Application.Dtos.File.SkyLabUserDetailFile;


using SkyLabIdP.Application.SystemApps.UploadFiles.SkyLabDocUserDetailFiles;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;



namespace SkyLabIdP.WebApi.Controllers.v1
{
    /// <summary>
    /// 檔案管理
    /// </summary>
    [ApiVersion("1.0")]
    [Route("skylabidp/api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class FilesController(IDataProtectionService dataProtectionService, Mediator.IMediator mediator): ApiController(dataProtectionService, mediator)
    {
        private const string NoFileUploadedMessage = "檔案上傳失敗，沒有檔案上傳";

        /// <summary>
        /// 上傳帳號申請單檔案
        /// </summary>
        /// <param name="fileUploadDto"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [EnableRateLimiting("FileUploadPolicy")]
        [RequestSizeLimit(5 * 1024 * 1024)] // 5MB 限制
        [RequestFormLimits(MultipartBodyLengthLimit = 5 * 1024 * 1024)]
        [HttpPost("register/upload-skylabdocuserdetail-file")]
        public async Task<ActionResult<SkyLabUserDetailFileResponse>> UploadSkyLabDocUserDetailFile([FromForm] FileDto fileUploadDto)
        {
            LogAction("開始上傳帳號申請單");

            if (fileUploadDto == null || fileUploadDto.File == null)
            {
                LogAction("上傳帳號申請單失敗");
                return BadRequest(NoFileUploadedMessage);
            }

            var response = await Mediator.Send(new SkyLabUserDetailFileUploadCommand(fileUploadDto));
            LogAction("上傳帳號申請單結束");
            // Depending on your application, you may want to adjust the response
            return response;
        }
        /// <summary>
        /// 重新上傳帳號申請單
        /// </summary>
        /// <param name="userid">使用者編號</param>
        /// <param name="fileUploadDto">檔案Dto</param>
        /// <returns></returns>
        /// <response code="200">成功重新上傳帳號申請單</response>
        /// <response code="400">重新上傳帳號申請單失敗</response>
        [Authorize]
        [EnableRateLimiting("FileUploadPolicy")]
        [RequestSizeLimit(5 * 1024 * 1024)] // 5MB 限制
        [RequestFormLimits(MultipartBodyLengthLimit = 5 * 1024 * 1024)]
        [HttpPost("register/{userid}/re-upload-skylabdocuserdetail-file")]
        public async Task<ActionResult<SkyLabUserDetailFileResponse>> ReUploadSkyLabdocUserDetailFile(string userid, [FromForm] FileDto fileUploadDto)
        {
            LogAction("開始重新上傳帳號申請單", userid);


            userid = DataProtectionService.Unprotect(userid);

            if (LoginUserId != userid)
            {
                LogAction("重新上傳帳號申請單失敗", userid);
                return BadRequest("無權限上傳檔案");
            }

            if (fileUploadDto == null || fileUploadDto.File == null)
            {
                LogAction("重新上傳帳號申請單失敗", userid);
                return BadRequest(NoFileUploadedMessage);
            }


            var response = await Mediator.Send(new SkyLabUserDetailFileUploadCommand(fileUploadDto));

            LogAction("重新上傳帳號申請單結束", userid);
            // Depending on your application, you may want to adjust the response
            return response;
        }

        

        
    }
}


