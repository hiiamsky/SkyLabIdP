using SkyLabIdP.Application.Dtos.SysCode;
using SkyLabIdP.Application.Common.Mappings;
using Mediator;

namespace SkyLabIdP.Application.SystemApps.SystemInfos.SysCodes.Queries
{
    public class SysCodeQuery : IRequest<SysCodeQueryVM>
    {
        public SysCodeRequestDto SysCodeRequestDto { get; set; } = new SysCodeRequestDto();

        public string LoginUserId { get; set; } = "";
    }
}
