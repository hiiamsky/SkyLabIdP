namespace SkyLabIdP.Application.Dtos.BranchArea
{
    public class BranchAreaDto
    {
        /// <summary>
        /// 行政區編號
        /// </summary>

        public string AreaId { get; set; } = string.Empty;

        /// <summary>
        /// 行政區名稱
        /// </summary>

        public string AreaName { get; set; } = string.Empty;

        /// <summary>
        /// 行政區編號_02
        /// </summary>

        public string AreaId2 { get; set; } = string.Empty;

        /// <summary>
        /// 分部區域碼
        /// </summary>

        public string DstCode { get; set; } = string.Empty;

        /// <summary>
        /// 是否顯示
        /// </summary>
        public bool IsDisplayed { get; set; } = false;

        /// <summary>
        /// 合併分部碼
        /// </summary>

        public string RelDstCode { get; set; } = string.Empty;

        /// <summary>
        /// 分部簡碼
        /// </summary>

        public string CityCode { get; set; } = string.Empty;


    }
}


