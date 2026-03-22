using System.Security.Claims;
using SkyLabIdP.Application.Dtos;
using SkyLabIdP.Application.Dtos.User.Authentication;
using SkyLabIdP.Domain.Entities;

namespace SkyLabIdP.Application.Common.Interfaces
{
    /// <summary>
    /// 外部登入處理介面
    /// </summary>
    public interface IExternalLoginHandler
    {
        /// <summary>
        /// 處理外部登入
        /// </summary>
        /// <param name="externalUserId">外部用戶 ID</param>
        /// <param name="provider">提供者名稱 (如 "Google")</param>
        /// <param name="claims">從提供者獲取的聲明</param>
        /// <param name="email">從提供者獲取的電子郵件</param>
        /// <param name="name">從提供者獲取的用戶名</param>
        /// <returns>登入回應</returns>
        Task<AuthenticateResponse> HandleExternalLoginAsync(
            string externalUserId, 
            string provider, 
            IEnumerable<Claim> claims, 
            string email, 
            string name,
            string tenantId);
        
        /// <summary>
        /// 完成用戶註冊流程
        /// </summary>
        /// <param name="userId">用戶 ID</param>
        /// <param name="userDetails">補充的用戶資料</param>
        /// <returns>操作結果</returns>
        Task<OperationResult> CompleteRegistrationAsync(string userId, ExternalUserRegistrationDto userDetails);
    }
}