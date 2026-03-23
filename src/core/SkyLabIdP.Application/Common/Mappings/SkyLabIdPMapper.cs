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

        // SkyLabDocUserDetail -> SkyLabDocUserDetailRequest
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.SerialNo))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.RegionCode))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.DepartmentName))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.LastLoginDatetime))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.CreateBy))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.CreateDatetime))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.LastUpdatedBy))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.LastUpdateDatetime))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.MoicaCardNumber))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.ReasonsForDisapproval))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.UserTenantGuid))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.UserTenant))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.ApplicationUser))]
        [MapperIgnoreTarget(nameof(SkyLabDocUserDetailRequest.TenantId))]
        public partial SkyLabDocUserDetailRequest SkyLabDocUserDetailToRequest(SkyLabDocUserDetail src);

        // SkyLabDocUserDetailRequest -> SkyLabDocUserDetail
        [MapperIgnoreSource(nameof(SkyLabDocUserDetailRequest.TenantId))]
        [MapperIgnoreTarget(nameof(SkyLabDocUserDetail.SerialNo))]
        [MapperIgnoreTarget(nameof(SkyLabDocUserDetail.RegionCode))]
        [MapperIgnoreTarget(nameof(SkyLabDocUserDetail.DepartmentName))]
        [MapperIgnoreTarget(nameof(SkyLabDocUserDetail.LastLoginDatetime))]
        [MapperIgnoreTarget(nameof(SkyLabDocUserDetail.CreateBy))]
        [MapperIgnoreTarget(nameof(SkyLabDocUserDetail.CreateDatetime))]
        [MapperIgnoreTarget(nameof(SkyLabDocUserDetail.LastUpdatedBy))]
        [MapperIgnoreTarget(nameof(SkyLabDocUserDetail.LastUpdateDatetime))]
        [MapperIgnoreTarget(nameof(SkyLabDocUserDetail.MoicaCardNumber))]
        [MapperIgnoreTarget(nameof(SkyLabDocUserDetail.ReasonsForDisapproval))]
        [MapperIgnoreTarget(nameof(SkyLabDocUserDetail.UserTenantGuid))]
        [MapperIgnoreTarget(nameof(SkyLabDocUserDetail.UserTenant))]
        [MapperIgnoreTarget(nameof(SkyLabDocUserDetail.ApplicationUser))]
        public partial SkyLabDocUserDetail RequestToSkyLabDocUserDetail(SkyLabDocUserDetailRequest src);

        // SkyLabDocUserDetail -> SkyLabDocUserDetailResponse
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.SerialNo))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.RegionCode))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.DepartmentName))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.LastLoginDatetime))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.CreateBy))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.CreateDatetime))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.LastUpdatedBy))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.LastUpdateDatetime))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.MoicaCardNumber))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.ReasonsForDisapproval))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.UserTenantGuid))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.UserTenant))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.ApplicationUser))]
        [MapperIgnoreTarget(nameof(SkyLabDocUserDetailResponse.BranchName))]
        [MapperIgnoreTarget(nameof(SkyLabDocUserDetailResponse.operationResult))]
        public partial SkyLabDocUserDetailResponse SkyLabDocUserDetailToResponse(SkyLabDocUserDetail src);

        // SkyLabDocUserDetail -> SkyLabDocUserDetailDto
        [MapProperty("ApplicationUser.IsActive", "IsActive")]
        [MapProperty("ApplicationUser.IsApproved", "IsApproved")]
        [MapProperty("ApplicationUser.LockoutEnabled", "LockoutEnabled")]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.SerialNo))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.RegionCode))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.DepartmentName))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.LastLoginDatetime))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.CreateBy))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.CreateDatetime))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.LastUpdatedBy))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.LastUpdateDatetime))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.UserTenantGuid))]
        [MapperIgnoreSource(nameof(SkyLabDocUserDetail.UserTenant))]
        [MapperIgnoreTarget(nameof(SkyLabDocUserDetailDto.OriginalFileName))]
        [MapperIgnoreTarget(nameof(SkyLabDocUserDetailDto.FileExtension))]
        [MapperIgnoreTarget(nameof(SkyLabDocUserDetailDto.BranchName))]
        public partial SkyLabDocUserDetailDto SkyLabDocUserDetailToDto(SkyLabDocUserDetail src);

        public partial BranchDto BranchToDto(Branch src);

        // FunctionGroup -> FunctionGroupDto
        [MapperIgnoreSource(nameof(FunctionGroup.IsDisabled))]
        public partial FunctionGroupDto FunctionGroupToDto(FunctionGroup src);

        // Function -> FunctionDto
        [MapperIgnoreSource(nameof(Function.IsDisabled))]
        [MapperIgnoreSource(nameof(Function.IsDisplayInMenu))]
        [MapperIgnoreSource(nameof(Function.FunctionGroup))]
        [MapperIgnoreSource(nameof(Function.PolicyConfigurations))]
        [MapperIgnoreTarget(nameof(FunctionDto.Permissions))]
        public partial FunctionDto FunctionToDto(Function src);

        // SysCode -> SysCodeResponseDto
        [MapperIgnoreSource(nameof(SysCode.SerialNo))]
        [MapperIgnoreSource(nameof(SysCode.CreateBy))]
        [MapperIgnoreSource(nameof(SysCode.CreateDate))]
        [MapperIgnoreSource(nameof(SysCode.LastUpdateBy))]
        [MapperIgnoreSource(nameof(SysCode.LastUpdateDate))]
        public partial SysCodeResponseDto SysCodeToResponseDto(SysCode src);

        public partial BranchAreaDto BranchAreaToDto(BranchArea src);

        public partial List<SysCodeResponseDto> SysCodeListToDtoList(List<SysCode> src);
        public partial List<BranchAreaDto> BranchAreaListToDtoList(List<BranchArea> src);
        public partial List<FunctionGroupDto> FunctionGroupListToDtoList(List<FunctionGroup> src);

        public partial IQueryable<BranchDto> ProjectToBranchDto(IQueryable<Branch> q);
    }
}
