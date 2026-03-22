using SkyLabIdP.Application.Dtos.FunctionGroup;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;

namespace SkyLabIdP.Application.Dtos.LoginUserInfo
{
    public class LoginUserInfoDto
    {
        public bool IsUserEligible { get; set; } = false;
        public string UserId { get; set; } = "";
        
        /// <summary>
        /// 系統角色 - 敏感欄位，防止 Mass Assignment 攻擊
        /// </summary>
        [BindNever]
        [JsonIgnore]
        public string SystemRole { get; set; } = "";

        public string UserName { get; set; } = "";

        public string BranchCode { get; set; } = "";

        public string RegionCode { get; set; } = "";

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

        public string OfficialEmail { get; set; } = "";

        public List<FunctionGroupDto> FunctionGroups { get; set; } = [];

        public OperationResult OperationResult { get; set; } = new OperationResult(false, "", 400);

    }
}

