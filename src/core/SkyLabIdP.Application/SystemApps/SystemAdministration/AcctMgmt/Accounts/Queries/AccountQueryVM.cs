using SkyLabIdP.Application.Dtos;
using SkyLabIdP.Application.Dtos.SkyLabDocUserDetail;

namespace SkyLabIdP.Application.SystemApps.SystemAdministration.AcctMgmt.Accounts.Queries
{
    public class AccountQueryVM
    {
        public List<SkyLabDocUserDetailDto> SkyLabDocUserDetailDtos { get; set; } = new List<SkyLabDocUserDetailDto>();


        // 分頁屬性
        public int CurrentPage { get; set; } = 1; // 當前頁碼
        public int PageSize { get; set; } = 10;  // 每頁顯示的記錄數
        public int TotalRecords { get; set; }    // 總記錄數
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);  // 總頁數，計算得出

        public OperationResult operationResult { get; set; } = new OperationResult(false, "", 400);
    }
}

