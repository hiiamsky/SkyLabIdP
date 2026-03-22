using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;

namespace SkyLabIdP.Application.Dtos.SkyLabDocUserDetail
{
    public class SkyLabDocUserDetailDto
    {

        public string UserId { get; set; } = "";

        /// <summary>
        /// 系統角色 - 敏感欄位，防止 Mass Assignment 攻擊
        /// </summary>
        [BindNever]
        [JsonIgnore]
        public string SystemRole { get; set; } = "";

        public string FileId { get; set; } = "";

        public string OriginalFileName { get; set; } = "";

        public string FileExtension { get; set; } = "";

        public string UserName { get; set; } = "";

        public string FullName { get; set; } = "";

        public string BranchCode { get; set; } = "";

        public string BranchName { get; set; } = "";

        public string SubordinateUnit { get; set; } = "";

        public string JobTitle { get; set; } = "";

        public string OfficialEmail { get; set; } = "";

        public string OfficialPhone { get; set; } = "";

        /// <summary>
        /// 審核狀態 - 敏感欄位，防止 Mass Assignment 攻擊
        /// </summary>
        [BindNever]
        [JsonIgnore]
        public bool IsApproved { get; set; }

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
        public bool IsActive { get; set; }

        public string MoicaCardNumber { get; set; } = "";

        public string ReasonsForDisapproval { get; set; } = "";

        public static implicit operator SkyLabDocUserDetailDto(Task<SkyLabDocUserDetailDto> v)
        {
            throw new NotImplementedException();
        }
    }
}


