using System.Text.Json;

namespace PlaywrightTests.Common;

/// <summary>
/// SKYLAB IdP API 測試的基礎配置和常數
/// </summary>
public static class ApiConfig
{
    /// <summary>
    /// API 基礎 URL（容器化環境）
    /// </summary>
    public const string BaseUrl = "http://localhost:8083";
    
    /// <summary>
    /// API 版本路徑前綴
    /// </summary>
    public const string ApiPrefix = "/skylabidp/api/v1";
    
    /// <summary>
    /// 完整 API 基礎 URL
    /// </summary>
    public static string FullApiUrl => $"{BaseUrl}{ApiPrefix}";
    
    /// <summary>
    /// API 金鑰環境變數名稱
    /// </summary>
    public const string ApiKeyEnvVar = "SKYLABIDP_APIKEY";
    
    /// <summary>
    /// 預設請求超時時間（秒）
    /// </summary>
    public const int DefaultTimeoutSeconds = 30;
    
    /// <summary>
    /// JSON 序列化選項
    /// </summary>
    public static JsonSerializerOptions JsonOptions => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
}

/// <summary>
/// 支援的租戶類型
/// </summary>
public static class TenantIds
{
    public const string SkyLabmgm = "SkyLabmgm";
    public const string SkyLabcaedp = "SkyLabcaedp";
    public const string SkyLabdevelop = "SkyLabdevelop";
    public const string SkyLabcommittee = "SkyLabcommittee";
    public const string SkyLabcommitteeAssistant = "SkyLabcommitteeAssistant";
    public const string SkyLabCollaborativeAgency = "SkyLabCollaborativeAgency";
    
    /// <summary>
    /// 所有有效的租戶 ID
    /// </summary>
    public static readonly string[] All = 
    {
        SkyLabmgm, SkyLabcaedp, SkyLabdevelop, 
        SkyLabcommittee, SkyLabcommitteeAssistant, SkyLabCollaborativeAgency
    };
}

/// <summary>
/// 常用的 HTTP 標頭名稱
/// </summary>
public static class Headers
{
    public const string TenantId = "X-Tenant-Id";
    public const string ApiKey = "X-API-key";
    public const string Authorization = "Authorization";
    public const string ContentType = "Content-Type";
}

/// <summary>
/// API 端點路徑
/// </summary>
public static class Endpoints
{
    // 認證相關
    public const string Login = "/Users/login";
    public const string RefreshToken = "/Users/refresh-token";
    public const string Logout = "/Users/logout";
    public const string Jwks = "/Jwks/.well-known/jwks.json";
    
    // 使用者註冊 - 各租戶專用端點
    public const string Register = "/Users/register";                           // SkyLabMgm 
    public const string RegisterDeveloper = "/Users/register-skylab-developer";    // SkyLabDevelop
    public const string RegisterCommittee = "/Users/register-skylab-committee";    // SkyLabCommittee
    public const string RegisterCaedp = "/Users/register-skylab-caedp";           // SkyLabCaedp
    public const string RegisterCommitteeAssistant = "/Users/register-skylab-committee-assistant"; // SkyLabCommitteeAssistant
    public const string RegisterCollaborativeAgency = "/Users/register-skylab-external-agency";    // SkyLabCollaborativeAgency
    
    // 系統資訊
    public const string ServiceAgency = "/SystemInfos/serviceagency";
    public const string Menu = "/SystemInfos/menu";
    public const string GenerateCaptcha = "/SystemInfos/generate-captcha";
    
    // 系統管理
    public const string AcctMgmtSearch = "/SystemAdministration/AcctMgmt/Search";
    public const string AcctMgmtAccounts = "/SystemAdministration/AcctMgmt/accounts";
    
    // 檔案管理
    public const string UploadFile = "/Files/register/upload-skylabdocuserdetail-file";
    public const string ReUploadFile = "/Files/register/{userid}/re-upload-skylabdocuserdetail-file";
    
    // 外部認證
    public const string ExternalLogin = "/ExternalAuth/login/{provider}";
    public const string ExternalCallback = "/ExternalAuth/callback";
    public const string CompleteRegistration = "/ExternalAuth/complete-registration";
}

/// <summary>
/// 測試用的使用者資料
/// </summary>
public static class TestUsers
{
    public static class SkyLabmgm
    {
        public const string UserName = "skylabmgm_test@example.com";
        public const string Password = "SkyLabMgm123!";
        public const string TenantId = TenantIds.SkyLabmgm;
        public const string ExpectedRole = "SkyLabSystemMgmt";
    }
    
    public static class Committee
    {
        public const string UserName = "committee_test@example.com";
        public const string Password = "Committee123!";
        public const string TenantId = TenantIds.SkyLabcommittee;
        public const string ExpectedRole = "CommitteeMember";
    }
    
    public static class Developer
    {
        public const string UserName = "develop_test@example.com";
        public const string Password = "Develop123!";
        public const string TenantId = TenantIds.SkyLabdevelop;
        public const string ExpectedRole = "Developer";
    }
    
    public static class Caedp
    {
        public const string UserName = "caedp_test@example.com";
        public const string Password = "Caedp123!";
        public const string TenantId = TenantIds.SkyLabcaedp;
        public const string ExpectedRole = "CaedpUser";
    }
}

/// <summary>
/// HTTP 狀態碼常數
/// </summary>
public static class HttpStatusCodes
{
    public const int Ok = 200;
    public const int BadRequest = 400;
    public const int Unauthorized = 401;
    public const int Forbidden = 403;
    public const int NotFound = 404;
    public const int Conflict = 409;
    public const int InternalServerError = 500;
}
