namespace Application.IntegrationTests.Models;

public class LoginRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? CaptchaId { get; set; }
    public string? CaptchaValue { get; set; }
}

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

public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public List<string>? Roles { get; set; }
    public Dictionary<string, object>? Claims { get; set; }
}

public class RegisterSkyLabMgmUserCommand
{
    public SkyLabMgmUserRegistrationRequest UserRegistrationRequest { get; set; } = new();
    public string TenantId { get; set; } = string.Empty;
}

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

public class RegisterSkyLabDevelopUserCommand
{
    public SkyLabDevelopUserRegistrationRequest SkyLabDevelopUserRegistrationRequest { get; set; } = new();
    public string TenantId { get; set; } = string.Empty;
}

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

public class RegisterSkyLabCommitteeUserCommand
{
    public SkyLabCommitteeUserRegistrationRequest SkyLabCommitteeUserRegistrationRequest { get; set; } = new();
    public string TenantId { get; set; } = string.Empty;
}

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

public class RegisterSkyLabCaedpUserCommand
{
    public SkyLabCaedpUserRegistrationRequest SkyLabCaedpUserRegistrationRequest { get; set; } = new();
    public string TenantId { get; set; } = string.Empty;
}

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

public class RegisterSkyLabCollaborativeAgencyUserCommand
{
    public SkyLabCollaborativeAgencyUserRegistrationRequest Request { get; set; } = new();
    public string TenantId { get; set; } = string.Empty;
}

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

public class RegisterSkyLabCommitteeAssistantCommand
{
    public SkyLabCommitteeAssistantRegistrationRequest UserRegistrationRequest { get; set; } = new();
    public string TenantId { get; set; } = string.Empty;
}

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

public class RegisterResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; } = string.Empty;
    public OperationResult OperationResult { get; set; } = new();

    public bool Success => OperationResult?.Success ?? false;
    public string Message => OperationResult?.Messages?.FirstOrDefault() ?? string.Empty;
}

public class OperationResult
{
    public bool Success { get; set; }
    public List<string> Messages { get; set; } = new();
    public int StatusCode { get; set; }
    public object? Data { get; set; }
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenResponse
{
    public bool Success { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
}

public class LogoutRequest
{
    public string? RefreshToken { get; set; }
}

public class ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
    public object? Data { get; set; }
}

public class JwksResponse
{
    public List<JwkKey>? Keys { get; set; }
}

public class JwkKey
{
    public string Kty { get; set; } = string.Empty;
    public string Use { get; set; } = string.Empty;
    public string Kid { get; set; } = string.Empty;
    public string N { get; set; } = string.Empty;
    public string E { get; set; } = string.Empty;
    public string Alg { get; set; } = string.Empty;
}

public class CaptchaResponse
{
    public bool Success { get; set; }
    public string CaptchaId { get; set; } = string.Empty;
    public string CaptchaImageBase64 { get; set; } = string.Empty;
    public string? Message { get; set; }
}

public class ErrorResponse
{
    public string? Type { get; set; }
    public string? Title { get; set; }
    public int Status { get; set; }
    public string? Detail { get; set; }
    public string? Instance { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}
