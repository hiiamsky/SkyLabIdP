namespace SkyLabIdP.Application.Dtos.Function
{
    public class FunctionDto
    {
        public string GroupID { get; set; } = "";
        public string FunctionID { get; set; } = "";

        public string FunctionIcon { get; set; } = "";
        public string TargetRoute { get; set; } = "";
        public string FunctionEnglishDescription { get; set; } = "";
        public string FunctionChineseDescription { get; set; } = "";

        public int FunctionOrder { get; set; } = 0;

        public PermissionSet Permissions { get; set; } = new PermissionSet();
    }

    public class PermissionSet
    {

        public string Read { get; set; } = "";

        public string Search { get; set; } = "";

        public string Create { get; set; } = "";
        public string Update { get; set; } = "";

        public string Delete { get; set; } = "";

        public string Upload { get; set; } = "";

        public string Download { get; set; } = "";

        public string Import { get; set; } = "";
        public string Export { get; set; } = "";

    }
}
