using Mediator;

namespace SkyLabIdP.Application.SystemApps.SystemAdministration.AcctMgmt.Accounts.Queries
{
    public class AccountQuery : IRequest<AccountQueryVM>
    {
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string OfficialEmail { get; set; } = "";

        public string FullName { get; set; } = "";

        public bool IsApproved { get; set; } = false;

        public bool LockoutEnabled { get; set; } = false;

        public bool IsActive { get; set; } = false;

        public string BranchCode { get; set; } = "";

        public int Status { get; set; } = 0;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string LoginUserId { get; set; } = "";

    }
}

