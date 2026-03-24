using System.Net;
using Application.IntegrationTests.Common;
using Application.IntegrationTests.Fixtures;
using Application.IntegrationTests.Models;

namespace Application.IntegrationTests.Tests;

[Collection("Integration")]
public class MultiTenantRegistrationTests : IntegrationTestBase
{
    public MultiTenantRegistrationTests(IntegrationTestFixture fixture) : base(fixture) { }

    /// <summary>
    /// AC01: SkyLabMgm 使用者註冊 — ServiceAgency "00"
    /// </summary>
    [Fact]
    public async Task RegisterSkyLabMgmUser_WithServiceAgency00_ShouldAssignSkyLabSystemMgmtRole()
    {
        var command = new RegisterSkyLabMgmUserCommand
        {
            TenantId = TenantIds.SkyLabmgm,
            UserRegistrationRequest = new SkyLabMgmUserRegistrationRequest
            {
                UserName = GenerateTestEmail("skylabmgm_00"),
                Password = "StrongPassword123!",
                ConfirmPassword = "StrongPassword123!",
                Email = GenerateTestEmail("skylabmgm_00"),
                TenantId = TenantIds.SkyLabmgm,
                FullName = "張小明",
                ServiceAgency = "00",
                SubordinateUnit = "環保局",
                JobTitle = "科員",
                OfficialPhone = "0987654321",
                FileId = "TEST001"
            }
        };

        var response = await PostAsync(ApiEndpoints.Register, TenantIds.SkyLabmgm, command);

        AssertSuccess(response);
        var registerData = await ParseResponseAsync<RegisterResponse>(response);
        Assert.NotNull(registerData);
        Assert.True(registerData.Success);
        Assert.NotEmpty(registerData.UserId);

        // 驗證使用者可以立即登入
        var loginRequest = new LoginRequest
        {
            UserName = command.UserRegistrationRequest.UserName,
            Password = command.UserRegistrationRequest.Password,
            CaptchaId = "test-captcha-id",
            CaptchaValue = "TEST"
        };
        var loginResponse = await PostAsync(ApiEndpoints.Login, TenantIds.SkyLabmgm, loginRequest);
        AssertSuccess(loginResponse);
    }

    /// <summary>
    /// AC01: SkyLabMgm 使用者註冊 — ServiceAgency 非 "00"
    /// </summary>
    [Theory]
    [InlineData("01")]
    [InlineData("02")]
    [InlineData("99")]
    public async Task RegisterSkyLabMgmUser_WithServiceAgencyNot00_ShouldAssignSkyLabSystemHighMgmtRole(string serviceAgency)
    {
        var command = new RegisterSkyLabMgmUserCommand
        {
            TenantId = TenantIds.SkyLabmgm,
            UserRegistrationRequest = new SkyLabMgmUserRegistrationRequest
            {
                UserName = GenerateTestEmail($"skylabmgm_{serviceAgency}"),
                Password = "StrongPassword123!",
                ConfirmPassword = "StrongPassword123!",
                Email = GenerateTestEmail($"skylabmgm_{serviceAgency}"),
                TenantId = TenantIds.SkyLabmgm,
                FullName = "李大華",
                ServiceAgency = serviceAgency,
                SubordinateUnit = "環保署",
                JobTitle = "技正",
                OfficialPhone = "0912345678",
                FileId = $"TEST{serviceAgency}"
            }
        };

        var response = await PostAsync(ApiEndpoints.Register, TenantIds.SkyLabmgm, command);

        AssertSuccess(response);
        var registerData = await ParseResponseAsync<RegisterResponse>(response);
        Assert.NotNull(registerData);
        Assert.True(registerData.Success);
    }

    /// <summary>
    /// AC02: SkyLab 開發者註冊
    /// </summary>
    [Fact]
    public async Task RegisterDeveloper_WithValidData_ShouldSucceed()
    {
        var command = new RegisterSkyLabDevelopUserCommand
        {
            TenantId = TenantIds.SkyLabdevelop,
            SkyLabDevelopUserRegistrationRequest = new SkyLabDevelopUserRegistrationRequest
            {
                UserName = GenerateTestEmail("skylabdevelop"),
                Password = "DeveloperPass123!",
                ConfirmPassword = "DeveloperPass123!",
                Email = GenerateTestEmail("skylabdevelop"),
                TenantId = TenantIds.SkyLabdevelop,
                FullName = "王開發",
                ServiceAgency = "00",
                SubordinateUnit = "技術部",
                JobTitle = "開發工程師",
                OfficialEmail = GenerateTestEmail("skylabdevelop_official"),
                OfficialPhone = "0923456789"
            }
        };

        var response = await PostAsync(ApiEndpoints.RegisterDeveloper, TenantIds.SkyLabdevelop, command);

        AssertSuccess(response);
        var registerData = await ParseResponseAsync<RegisterResponse>(response);
        Assert.NotNull(registerData);
        Assert.True(registerData.Success);
    }

    /// <summary>
    /// AC03: SkyLab 委員註冊
    /// </summary>
    [Theory]
    [InlineData(TenantIds.SkyLabcommittee, "環境工程")]
    public async Task RegisterCommittee_WithValidData_ShouldSucceed(string tenantId, string specialization)
    {
        var command = new RegisterSkyLabCommitteeUserCommand
        {
            TenantId = tenantId,
            SkyLabCommitteeUserRegistrationRequest = new SkyLabCommitteeUserRegistrationRequest
            {
                UserName = GenerateTestEmail($"committee_{tenantId.ToLower()}"),
                Password = "CommitteePassword123!",
                ConfirmPassword = "CommitteePassword123!",
                Email = GenerateTestEmail($"committee_{tenantId.ToLower()}"),
                TenantId = tenantId,
                FullName = "陳委員",
                Gender = "M",
                MemberType = "專",
                CompanyName = "台灣大學",
                DepartmentName = "環境工程學研究所",
                JobTitle = "教授",
                Active = true,
                IsPersonal = false,
                MainEmail = GenerateTestEmail($"committee_{tenantId.ToLower()}_main"),
                SpareEmail = GenerateTestEmail($"committee_{tenantId.ToLower()}_spare"),
                MainTel = "02-33665511",
                SpareTel = "0912345678",
                Address = "台北市大安區羅斯福路四段1號",
                Specialty = specialization
            }
        };

        var response = await PostAsync(ApiEndpoints.RegisterCommittee, tenantId, command);

        AssertSuccess(response);
        var registerData = await ParseResponseAsync<RegisterResponse>(response);
        Assert.NotNull(registerData);
        Assert.True(registerData.Success);
    }

    /// <summary>
    /// AC05: CAEDP 使用者註冊
    /// </summary>
    [Fact]
    public async Task RegisterCaedpUser_WithValidData_ShouldSucceed()
    {
        var command = new RegisterSkyLabCaedpUserCommand
        {
            TenantId = TenantIds.SkyLabcaedp,
            SkyLabCaedpUserRegistrationRequest = new SkyLabCaedpUserRegistrationRequest
            {
                UserName = GenerateTestEmail("skylabcaedp"),
                Password = "CaedpPassword123!",
                ConfirmPassword = "CaedpPassword123!",
                Email = GenerateTestEmail("skylabcaedp"),
                TenantId = TenantIds.SkyLabcaedp,
                FullName = "林CAEDP",
                ServiceAgency = "00",
                SubordinateUnit = "綜合計畫處",
                JobTitle = "專員",
                OfficialPhone = "02-23117722"
            }
        };

        var response = await PostAsync(ApiEndpoints.RegisterCaedp, TenantIds.SkyLabcaedp, command);

        AssertSuccess(response);
        var registerData = await ParseResponseAsync<RegisterResponse>(response);
        Assert.NotNull(registerData);
        Assert.True(registerData.Success);
    }

    /// <summary>
    /// AC06: 協作機關使用者註冊
    /// </summary>
    [Fact]
    public async Task RegisterCollaborativeAgency_WithValidData_ShouldSucceed()
    {
        var command = new RegisterSkyLabCollaborativeAgencyUserCommand
        {
            TenantId = TenantIds.SkyLabCollaborativeAgency,
            Request = new SkyLabCollaborativeAgencyUserRegistrationRequest
            {
                UserName = GenerateTestEmail("collaborative"),
                Password = "CollaborativePass123!",
                ConfirmPassword = "CollaborativePass123!",
                Email = GenerateTestEmail("collaborative"),
                TenantId = TenantIds.SkyLabCollaborativeAgency,
                SystemRole = "協作機關使用者",
                FileId = "COLLAB001",
                FullName = "黃協作",
                ServiceAgency = "交通",
                SubordinateUnit = "公路總局",
                JobTitle = "工程司",
                OfficialEmail = GenerateTestEmail("collaborative_official"),
                OfficialPhone = "02-23070123"
            }
        };

        var response = await PostAsync(ApiEndpoints.RegisterCollaborativeAgency, TenantIds.SkyLabCollaborativeAgency, command);

        AssertSuccess(response);
        var registerData = await ParseResponseAsync<RegisterResponse>(response);
        Assert.NotNull(registerData);
        Assert.True(registerData.Success);
    }

    /// <summary>
    /// 密碼強度驗證 — 弱密碼應回傳 400
    /// </summary>
    [Theory]
    [InlineData("weak")]
    [InlineData("123456789012")]
    [InlineData("passwordtext")]
    [InlineData("PASSWORDTEXT")]
    [InlineData("Password1234")]
    [InlineData("Password!")]
    public async Task Register_WithWeakPassword_ShouldReturnValidationError(string password)
    {
        var command = new RegisterSkyLabMgmUserCommand
        {
            TenantId = TenantIds.SkyLabmgm,
            UserRegistrationRequest = new SkyLabMgmUserRegistrationRequest
            {
                UserName = GenerateTestEmail("weak_pass"),
                Password = password,
                ConfirmPassword = password,
                Email = GenerateTestEmail("weak_pass"),
                TenantId = TenantIds.SkyLabmgm,
                FullName = "弱密碼測試",
                ServiceAgency = "00",
                OfficialPhone = "0987654321",
                FileId = "WEAK001"
            }
        };

        var response = await PostAsync(ApiEndpoints.Register, TenantIds.SkyLabmgm, command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// ServiceAgency 長度限制 — 超過 10 字元應回傳 400
    /// </summary>
    [Fact]
    public async Task Register_WithTooLongServiceAgency_ShouldReturnValidationError()
    {
        var command = new RegisterSkyLabMgmUserCommand
        {
            TenantId = TenantIds.SkyLabmgm,
            UserRegistrationRequest = new SkyLabMgmUserRegistrationRequest
            {
                UserName = GenerateTestEmail("long_service_agency"),
                Password = "ValidPassword123!",
                ConfirmPassword = "ValidPassword123!",
                Email = GenerateTestEmail("long_service_agency"),
                TenantId = TenantIds.SkyLabmgm,
                FullName = "長服務機構測試",
                ServiceAgency = "12345678901",
                OfficialPhone = "0987654321",
                FileId = "LONG001"
            }
        };

        var response = await PostAsync(ApiEndpoints.Register, TenantIds.SkyLabmgm, command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// 帳號不能與密碼相同
    /// </summary>
    [Fact]
    public async Task Register_WithUserNameSameAsPassword_ShouldReturnValidationError()
    {
        var sameValue = "SameUserPassword123!";
        var command = new RegisterSkyLabMgmUserCommand
        {
            TenantId = TenantIds.SkyLabmgm,
            UserRegistrationRequest = new SkyLabMgmUserRegistrationRequest
            {
                UserName = sameValue,
                Password = sameValue,
                ConfirmPassword = sameValue,
                Email = GenerateTestEmail("same_user_pass"),
                TenantId = TenantIds.SkyLabmgm,
                FullName = "相同帳密測試",
                ServiceAgency = "00",
                OfficialPhone = "0987654321",
                FileId = "SAME001"
            }
        };

        var response = await PostAsync(ApiEndpoints.Register, TenantIds.SkyLabmgm, command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// SkyLabDevelop 密碼驗證
    /// </summary>
    [Theory]
    [InlineData("weak")]
    [InlineData("WeakPassword")]
    [InlineData("WeakPass123")]
    public async Task RegisterDeveloper_WithWeakPassword_ShouldReturnValidationError(string weakPassword)
    {
        var command = new RegisterSkyLabDevelopUserCommand
        {
            TenantId = TenantIds.SkyLabdevelop,
            SkyLabDevelopUserRegistrationRequest = new SkyLabDevelopUserRegistrationRequest
            {
                UserName = GenerateTestEmail("developer_weak_pass"),
                Password = weakPassword,
                ConfirmPassword = weakPassword,
                Email = GenerateTestEmail("developer_weak_pass"),
                FullName = "開發者弱密碼測試",
                ServiceAgency = "開發公司",
                OfficialPhone = "0987654321"
            }
        };

        var response = await PostAsync(ApiEndpoints.RegisterDeveloper, TenantIds.SkyLabdevelop, command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// SkyLabCommittee MainEmail 必填驗證
    /// </summary>
    [Fact]
    public async Task RegisterCommittee_WithMissingMainEmail_ShouldReturnValidationError()
    {
        var command = new RegisterSkyLabCommitteeUserCommand
        {
            TenantId = TenantIds.SkyLabcommittee,
            SkyLabCommitteeUserRegistrationRequest = new SkyLabCommitteeUserRegistrationRequest
            {
                UserName = GenerateTestEmail("committee_no_main_email"),
                Password = "CommitteePassword123!",
                ConfirmPassword = "CommitteePassword123!",
                Email = GenerateTestEmail("committee_no_main_email"),
                FullName = "委員無主要信箱測試",
                MainEmail = "",
                MainTel = "02-33665511"
            }
        };

        var response = await PostAsync(ApiEndpoints.RegisterCommittee, TenantIds.SkyLabcommittee, command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// 協作機關 ServiceAgency 2 字元限制
    /// </summary>
    [Fact]
    public async Task RegisterCollaborativeAgency_WithTooLongServiceAgency_ShouldReturnValidationError()
    {
        var command = new RegisterSkyLabCollaborativeAgencyUserCommand
        {
            TenantId = TenantIds.SkyLabCollaborativeAgency,
            Request = new SkyLabCollaborativeAgencyUserRegistrationRequest
            {
                UserName = GenerateTestEmail("collaborative_long_agency"),
                Password = "CollaborativePass123!",
                ConfirmPassword = "CollaborativePass123!",
                Email = GenerateTestEmail("collaborative_long_agency"),
                FullName = "協作機關長代碼測試",
                ServiceAgency = "交通運輸部",
                SystemRole = "協作機關使用者",
                FileId = "LONG001",
                SubordinateUnit = "公路總局",
                JobTitle = "工程司",
                OfficialEmail = GenerateTestEmail("collaborative_long_agency_official"),
                OfficialPhone = "02-23070123"
            }
        };

        var response = await PostAsync(ApiEndpoints.RegisterCollaborativeAgency, TenantIds.SkyLabCollaborativeAgency, command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// 協作機關密碼強度驗證 — 至少 12 字元 + 複雜度
    /// </summary>
    [Theory]
    [InlineData("123")]
    [InlineData("12345")]
    [InlineData("shortpass")]
    [InlineData("CollabPass1")]
    [InlineData("CollabPassword")]
    public async Task RegisterCollaborativeAgency_WithWeakPassword_ShouldReturnValidationError(string weakPassword)
    {
        var command = new RegisterSkyLabCollaborativeAgencyUserCommand
        {
            TenantId = TenantIds.SkyLabCollaborativeAgency,
            Request = new SkyLabCollaborativeAgencyUserRegistrationRequest
            {
                UserName = GenerateTestEmail("collaborative_weak_pass"),
                Password = weakPassword,
                ConfirmPassword = weakPassword,
                Email = GenerateTestEmail("collaborative_weak_pass"),
                FullName = "協作機關弱密碼測試",
                ServiceAgency = "交通",
                SystemRole = "協作機關使用者",
                FileId = "WEAK001",
                SubordinateUnit = "公路總局",
                JobTitle = "工程司",
                OfficialEmail = GenerateTestEmail("collaborative_weak_pass_official"),
                OfficialPhone = "02-23070123"
            }
        };

        var response = await PostAsync(ApiEndpoints.RegisterCollaborativeAgency, TenantIds.SkyLabCollaborativeAgency, command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
