using Mediator;
namespace SkyLabIdP.Application.SystemApps.SystemInfos.Menus.Queries
{

    public class MenuQuery : IRequest<MenuVM>
    {
        public string GroupID { get; set; } = "";

        public string FunctionID { get; set; } = "";

        public string LoginUserId { get; set; } = "";

    }
}


