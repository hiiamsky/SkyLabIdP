using Mediator;
using System.ComponentModel.DataAnnotations;

namespace SkyLabIdP.Application.SystemApps.SystemAdministration.AcctMgmt.Accounts.Queries
{
    public class AcctMaintainQuery : IRequest<AcctMaintainQueryVM>
    {
        [Required]
        public string UserId { get; set; } = "";

        public string LoginUserId { get; set; } = "";
    }
};


