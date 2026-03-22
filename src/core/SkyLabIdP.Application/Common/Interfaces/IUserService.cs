using SkyLabIdP.Application.Dtos;
using SkyLabIdP.Application.Dtos.SkyLabDocUserDetail;
using SkyLabIdP.Application.Dtos.LoginUserInfo;
using SkyLabIdP.Application.Dtos.User.Authentication;
using SkyLabIdP.Application.SystemApps.SystemAdministration.AcctMgmt.Accounts.Commands.CreateAccout;
using SkyLabIdP.Application.SystemApps.SystemAdministration.AcctMgmt.Accounts.Commands.PutAccountDetail;
using SkyLabIdP.Application.SystemApps.SystemAdministration.AcctMgmt.Accounts.Queries;
using SkyLabIdP.Application.SystemApps.Users.Commands.ChangePassWord;
using SkyLabIdP.Application.SystemApps.Users.Commands.LoginUser;
using SkyLabIdP.Application.SystemApps.Users.Commands.ResetPassword;

namespace SkyLabIdP.Application.Common.Interfaces
{
    public interface IUserService
    {
        /// <summary>
        /// 取得登入使用者資訊
        /// </summary>
        /// <param name="loginUserId"></param>
        /// <returns></returns>
        Task<LoginUserInfoDto> GetLoginUserInfoAsync(string loginUserId, CancellationToken cancellationToken);

        /// <summary>
        /// 處理登入使用者命令
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<AuthenticateResponse> HandleLoginUserCommandAsync(LoginUserCommand request, CancellationToken cancellationToken);
        /// <summary>
        /// 處理變更密碼命令
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<OperationResult> HandleChangePassWordCommandAsync(ChangePassWordCommand request, CancellationToken cancellationToken);
        /// <summary>
        /// 處理重設密碼命令
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<OperationResult> HandleResetPasswordAsync(ResetPasswordCommand request, CancellationToken cancellationToken);
        /// <summary>
        /// 處理建立帳號命令
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<SkyLabDocUserDetailResponse> HandleCreateAccoutCommandAsync(CreateAccoutCommand request, CancellationToken cancellationToken);
        /// <summary>
        /// 處理更新帳號明細命令
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<AcctMaintainQueryVM> HandlePutAccountCommandAsync(PutAccountCommad request, CancellationToken cancellationToken);
        /// <summary>
        /// 處理帳號查詢命令
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<AccountQueryVM> HandleAccountQueryAsync(AccountQuery request, CancellationToken cancellationToken);
        /// <summary>
        /// 處理帳號維護查詢命令
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<AcctMaintainQueryVM> HandleAcctMaintainQueryAsync(AcctMaintainQuery request, CancellationToken cancellationToken);



        /// <summary>
        /// 為外部登入用戶創建用戶詳情
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <param name="username">用戶名稱</param>
        /// <param name="fullName">全名</param>
        /// <param name="email">電子郵件</param>
        /// <param name="tenantId">租戶ID</param>
        /// <param name="cancellationToken">取消權杖</param>
        /// <returns>操作結果</returns>
        Task CreateExternalUserDetailAsync(
            string userId,
            string username,
            string fullName,
            string email,
            string tenantId,
            CancellationToken cancellationToken);
        /// <summary>
        /// 更新外部用戶註冊資料
        /// </summary>
        /// <param name="encryptedUserId">加密的用戶ID</param>
        /// <param name="userDetails">用戶註冊詳情</param>
         /// <param name="cancellationToken">取消權杖</param>
        /// <returns>操作結果</returns>
        Task<OperationResult> UpdateExternalUserRegistrationAsync(
            string encryptedUserId,
            ExternalUserRegistrationDto userDetails,
            CancellationToken cancellationToken);


    }
}


