using SkyLabIdP.Application.Dtos.Function;

namespace SkyLabIdP.Application.Dtos.FunctionGroup
{
    public class FunctionGroupDto
    {
        public string GroupID { get; set; } = "";
        public string GroupTitle { get; set; } = "";
        public string GroupIcon { get; set; } = "";
        public string GroupEnglishDescription { get; set; } = "";
        public string GroupChineseDescription { get; set; } = "";
        public string TargetRoute { get; set; } = "";

        public bool IsOpenFunctionList { get; set; } = true;

        public int GroupOrder { get; set; }

        public List<FunctionDto> Functions { get; set; } = new List<FunctionDto>();
    }

}
