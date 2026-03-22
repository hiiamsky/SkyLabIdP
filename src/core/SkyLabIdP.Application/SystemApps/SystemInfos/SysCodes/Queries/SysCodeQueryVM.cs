using SkyLabIdP.Application.Dtos;
using SkyLabIdP.Application.Dtos.SysCode;
using SkyLabIdP.Application.Common.Mappings;

namespace SkyLabIdP.Application.SystemApps.SystemInfos.SysCodes.Queries
{
    public class SysCodeQueryVM
    {
        public List<SysCodeResponseDto> SysCodes { get; set; } = [];

        public OperationResult operationResult { get; set; } = new OperationResult(true, "成功", 200);
    }
}


