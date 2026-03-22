using Riok.Mapperly.Abstractions;
using SkyLabIdP.Application.Dtos.BranchArea;
using SkyLabIdP.Application.Dtos.Branch;
using SkyLabIdP.Application.Dtos.Function;
using SkyLabIdP.Application.Dtos.FunctionGroup;
using SkyLabIdP.Application.Dtos.SkyLabDocUserDetail;
using SkyLabIdP.Application.Dtos.SysCode;
using SkyLabIdP.Application.Common.Mappings;
using SkyLabIdP.Domain.Entities;

namespace SkyLabIdP.Application.Common.Mappings
{
    [Mapper]
    public partial class SkyLabIdPMapper
    {
        public static readonly SkyLabIdPMapper Instance = new SkyLabIdPMapper();

        public partial SkyLabDocUserDetailRequest SkyLabDocUserDetailToRequest(SkyLabDocUserDetail src);
        public partial SkyLabDocUserDetail RequestToSkyLabDocUserDetail(SkyLabDocUserDetailRequest src);
        public partial SkyLabDocUserDetailResponse SkyLabDocUserDetailToResponse(SkyLabDocUserDetail src);

        [MapProperty("ApplicationUser.IsActive", "IsActive")]
        [MapProperty("ApplicationUser.IsApproved", "IsApproved")]
        [MapProperty("ApplicationUser.LockoutEnabled", "LockoutEnabled")]
        public partial SkyLabDocUserDetailDto SkyLabDocUserDetailToDto(SkyLabDocUserDetail src);

        public partial BranchDto BranchToDto(Branch src);

        public partial FunctionGroupDto FunctionGroupToDto(FunctionGroup src);

        public partial FunctionDto FunctionToDto(Function src);

        public partial SysCodeResponseDto SysCodeToResponseDto(SysCode src);

        public partial BranchAreaDto BranchAreaToDto(BranchArea src);

        public partial List<SysCodeResponseDto> SysCodeListToDtoList(List<SysCode> src);
        public partial List<BranchAreaDto> BranchAreaListToDtoList(List<BranchArea> src);
        public partial List<FunctionGroupDto> FunctionGroupListToDtoList(List<FunctionGroup> src);

        public partial IQueryable<BranchDto> ProjectToBranchDto(IQueryable<Branch> q);
    }
}
