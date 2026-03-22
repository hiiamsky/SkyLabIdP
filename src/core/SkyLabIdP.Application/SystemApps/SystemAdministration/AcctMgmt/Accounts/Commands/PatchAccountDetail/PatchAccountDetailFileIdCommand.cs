using SkyLabIdP.Application.Dtos.SkyLabDocUserDetail;
using Mediator;

namespace SkyLabIdP.Application.SystemApps.SystemAdministration.AcctMgmt.Accounts.Commands.PatchAccountDetail
{
    public class PatchAccountDetailFileIdCommand : IRequest<SkyLabDocUserDetailResponse>
    {
        public string UserId { get; set; } = "";

        public string FileId { get; set; } = "";
    }
}
