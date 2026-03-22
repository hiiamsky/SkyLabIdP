using SkyLabIdP.Application.Dtos.FunctionGroup;
using SkyLabIdP.Application.Dtos.LoginUserInfo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;

namespace SkyLabIdP.Application.Dtos.User.Authentication
{
    public class AuthenticateResponse
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty; // 存取令牌
        public string RefreshToken { get; set; } = string.Empty; // 刷新令牌
        public bool MustResetPassword { get; set; } = false;
        public bool MustChangePassword { get; set; } = false;
        
        /// <summary>
        /// 審核狀態 - 敏感欄位，防止 Mass Assignment 攻擊
        /// </summary>
        [BindNever]
        [JsonIgnore]
        public bool IsApproved { get; set; } = false;
        
        /// <summary>
        /// 鎖定狀態 - 敏感欄位，防止 Mass Assignment 攻擊
        /// </summary>
        [BindNever]
        [JsonIgnore]
        public bool LockoutEnabled { get; set; } = false;
        
        /// <summary>
        /// 啟用狀態 - 敏感欄位，防止 Mass Assignment 攻擊
        /// </summary>
        [BindNever]
        [JsonIgnore]
        public bool IsActive { get; set; } = false;
        public string BranchCode { get; set; } = string.Empty;
        public string RegionCode { get; set; } = string.Empty;
        
        // 外部登入相關屬性
        public bool IsExternalLogin { get; set; } = false;
        public string ExternalProvider { get; set; } = string.Empty;
        public bool NeedsProfileCompletion { get; set; } = false;

        public List<FunctionGroupDto> FunctionGroups { get; set; } = [];
        public OperationResult OperationResult { get; set; } = new OperationResult(true, "使用者通過驗證", StatusCodes.Status200OK);

        public AuthenticateResponse()
        {
        }

        public AuthenticateResponse(LoginUserInfoDto userInfo, string encryptedUserId, string jwtToken, string refreshToken = "", bool mustResetPassword = false, bool mustChangePassword = false)
        {
            UserId = encryptedUserId;
            Email = userInfo.OfficialEmail ?? "";
            Username = userInfo.UserName ?? "";
            AccessToken = jwtToken; // 存取令牌
            RefreshToken = ""; // 刷新令牌
            MustResetPassword = mustResetPassword;
            MustChangePassword = mustChangePassword;
            IsApproved = userInfo.IsApproved;
            LockoutEnabled = userInfo.LockoutEnabled;
            IsActive = userInfo.IsActive;
            BranchCode = userInfo.BranchCode;
            RegionCode = userInfo.RegionCode;
            FunctionGroups = userInfo.FunctionGroups;
        }
    }
}