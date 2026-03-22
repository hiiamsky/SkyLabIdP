namespace PlaywrightTests.Models;

/// <summary>
/// 使用者登入請求模型
/// </summary>
public class LoginRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? CaptchaId { get; set; }
    public string? CaptchaValue { get; set; }
}

/// <summary>
/// 使用者登入回應模型
/// </summary>
public class LoginResponse
{
    public bool Success { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserInfo? User { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
}

/// <summary>
/// 使用者資訊模型
/// </summary>
public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public List<string>? Roles { get; set; }
    public Dictionary<string, object>? Claims { get; set; }
}

/// <summary>
/// SkyLabMgm 使用者註冊 Command
/// </summary>
public class RegisterSkyLabMgmUserCommand
{
    public SkyLabMgmUserRegistrationRequest UserRegistrationRequest { get; set; } = new();
    public string TenantId { get; set; } = string.Empty;
}

/// <summary>
/// SkyLabMgm 使用者註冊請求模型
/// </summary>
public class SkyLabMgmUserRegistrationRequest
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string ServiceAgency { get; set; } = string.Empty;
    public string SubordinateUnit { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string OfficialPhone { get; set; } = string.Empty;
    public string FileId { get; set; } = string.Empty;
}

/// <summary>
/// SkyLabDevelop 使用者註冊 Command
/// </summary>
public class RegisterSkyLabDevelopUserCommand
{
    public SkyLabDevelopUserRegistrationRequest SkyLabDevelopUserRegistrationRequest { get; set; } = new();
    public string TenantId { get; set; } = string.Empty;
}

/// <summary>
/// SkyLabDevelop 使用者註冊請求模型
/// </summary>
public class SkyLabDevelopUserRegistrationRequest
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string ServiceAgency { get; set; } = string.Empty;
    public string SubordinateUnit { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string OfficialEmail { get; set; } = string.Empty;
    public string OfficialPhone { get; set; } = string.Empty;
}

/// <summary>
/// SkyLabCommittee 使用者註冊 Command
/// </summary>
public class RegisterSkyLabCommitteeUserCommand
{
    public SkyLabCommitteeUserRegistrationRequest SkyLabCommitteeUserRegistrationRequest { get; set; } = new();
    public string TenantId { get; set; } = string.Empty;
}

/// <summary>
/// SkyLabCommittee 使用者註冊請求模型
/// </summary>
public class SkyLabCommitteeUserRegistrationRequest
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string MemberType { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public bool Active { get; set; } = true;
    public bool IsPersonal { get; set; } = false;
    public int GroupSort { get; set; } = 0;
    public string MainEmail { get; set; } = string.Empty;
    public string SpareEmail { get; set; } = string.Empty;
    public string MainTel { get; set; } = string.Empty;
    public string SpareTel { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
}

/// <summary>
/// SkyLabCaedp 使用者註冊 Command
/// </summary>
public class RegisterSkyLabCaedpUserCommand
{
    public SkyLabCaedpUserRegistrationRequest SkyLabCaedpUserRegistrationRequest { get; set; } = new();
    public string TenantId { get; set; } = string.Empty;
}

/// <summary>
/// SkyLabCaedp 使用者註冊請求模型
/// </summary>
public class SkyLabCaedpUserRegistrationRequest
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string ServiceAgency { get; set; } = string.Empty;
    public string SubordinateUnit { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string OfficialEmail { get; set; } = string.Empty;
    public string OfficialPhone { get; set; } = string.Empty;
}

/// <summary>
/// SkyLabCollaborativeAgency 使用者註冊 Command
/// </summary>
public class RegisterSkyLabCollaborativeAgencyUserCommand
{
    public SkyLabCollaborativeAgencyUserRegistrationRequest Request { get; set; } = new();
    public string TenantId { get; set; } = string.Empty;
}

/// <summary>
/// SkyLabCollaborativeAgency 使用者註冊請求模型
/// </summary>
public class SkyLabCollaborativeAgencyUserRegistrationRequest
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string SystemRole { get; set; } = string.Empty;
    public string FileId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string ServiceAgency { get; set; } = string.Empty;
    public string SubordinateUnit { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string OfficialEmail { get; set; } = string.Empty;
    public string OfficialPhone { get; set; } = string.Empty;
    public string? MoicaCardNumber { get; set; }
}

/// <summary>
/// 使用者註冊請求模型（舊版，向後相容）
/// </summary>
public class RegisterRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? ServiceAgency { get; set; }
    public string? CaptchaId { get; set; }
    public string? CaptchaValue { get; set; }
}

/// <summary>
/// SkyLab開發者註冊請求模型
/// </summary>
public class RegisterDeveloperRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public string? CaptchaId { get; set; }
    public string? CaptchaValue { get; set; }
}

/// <summary>
/// SkyLab委員註冊請求模型
/// </summary>
public class RegisterCommitteeRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string? CaptchaId { get; set; }
    public string? CaptchaValue { get; set; }
}

/// <summary>
/// 使用者註冊回應模型
/// </summary>
public class RegisterResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; } = string.Empty;
    public OperationResult OperationResult { get; set; } = new();
    
    // 方便測試使用的屬性
    public bool Success => OperationResult?.Success ?? false;
    public string Message => OperationResult?.Messages?.FirstOrDefault() ?? string.Empty;
}

/// <summary>
/// 操作結果模型
/// </summary>
public class OperationResult
{
    public bool Success { get; set; }
    public List<string> Messages { get; set; } = new();
    public int StatusCode { get; set; }
    public object? Data { get; set; }
}

/// <summary>
/// 令牌刷新請求模型
/// </summary>
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// 令牌刷新回應模型
/// </summary>
public class RefreshTokenResponse
{
    public bool Success { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
}

/// <summary>
/// 登出請求模型
/// </summary>
public class LogoutRequest
{
    public string? RefreshToken { get; set; }
}

/// <summary>
/// 標準 API 回應模型
/// </summary>
public class ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
    public object? Data { get; set; }
}

/// <summary>
/// JWKS 回應模型
/// </summary>
public class JwksResponse
{
    public List<JwkKey>? Keys { get; set; }
}

/// <summary>
/// JSON Web Key 模型
/// </summary>
public class JwkKey
{
    public string Kty { get; set; } = string.Empty;  // Key Type
    public string Use { get; set; } = string.Empty;  // Key Use
    public string Kid { get; set; } = string.Empty;  // Key ID
    public string N { get; set; } = string.Empty;    // RSA Modulus
    public string E { get; set; } = string.Empty;    // RSA Exponent
    public string Alg { get; set; } = string.Empty;  // Algorithm
}

/// <summary>
/// 驗證碼產生回應模型
/// </summary>
public class CaptchaResponse
{
    public bool Success { get; set; }
    public string CaptchaId { get; set; } = string.Empty;
    public string CaptchaImageBase64 { get; set; } = string.Empty;
    public string? Message { get; set; }
}

/// <summary>
/// 檔案上傳回應模型
/// </summary>
public class FileUploadResponse
{
    public bool Success { get; set; }
    public string FileId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
}

/// <summary>
/// 帳戶搜尋請求模型
/// </summary>
public class AccountSearchRequest
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? TenantId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// 分頁結果模型
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}

/// <summary>
/// SkyLabCommitteeAssistant 使用者註冊 Command
/// </summary>
public class RegisterSkyLabCommitteeAssistantCommand
{
    public SkyLabCommitteeAssistantRegistrationRequest UserRegistrationRequest { get; set; } = new();
    public string TenantId { get; set; } = string.Empty;
}

/// <summary>
/// SkyLabCommitteeAssistant 使用者註冊請求模型
/// </summary>
public class SkyLabCommitteeAssistantRegistrationRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string MainEmail { get; set; } = string.Empty;
    public string MainTel { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
}

/// <summary>
/// 錯誤回應模型
/// </summary>
public class ErrorResponse
{
    public string? Type { get; set; }
    public string? Title { get; set; }
    public int Status { get; set; }
    public string? Detail { get; set; }
    public string? Instance { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}

/// <summary>
/// 系統資訊回應模型
/// </summary>
public class SystemInfoResponse
{
    public bool Success { get; set; }
    public Dictionary<string, object>? Data { get; set; }
    public string? Message { get; set; }
}
