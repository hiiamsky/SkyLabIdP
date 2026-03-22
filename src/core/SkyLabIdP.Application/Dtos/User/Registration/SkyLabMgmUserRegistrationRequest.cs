using System;

namespace SkyLabIdP.Application.Dtos.User.Registration;

public class SkyLabMgmUserRegistrationRequest: BaseUserRegistrationRequest
{
        public string FullName { get; set; } = "";
        public string BranchCode { get; set; } = "";

        public string SubordinateUnit { get; set; } = "";

        public string JobTitle { get; set; } = "";

        public string OfficialPhone { get; set; } = "";

        public string FileId { get; set; } = "";
}
