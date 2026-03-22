using SkyLabIdP.Application.Dtos.SkyLabDocUserDetail;
using Mediator;

namespace SkyLabIdP.Application.SystemApps.SystemAdministration.AcctMgmt.Accounts.Commands.PatchAccountDetail
{

    public class PatchAccountDetailOfficialPhoneCommand : IRequest<SkyLabDocUserDetailResponse>
    {
        public string UserId { get; set; } = "";

        public string OfficialPhone { get; set; } = "";
    }
}
