using Mediator;

namespace SkyLabIdP.Application.SystemApps.SystemInfos.Areas.Queries
{
    public class BranchAreaQuery : IRequest<BranchAreaQueryVM>
    {
        /// <summary>
        /// 行政區編號
        /// </summary>        
        public string AreaID { get; set; } = "";
        /// <summary>
        /// 行政區名稱
        /// </summary>
        public string AreaName { get; set; } = "";
        /// <summary>
        /// 分部區域碼
        /// </summary>    
        public string DstCode { get; set; } = "";
        /// <summary>
        /// 分部簡碼
        /// </summary>
        public string CityCode { get; set; } = "";

        public string LoginUserId { get; set; } = "";
    }
}


