using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Common.Interfaces.Repositories;
using SkyLabIdP.Application.Dtos;
using SkyLabIdP.Application.Dtos.User.Registration;
using SkyLabIdP.Application.SystemApps.Services;
using SkyLabIdP.Application.SystemApps.Services.ServiceSettings;
using SkyLabIdP.Application.SystemApps.Users.Commands.Writers;
using SkyLabIdP.Domain.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Application.UnitTests.SystemApps.Users.Commands.RegisterUser
{
    // 新增一個包裝器介面以解決 Moq 不支援擴展方法的問題
    public interface IDataProtectorWrapper
    {
        string Protect(string data);
        string Unprotect(string data);
    }

    public class DataProtectorWrapper : IDataProtectorWrapper
    {
        private readonly IDataProtectionService _dataProtectionService;

        public DataProtectorWrapper(IDataProtectionService protector)
        {
            _dataProtectionService = protector;
        }

        public string Protect(string data) => _dataProtectionService.Protect(data);
        public string Unprotect(string data) => _dataProtectionService.Unprotect(data);
    }

    public class BaseUserDetailWriterTests
    {
        /// <summary>
        /// 測試自定義請求類型與回應類型
        /// </summary>
        [Fact]
        public async Task WriteAsync_WithCustomTypesAndValidData_ReturnsCorrectResponse()
        {
            // Arrange
            var fixtures = new TestFixtures();
            var request = new CustomUserRegistrationRequest
            {
                CustomField = "TestValue",
                UserName = "testuser",
                Email = "test@example.com",
                Password = "Password123!@#",
                TenantId = "skylabmgm"
            };
            var writer = fixtures.CreateTestUserDetailWriter();

            // Act
            var result = await writer.WriteAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.operationResult.Success);
            Assert.Equal(200, result.operationResult.StatusCode);
            Assert.Equal("TestValue", result.CustomResponseField);
        }

        /// <summary>
        /// 測試固定裝置
        /// </summary>
        private class TestFixtures
        {
            private readonly Mock<IUnitOfWork> _unitOfWorkMock;
            private readonly Mock<IUserTenantRepository> _userTenantRepoMock;
            private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
            private readonly Mock<IDataProtector> _dataProtectionServiceMock;
            private readonly Mock<IDataProtectorWrapper> _dataProtectionServiceWrapperMock;
            private readonly Mock<ILogger> _loggerMock;
            private readonly Mock<DbSet<TestUserDetail>> _testUserDetailsMock;
            private readonly List<ApplicationUser> _users;

            public TestFixtures()
            {
                _users = new List<ApplicationUser>();
                
                _userManagerMock = MockUserManager();
                _unitOfWorkMock = new Mock<IUnitOfWork>();
                _userTenantRepoMock = new Mock<IUserTenantRepository>();
                _dataProtectionServiceMock = new Mock<IDataProtector>();
                _dataProtectionServiceWrapperMock = new Mock<IDataProtectorWrapper>();
                _loggerMock = new Mock<ILogger>();
                
                _testUserDetailsMock = new Mock<DbSet<TestUserDetail>>();
                
                _userTenantRepoMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);
                _userTenantRepoMock.Setup(r => r.AddAsync(It.IsAny<UserTenant>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
                _unitOfWorkMock.Setup(u => u.UserTenants).Returns(_userTenantRepoMock.Object);
                
                SetupDefaultBehavior();
            }

            public TestUserDetailWriter CreateTestUserDetailWriter()
            {
                // 創建所有必要的模擬服務
                var mockJwtService = new Mock<IJwtService>();
                var mockLoginServiceLogger = new Mock<ILogger<AbstractLoginUserInfoService>>();
                var mockEmailService = new Mock<IEmailService>();
                var mockLoginNotificationService = new Mock<ILoginNotificationService>();
                var mockSaltGenerator = new Mock<ISaltGenerator>();
                var mockMapper = SkyLabIdPMapper.Instance;
                var mockCaptchaService = new Mock<ICaptchaService>();
                var mockDistributedCache = new Mock<IDistributedCache>();
                var mockTokenService = new Mock<SkyLabIdP.Application.Common.Interfaces.ITokenStorageService>();

                var loginUserInfoServiceSettings = new LoginUserInfoServiceSettings
                {
                    UnitOfWork = _unitOfWorkMock.Object,
                    UserManager = _userManagerMock.Object,
                    Dataprotectionservice = CreateDataProtectionService(),
                    Configuration = CreateConfiguration(),
                    JwtService = mockJwtService.Object,
                    Logger = mockLoginServiceLogger.Object,
                    EmailService = mockEmailService.Object,
                    LoginNotificationService = mockLoginNotificationService.Object,
                    SaltGenerator = mockSaltGenerator.Object,
                    Mapper = mockMapper,
                    CaptchaService = mockCaptchaService.Object,
                    Cache = mockDistributedCache.Object,
                    TokenStorageService = mockTokenService.Object
                };

                return new TestUserDetailWriter(loginUserInfoServiceSettings, _loggerMock.Object, _testUserDetailsMock);
            }

            private void SetupDefaultBehavior()
            {
                // 設置默認的使用者創建行為
                _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                    .ReturnsAsync(IdentityResult.Success)
                    .Callback<ApplicationUser, string>((user, _) => 
                    {
                        user.Id = "test-user-id";
                        _users.Add(user);
                    });

                _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync((string email) => _users.FirstOrDefault(u => u.Email == email));

                // 使用包裝器類來模擬 IDataProtector 功能，避免直接模擬擴展方法
                _dataProtectionServiceWrapperMock.Setup(p => p.Protect(It.IsAny<string>()))
                    .Returns<string>(s => $"protected_{s}");

                _dataProtectionServiceWrapperMock.Setup(p => p.Unprotect(It.IsAny<string>()))
                    .Returns<string>(s => s.Replace("protected_", ""));
            }

            private IDataProtectionService CreateDataProtectionService()
            {
                var protectionServiceMock = new Mock<IDataProtectionService>();
                
                // 設置 IDataProtectionService 的方法
                protectionServiceMock.Setup(p => p.Protect(It.IsAny<string>()))
                    .Returns<string>(s => $"protected_{s}");
                
                protectionServiceMock.Setup(p => p.Unprotect(It.IsAny<string>()))
                    .Returns<string>(s => s.Replace("protected_", ""));
                
                return protectionServiceMock.Object;
            }

            private IConfiguration CreateConfiguration()
            {
                var configMock = new Mock<IConfiguration>();
                var configSectionMock = new Mock<IConfigurationSection>();
                
                configSectionMock.Setup(s => s.Value).Returns("test_purpose");
                configMock
                    .Setup(c => c[It.Is<string>(s => s == "DataProtection:Purpose")])
                    .Returns("test_purpose");
                
                return configMock.Object;
            }

            private Mock<UserManager<ApplicationUser>> MockUserManager()
            {
                var store = new Mock<IUserStore<ApplicationUser>>();
                var options = new Mock<IOptions<IdentityOptions>>();
                var idOptions = new IdentityOptions();
                options.Setup(o => o.Value).Returns(idOptions);
                
                var userValidators = new List<IUserValidator<ApplicationUser>>();
                var validator = new Mock<IUserValidator<ApplicationUser>>();
                userValidators.Add(validator.Object);
                
                var pwdValidators = new List<IPasswordValidator<ApplicationUser>>();
                var pwdValidator = new Mock<IPasswordValidator<ApplicationUser>>();
                pwdValidators.Add(pwdValidator.Object);
                
                var userManager = new Mock<UserManager<ApplicationUser>>(
                    store.Object,
                    options.Object,
                    new Mock<IPasswordHasher<ApplicationUser>>().Object,
                    userValidators,
                    pwdValidators,
                    new Mock<ILookupNormalizer>().Object,
                    new Mock<IdentityErrorDescriber>().Object,
                    new Mock<IServiceProvider>().Object,
                    new Mock<ILogger<UserManager<ApplicationUser>>>().Object);
                
                return userManager;
            }
        }

        // 測試用的自定義類型
        private class CustomUserRegistrationRequest : BaseUserRegistrationRequest
        {
            public string CustomField { get; set; } = string.Empty;
        }

        private class CustomUserRegistrationResponse : BaseUserRegistrationResponse
        {
            public string CustomResponseField { get; set; } = string.Empty;
        }

        public class TestUserDetail
        {
            public int Id { get; set; }
            public string UserId { get; set; } = string.Empty;
            public string CustomField { get; set; } = string.Empty;
        }

        private class TestUserDetailWriter : BaseUserDetailWriter<CustomUserRegistrationRequest, CustomUserRegistrationResponse, TestUserDetail>
        {
            private readonly Mock<DbSet<TestUserDetail>> _testUserDetails;

            public TestUserDetailWriter(
                LoginUserInfoServiceSettings loginUserInfoServiceSettings,
                ILogger logger,
                Mock<DbSet<TestUserDetail>> testUserDetails)
                : base(loginUserInfoServiceSettings, logger)
            {
                _testUserDetails = testUserDetails;
            }

            protected override TestUserDetail CreateUserDetail(CustomUserRegistrationRequest request, ApplicationUser user, string tenantGuid)
            {
                return new TestUserDetail
                {
                    Id = 1,
                    UserId = user.Id,
                    CustomField = request.CustomField
                };
            }

            protected override async Task AddUserDetailToContextAsync(TestUserDetail userDetail, CancellationToken cancellationToken)
            {
                await Task.CompletedTask;
                // 當這個方法被調用時，設置自定義回應欄位值
                _testUserDetails.Object.Add(userDetail);
            }

            public override async Task<CustomUserRegistrationResponse> WriteAsync(CustomUserRegistrationRequest request, CancellationToken cancellationToken)
            {
                var response = await base.WriteAsync(request, cancellationToken);
                response.CustomResponseField = request.CustomField;
                return response;
            }
        }
    }
}