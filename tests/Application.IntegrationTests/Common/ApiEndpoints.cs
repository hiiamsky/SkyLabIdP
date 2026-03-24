namespace Application.IntegrationTests.Common;

public static class ApiEndpoints
{
    public const string ApiPrefix = "/skylabidp/api/v1";

    // 認證相關
    public const string Login = "/Users/login";
    public const string RefreshToken = "/Users/refresh-token";
    public const string Logout = "/Users/logout";
    public const string Jwks = "/Jwks/.well-known/jwks.json";

    // 使用者註冊
    public const string Register = "/Users/register";
    public const string RegisterDeveloper = "/Users/register-skylab-developer";
    public const string RegisterCommittee = "/Users/register-skylab-committee";
    public const string RegisterCaedp = "/Users/register-skylab-caedp";
    public const string RegisterCommitteeAssistant = "/Users/register-skylab-committee-assistant";
    public const string RegisterCollaborativeAgency = "/Users/register-skylab-external-agency";

    // 系統管理
    public const string AcctMgmtSearch = "/SystemAdministration/AcctMgmt/Search";
}

public static class TenantIds
{
    public const string SkyLabmgm = "SkyLabmgm";
    public const string SkyLabcaedp = "SkyLabcaedp";
    public const string SkyLabdevelop = "SkyLabdevelop";
    public const string SkyLabcommittee = "SkyLabcommittee";
    public const string SkyLabcommitteeAssistant = "SkyLabcommitteeAssistant";
    public const string SkyLabCollaborativeAgency = "SkyLabCollaborativeAgency";

    public static readonly string[] All =
    [
        SkyLabmgm, SkyLabcaedp, SkyLabdevelop,
        SkyLabcommittee, SkyLabcommitteeAssistant, SkyLabCollaborativeAgency
    ];
}

public static class TestUsers
{
    public static class SkyLabmgm
    {
        public const string UserName = "skylabmgm_test@example.com";
        public const string Password = "SkyLabMgm123!";
        public const string TenantId = TenantIds.SkyLabmgm;
    }

    public static class Committee
    {
        public const string UserName = "committee_test@example.com";
        public const string Password = "Committee123!";
        public const string TenantId = TenantIds.SkyLabcommittee;
    }

    public static class Developer
    {
        public const string UserName = "develop_test@example.com";
        public const string Password = "Develop123!";
        public const string TenantId = TenantIds.SkyLabdevelop;
    }

    public static class Caedp
    {
        public const string UserName = "caedp_test@example.com";
        public const string Password = "Caedp123!";
        public const string TenantId = TenantIds.SkyLabcaedp;
    }
}
