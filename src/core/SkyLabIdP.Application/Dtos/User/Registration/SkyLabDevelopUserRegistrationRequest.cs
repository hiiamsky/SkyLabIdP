using System;

namespace SkyLabIdP.Application.Dtos.User.Registration;

public class SkyLabDevelopUserRegistrationRequest: BaseUserRegistrationRequest
{
   
        public string FullName { get; set; } = "";

        public string BranchCode { get; set; } = "";

        public string SubordinateUnit { get; set; } = "";

        public string JobTitle { get; set; } = "";
        public string OfficialEmail { get; set; } = "";
        public string OfficialPhone { get; set; } = "";

}
