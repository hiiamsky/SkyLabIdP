using PlaywrightTests.Common;
using PlaywrightTests.Models;

namespace PlaywrightTests.Tests;

/// <summary>
/// 多租戶使用者註冊的 E2E 測試
/// 基於使用者故事: 02_Multi_Tenant_Registration.md
/// 所有註冊測試都已根據對應的 CommandValidator 驗證規則進行調整
/// </summary>
[Collection("Sequential")]
public class MultiTenantRegistrationTests : ApiTestBase
{
    /// <summary>
    /// AC01: SkyLabMgm 使用者註冊 - ServiceAgency "00"
    /// 基於 RegisterSkyLabMgmUserCommandValidator 規則
    /// </summary>
    [Fact]
    public async Task RegisterSkyLabMgmUser_WithServiceAgency00_ShouldAssignSkyLabSystemMgmtRole()
    {
        // Arrange
        var command = new RegisterSkyLabMgmUserCommand
        {
            TenantId = TenantIds.SkyLabmgm,
            UserRegistrationRequest = new SkyLabMgmUserRegistrationRequest
            {
                UserName = GenerateTestEmail("skylabmgm_00"),
                Password = "StrongPassword123!",  // 17個字符，符合12字符最小要求和複雜度
                ConfirmPassword = "StrongPassword123!",
                Email = GenerateTestEmail("skylabmgm_00"),
                TenantId = TenantIds.SkyLabmgm,
                FullName = "張小明",                    // 必填
                ServiceAgency = "00",                   // 必填，符合10字符限制
                SubordinateUnit = "環保局",
                JobTitle = "科員",
                OfficialPhone = "0987654321",           // 必填
                FileId = "TEST001"                      // 必填
            }
        };

        // Act
        var response = await PostAsync(Endpoints.Register, TenantIds.SkyLabmgm, command);

        // Assert
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

        var loginResponse = await PostAsync(Endpoints.Login, TenantIds.SkyLabmgm, loginRequest);
        AssertSuccess(loginResponse);
    }

    /// <summary>
    /// AC01: SkyLabMgm 使用者註冊 - ServiceAgency 非 "00"
    /// 基於 RegisterSkyLabMgmUserCommandValidator 規則
    /// </summary>
    [Theory]
    [InlineData("01")]
    [InlineData("02")]
    [InlineData("99")]
    public async Task RegisterSkyLabMgmUser_WithServiceAgencyNot00_ShouldAssignSkyLabSystemHighMgmtRole(string serviceAgency)
    {
        // Arrange
        var command = new RegisterSkyLabMgmUserCommand
        {
            TenantId = TenantIds.SkyLabmgm,
            UserRegistrationRequest = new SkyLabMgmUserRegistrationRequest
            {
                UserName = GenerateTestEmail($"skylabmgm_{serviceAgency}"),
                Password = "StrongPassword123!",        // 17個字符，符合驗證要求
                ConfirmPassword = "StrongPassword123!",
                Email = GenerateTestEmail($"skylabmgm_{serviceAgency}"),
                TenantId = TenantIds.SkyLabmgm,
                FullName = "李大華",                    // 必填
                ServiceAgency = serviceAgency,          // 必填，符合10字符限制
                SubordinateUnit = "環保署",
                JobTitle = "技正",
                OfficialPhone = "0912345678",           // 必填
                FileId = $"TEST{serviceAgency}"         // 必填
            }
        };

        // Act
        var response = await PostAsync(Endpoints.Register, TenantIds.SkyLabmgm, command);

        // Assert
        AssertSuccess(response);
        
        var registerData = await ParseResponseAsync<RegisterResponse>(response);
        Assert.NotNull(registerData);
        Assert.True(registerData.Success);
    }

    /// <summary>
    /// AC02: SkyLab開發者註冊
    /// 基於 RegisterSkyLabDevelopUserCommandValidator 規則
    /// 密碼：至少12字符 + 大小寫字母 + 數字 + 特殊字符
    /// ServiceAgency：最大10字符 + 必填
    /// </summary>
    [Fact]
    public async Task RegisterDeveloper_WithValidData_ShouldSucceed()
    {
        // Arrange
        var command = new RegisterSkyLabDevelopUserCommand
        {
            TenantId = TenantIds.SkyLabdevelop,
            SkyLabDevelopUserRegistrationRequest = new SkyLabDevelopUserRegistrationRequest
            {
                UserName = GenerateTestEmail("skylabdevelop"),
                Password = "DeveloperPass123!",         // 16個字符，符合12字符最小要求和複雜度
                ConfirmPassword = "DeveloperPass123!",
                Email = GenerateTestEmail("skylabdevelop"), // 必填且需唯一
                TenantId = TenantIds.SkyLabdevelop,
                FullName = "王開發",                     // 必填
                ServiceAgency = "00",             // 必填，符合10字符限制 (7字符)
                SubordinateUnit = "技術部",
                JobTitle = "開發工程師",
                OfficialEmail = GenerateTestEmail("skylabdevelop_official"), // 對應Email驗證
                OfficialPhone = "0923456789"            // 必填
            }
        };

        // Act
        var response = await PostAsync(Endpoints.RegisterDeveloper, TenantIds.SkyLabdevelop, command);

        // Assert
        AssertSuccess(response);
        
        var registerData = await ParseResponseAsync<RegisterResponse>(response);
        Assert.NotNull(registerData);
        Assert.True(registerData.Success);
    }

    /// <summary>
    /// AC03: SkyLab委員註冊
    /// 基於 RegisterSkyLabCommitteeUserCommandValidator 規則
    /// MainEmail: 必填且需唯一
    /// MainTel: 必填
    /// 密碼：至少12字符 + 複雜度要求
    /// SpareEmail: 可選但需有效格式
    /// </summary>
    [Theory]
    [InlineData(TenantIds.SkyLabcommittee, "環境工程")]
   
    public async Task RegisterCommittee_WithValidData_ShouldSucceed(string tenantId, string specialization)
    {
        // Arrange
        var command = new RegisterSkyLabCommitteeUserCommand
        {
            TenantId = tenantId,
            SkyLabCommitteeUserRegistrationRequest = new SkyLabCommitteeUserRegistrationRequest
            {
                UserName = GenerateTestEmail($"committee_{tenantId.ToLower()}"),
                Password = "CommitteePassword123!",     // 20個字符，符合12字符最小要求和複雜度
                ConfirmPassword = "CommitteePassword123!",
                Email = GenerateTestEmail($"committee_{tenantId.ToLower()}"),
                TenantId = tenantId,
                FullName = "陳委員",                     // 必填
                Gender = "M",
                MemberType = "專",                       // 修正：資料庫欄位長度限制為1字符
                CompanyName = "台灣大學",
                DepartmentName = "環境工程學研究所",
                JobTitle = "教授",
                Active = true,
                IsPersonal = false,
                MainEmail = GenerateTestEmail($"committee_{tenantId.ToLower()}_main"),  // 必填且需唯一
                SpareEmail = GenerateTestEmail($"committee_{tenantId.ToLower()}_spare"), // 可選但需有效格式
                MainTel = "02-33665511",                // 必填
                SpareTel = "0912345678",
                Address = "台北市大安區羅斯福路四段1號",
                Specialty = specialization
            }
        };

        // Act
        var response = await PostAsync(Endpoints.RegisterCommittee, tenantId, command);

        // Assert
        AssertSuccess(response);
        
        var registerData = await ParseResponseAsync<RegisterResponse>(response);
        Assert.NotNull(registerData);
        Assert.True(registerData.Success);
    }

    /// <summary>
    /// AC05: CAEDP 使用者註冊
    /// 基於 RegisterSkyLabCaedpUserCommandValidator 規則
    /// Email: 必填且需唯一
    /// OfficialPhone: 必填
    /// 密碼：至少12字符 + 複雜度要求
    /// ServiceAgency: 必填且最大10字符
    /// FullName: 必填
    /// </summary>
    [Fact]
    public async Task RegisterCaedpUser_WithValidData_ShouldSucceed()
    {
        // Arrange
        var command = new RegisterSkyLabCaedpUserCommand
        {
            TenantId = TenantIds.SkyLabcaedp,
            SkyLabCaedpUserRegistrationRequest = new SkyLabCaedpUserRegistrationRequest
            {
                UserName = GenerateTestEmail("skylabcaedp"),
                Password = "CaedpPassword123!",         // 16個字符，符合12字符最小要求和複雜度
                ConfirmPassword = "CaedpPassword123!",
                Email = GenerateTestEmail("skylabcaedp"),   // 必填且需唯一
                TenantId = TenantIds.SkyLabcaedp,
                FullName = "林CAEDP",                    // 必填
                ServiceAgency = "00",              // 必填，符合10字符限制 (7字符)
                SubordinateUnit = "綜合計畫處",
                JobTitle = "專員",
                OfficialPhone = "02-23117722"           // 必填
            }
        };

        // Act
        var response = await PostAsync(Endpoints.RegisterCaedp, TenantIds.SkyLabcaedp, command);

        // Assert
        AssertSuccess(response);
        
        var registerData = await ParseResponseAsync<RegisterResponse>(response);
        Assert.NotNull(registerData);
        Assert.True(registerData.Success);
    }

    /// <summary>
    /// AC06: 協作機關使用者註冊
    /// 基於 RegisterSkyLabCollaborativeAgencyUserCommandValidator 規則 (已修正為12字符密碼要求)
    /// OfficialEmail: 必填且需有效格式
    /// Password: 至少12字符 + 複雜度要求 (與其他租戶保持一致)
    /// ServiceAgency: 必填且最大2字符
    /// 其他欄位: FileId, FullName, SystemRole, SubordinateUnit, JobTitle, OfficialPhone 都是必填
    /// </summary>
    [Fact]
    public async Task RegisterCollaborativeAgency_WithValidData_ShouldSucceed()
    {
        // Arrange
        var command = new RegisterSkyLabCollaborativeAgencyUserCommand
        {
            TenantId = TenantIds.SkyLabCollaborativeAgency,
            Request = new SkyLabCollaborativeAgencyUserRegistrationRequest
            {
                UserName = GenerateTestEmail("collaborative"),
                Password = "CollaborativePass123!",     // 20個字符，符合12字符最小要求和複雜度
                ConfirmPassword = "CollaborativePass123!",
                Email = GenerateTestEmail("collaborative"),
                TenantId = TenantIds.SkyLabCollaborativeAgency,
                SystemRole = "協作機關使用者",             // 必填，最大255字符
                FileId = "COLLAB001",                  // 必填，最大450字符
                FullName = "黃協作",                     // 必填，最大255字符
                ServiceAgency = "交通",                  // 必填，最大2字符 (2字符符合要求)
                SubordinateUnit = "公路總局",             // 必填，最大255字符
                JobTitle = "工程司",                     // 必填，最大255字符
                OfficialEmail = GenerateTestEmail("collaborative_official"), // 對應OfficialEmail驗證
                OfficialPhone = "02-23070123"           // 必填，最大50字符
            }
        };

        // Act
        var response = await PostAsync(Endpoints.RegisterCollaborativeAgency, TenantIds.SkyLabCollaborativeAgency, command);

        // Assert
        AssertSuccess(response);
        
        var registerData = await ParseResponseAsync<RegisterResponse>(response);
        Assert.NotNull(registerData);
        Assert.True(registerData.Success);
    }

    /// <summary>
    /// 測試密碼強度要求 - 基於 RegisterSkyLabMgmUserCommandValidator 的規則
    /// 密碼必須：至少12字符 + 大小寫字母 + 數字 + 特殊字符
    /// </summary>
    [Theory]
    [InlineData("weak", "密碼太簡單 - 少於12字符")]
    [InlineData("123456789012", "只有數字 - 12字符但無字母和特殊字符")]
    [InlineData("passwordtext", "只有小寫字母 - 12字符但無大寫、數字和特殊字符")]
    [InlineData("PASSWORDTEXT", "只有大寫字母 - 12字符但無小寫、數字和特殊字符")]
    [InlineData("Password1234", "缺少特殊字符 - 有大小寫和數字但無特殊字符")]
    [InlineData("Password!", "少於12字符 - 只有10字符")]
    public async Task Register_WithWeakPassword_ShouldReturnValidationError(string password, string description)
    {
        // Arrange
        var command = new RegisterSkyLabMgmUserCommand
        {
            TenantId = TenantIds.SkyLabmgm,
            UserRegistrationRequest = new SkyLabMgmUserRegistrationRequest
            {
                UserName = GenerateTestEmail($"weak_pass_{description.Split('-')[0].Trim()}"),
                Password = password,
                ConfirmPassword = password,
                Email = GenerateTestEmail($"weak_pass_{description.Split('-')[0].Trim()}"),
                TenantId = TenantIds.SkyLabmgm,
                FullName = $"弱密碼測試 - {description}",
                ServiceAgency = "00",                   // 提供必填欄位
                OfficialPhone = "0987654321",           // 提供必填欄位
                FileId = "WEAK001"                      // 提供必填欄位
            }
        };

        // Act
        var response = await PostAsync(Endpoints.Register, TenantIds.SkyLabmgm, command);

        // Assert
        Assert.Equal(400, response.Status); // BadRequest - 密碼驗證失敗
    }

    /// <summary>
    /// 測試 ServiceAgency 長度限制 - 基於驗證器的 MaximumLength(10) 規則
    /// </summary>
    [Fact]
    public async Task Register_WithTooLongServiceAgency_ShouldReturnValidationError()
    {
        // Arrange
        var command = new RegisterSkyLabMgmUserCommand
        {
            TenantId = TenantIds.SkyLabmgm,
            UserRegistrationRequest = new SkyLabMgmUserRegistrationRequest
            {
                UserName = GenerateTestEmail("long_service_agency"),
                Password = "ValidPassword123!",         // 符合密碼要求
                ConfirmPassword = "ValidPassword123!",
                Email = GenerateTestEmail("long_service_agency"),
                TenantId = TenantIds.SkyLabmgm,
                FullName = "長服務機構測試",
                ServiceAgency = "12345678901",          // 11字符，超過10字符限制
                OfficialPhone = "0987654321",           // 必填欄位
                FileId = "LONG001"                      // 必填欄位
            }
        };

        // Act
        var response = await PostAsync(Endpoints.Register, TenantIds.SkyLabmgm, command);

        // Assert
        Assert.Equal(400, response.Status); // BadRequest - ServiceAgency長度驗證失敗
    }

    /// <summary>
    /// 測試帳號不能與密碼相同 - 基於驗證器的 Must 規則
    /// </summary>
    [Fact]
    public async Task Register_WithUserNameSameAsPassword_ShouldReturnValidationError()
    {
        // Arrange
        var sameValue = "SameUserPassword123!";  // 符合密碼複雜度但與帳號相同
        var command = new RegisterSkyLabMgmUserCommand
        {
            TenantId = TenantIds.SkyLabmgm,
            UserRegistrationRequest = new SkyLabMgmUserRegistrationRequest
            {
                UserName = sameValue,                   // 與密碼相同
                Password = sameValue,                   // 與帳號相同
                ConfirmPassword = sameValue,
                Email = GenerateTestEmail("same_user_pass"),
                TenantId = TenantIds.SkyLabmgm,
                FullName = "相同帳密測試",
                ServiceAgency = "00",                   // 必填欄位
                OfficialPhone = "0987654321",           // 必填欄位
                FileId = "SAME001"                      // 必填欄位
            }
        };

        // Act
        var response = await PostAsync(Endpoints.Register, TenantIds.SkyLabmgm, command);

        // Assert
        Assert.Equal(400, response.Status); // BadRequest - 帳號密碼相同驗證失敗
    }

    #region 其他租戶類型的驗證測試

    /// <summary>
    /// 測試 SkyLabDevelop 租戶的密碼驗證規則
    /// 基於 RegisterSkyLabDevelopUserCommandValidator
    /// </summary>
    [Theory]
    [InlineData("weak")]
    [InlineData("WeakPassword")]
    [InlineData("WeakPass123")]  // 缺少特殊字符
    public async Task RegisterDeveloper_WithWeakPassword_ShouldReturnValidationError(string weakPassword)
    {
        // Arrange
        var command = new RegisterSkyLabDevelopUserCommand
        {
            TenantId = TenantIds.SkyLabdevelop,
            SkyLabDevelopUserRegistrationRequest = new SkyLabDevelopUserRegistrationRequest
            {
                UserName = GenerateTestEmail("developer_weak_pass"),
                Password = weakPassword,
                ConfirmPassword = weakPassword,
                Email = GenerateTestEmail("developer_weak_pass"),
                FullName = "開發者弱密碼測試",               // 必填
                ServiceAgency = "開發公司",                 // 必填，符合10字符限制
                OfficialPhone = "0987654321"              // 必填
            }
        };

        // Act
        var response = await PostAsync(Endpoints.RegisterDeveloper, TenantIds.SkyLabdevelop, command);

        // Assert
        Assert.Equal(400, response.Status); // BadRequest - 密碼不符合要求
    }

    /// <summary>
    /// 測試 SkyLabCommittee 租戶的主要電子郵件必填驗證
    /// 基於 RegisterSkyLabCommitteeUserCommandValidator
    /// </summary>
    [Fact]
    public async Task RegisterCommittee_WithMissingMainEmail_ShouldReturnValidationError()
    {
        // Arrange
        var command = new RegisterSkyLabCommitteeUserCommand
        {
            TenantId = TenantIds.SkyLabcommittee,
            SkyLabCommitteeUserRegistrationRequest = new SkyLabCommitteeUserRegistrationRequest
            {
                UserName = GenerateTestEmail("committee_no_main_email"),
                Password = "CommitteePassword123!",     // 符合密碼要求
                ConfirmPassword = "CommitteePassword123!",
                Email = GenerateTestEmail("committee_no_main_email"),
                FullName = "委員無主要信箱測試",           // 必填
                MainEmail = "",                         // 故意設為空值測試必填驗證
                MainTel = "02-33665511"                 // 必填
            }
        };

        // Act
        var response = await PostAsync(Endpoints.RegisterCommittee, TenantIds.SkyLabcommittee, command);

        // Assert
        Assert.Equal(400, response.Status); // BadRequest - MainEmail 必填驗證失敗
    }

    /// <summary>
    /// 測試協作機關使用者的 ServiceAgency 2字符限制
    /// 基於 RegisterSkyLabCollaborativeAgencyUserCommandValidator
    /// </summary>
    [Fact]
    public async Task RegisterCollaborativeAgency_WithTooLongServiceAgency_ShouldReturnValidationError()
    {
        // Arrange
        var command = new RegisterSkyLabCollaborativeAgencyUserCommand
        {
            TenantId = TenantIds.SkyLabCollaborativeAgency,
            Request = new SkyLabCollaborativeAgencyUserRegistrationRequest
            {
                UserName = GenerateTestEmail("collaborative_long_agency"),
                Password = "CollaborativePass123!",     // 符合12字符密碼要求
                ConfirmPassword = "CollaborativePass123!",
                Email = GenerateTestEmail("collaborative_long_agency"),
                FullName = "協作機關長代碼測試",           // 必填
                ServiceAgency = "交通運輸部",              // 5字符，超過2字符限制
                SystemRole = "協作機關使用者",             // 必填
                FileId = "LONG001",                    // 必填
                SubordinateUnit = "公路總局",             // 必填
                JobTitle = "工程司",                     // 必填
                OfficialEmail = GenerateTestEmail("collaborative_long_agency_official"),
                OfficialPhone = "02-23070123"           // 必填
            }
        };

        // Act
        var response = await PostAsync(Endpoints.RegisterCollaborativeAgency, TenantIds.SkyLabCollaborativeAgency, command);

        // Assert
        Assert.Equal(400, response.Status); // BadRequest - ServiceAgency 長度限制驗證失敗
    }

    /// <summary>
    /// 測試協作機關使用者的密碼要求 (現在也是12字符 + 複雜度要求)
    /// 基於修正後的 RegisterSkyLabCollaborativeAgencyUserCommandValidator
    /// </summary>
    [Theory]
    [InlineData("123")]           // 3字符
    [InlineData("12345")]         // 5字符
    [InlineData("shortpass")]     // 9字符
    [InlineData("CollabPass1")]   // 11字符，少於12字符
    [InlineData("CollabPassword")] // 15字符但缺少數字和特殊字符
    public async Task RegisterCollaborativeAgency_WithWeakPassword_ShouldReturnValidationError(string weakPassword)
    {
        // Arrange
        var command = new RegisterSkyLabCollaborativeAgencyUserCommand
        {
            TenantId = TenantIds.SkyLabCollaborativeAgency,
            Request = new SkyLabCollaborativeAgencyUserRegistrationRequest
            {
                UserName = GenerateTestEmail("collaborative_weak_pass"),
                Password = weakPassword,
                ConfirmPassword = weakPassword,
                Email = GenerateTestEmail("collaborative_weak_pass"),
                FullName = "協作機關弱密碼測試",           // 必填
                ServiceAgency = "交通",                  // 必填，符合2字符限制
                SystemRole = "協作機關使用者",             // 必填
                FileId = "WEAK001",                    // 必填
                SubordinateUnit = "公路總局",             // 必填
                JobTitle = "工程司",                     // 必填
                OfficialEmail = GenerateTestEmail("collaborative_weak_pass_official"),
                OfficialPhone = "02-23070123"           // 必填
            }
        };

        // Act
        var response = await PostAsync(Endpoints.RegisterCollaborativeAgency, TenantIds.SkyLabCollaborativeAgency, command);

        // Assert
        Assert.Equal(400, response.Status); // BadRequest - 密碼不符合12字符和複雜度要求
    }

    #endregion
}
