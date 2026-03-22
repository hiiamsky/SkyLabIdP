using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;

namespace SkyLabIdP.Application.Dtos.SkyLabDocUserDetail
{
    public class SkyLabDocUserDetailRequest
    {

        public string UserId { get; set; } = "";

        /// <summary>
        /// 系統角色 - 敏感欄位，防止 Mass Assignment 攻擊
        /// </summary>
        [BindNever]
        [JsonIgnore]
        public string SystemRole { get; set; } = "";

        public string FileId { get; set; } = "";


        public string UserName { get; set; } = "";

        public string FullName { get; set; } = "";


        public string BranchCode { get; set; } = "";

        public string SubordinateUnit { get; set; } = "";

        public string JobTitle { get; set; } = "";


        public string OfficialEmail { get; set; } = "";


        public string OfficialPhone { get; set; } = "";

        public string TenantId { get; set; } = ""; // 需在 DTO 中新增這欄


    }
}


