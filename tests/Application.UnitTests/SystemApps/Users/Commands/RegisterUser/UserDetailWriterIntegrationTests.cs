using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Common.Interfaces.Repositories;
using SkyLabIdP.Application.Dtos.User.Registration;
using SkyLabIdP.Application.SystemApps.Services.ServiceSettings;
using SkyLabIdP.Application.SystemApps.Users.Commands.Writers;
using SkyLabIdP.Application.SystemApps.Services;
using SkyLabIdP.Domain.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Application.UnitTests.SystemApps.Users.Commands.RegisterUser
{
    public class UserDetailWriterIntegrationTests
    {
        [Fact]
        public async Task WriteAsync_WithValidData_ReturnsSuccessResponse()
        {
            // Arrange
            var fixtures = new UserDetailWriterTestFixtures();
            var request = fixtures.CreateValidRequest();
            var writer = fixtures.CreateSkyLabDocUserDetailWriter();

            // Act
            var result = await writer.WriteAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.operationResult.Success);
            Assert.Equal(200, result.operationResult.StatusCode);
            Assert.Equal("新增成功", result.operationResult.Messages.FirstOrDefault());
        }

        [Fact]
        public async Task WriteAsync_WithExistingUser_UsesExistingUser()
        {
            // Arrange
            var fixtures = new UserDetailWriterTestFixtures();
            var existingUser = new ApplicationUser
            {
                Id = "existing-user-id",
                UserName = "existinguser",
                Email = "existing@example.com"
            };

            fixtures.SetupExistingUser(existingUser);
            var request = new SkyLabMgmUserRegistrationRequest
            {
                UserName = "existinguser",
                Email = "existing@example.com",
                Password = "Password123!@#",
                TenantId = "skylabmgm"
            };

            var writer = fixtures.CreateSkyLabDocUserDetailWriter();

            // Act
            var result = await writer.WriteAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.operationResult.Success);
            
            // 檢查現有用戶是否被使用 - 驗證用戶列表中包含設置的現有用戶
            Assert.Contains(existingUser, fixtures.GetUsers());

            // 確認不調用 CreateAsync 方法創建新用戶
            fixtures.VerifyUserManagerCreateAsyncNotCalled();
        }

        [Fact]
        public async Task WriteAsync_WithUserInSameTenant_ReturnsConflictError()
        {
            // Arrange
            var fixtures = new UserDetailWriterTestFixtures();
            var existingUser = new ApplicationUser
            {
                Id = "existing-user-id",
                UserName = "existinguser",
                Email = "existing@example.com"
            };

            fixtures.SetupExistingUser(existingUser);
            fixtures.SetupUserExistsInTenant(existingUser.Id, "skylabmgm");

            var request = new SkyLabMgmUserRegistrationRequest
            {
                UserName = "existinguser",
                Email = "existing@example.com",
                Password = "Password123!@#",
                TenantId = "skylabmgm"
            };

            var writer = fixtures.CreateSkyLabDocUserDetailWriter();

            // Act
            var result = await writer.WriteAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.operationResult.Success);
            Assert.Equal(400, result.operationResult.StatusCode);
            Assert.Contains("帳號已存在於租戶中", result.operationResult.Messages.FirstOrDefault());
        }

        [Fact]
        public async Task WriteAsync_WithUserCreationFailing_ReturnsError()
        {
            // Arrange
            var fixtures = new UserDetailWriterTestFixtures();

            fixtures.SetupUserCreationFailure(new IdentityError[]
            {
                new IdentityError { Description = "測試錯誤" }
            });

            var request = fixtures.CreateValidRequest();
            var writer = fixtures.CreateSkyLabDocUserDetailWriter();

            // Act
            var result = await writer.WriteAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.operationResult.Success);
            Assert.Equal(400, result.operationResult.StatusCode);
            Assert.Contains("帳號建立失敗", result.operationResult.Messages.FirstOrDefault());
        }

        #region SkyLabDevelop Integration Tests

        [Fact]
        public async Task WriteAsync_WithValidSkyLabDevelopData_ReturnsSuccessResponse()
        {
            // Arrange
            var fixtures = new UserDetailWriterTestFixtures();
            var request = fixtures.CreateValidSkyLabDevelopRequest();
            var writer = fixtures.CreateSkyLabDevelopUserDetailWriter();

            // Act
            var result = await writer.WriteAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.operationResult.Success);
            Assert.Equal(200, result.operationResult.StatusCode);
            Assert.Equal("新增成功", result.operationResult.Messages.FirstOrDefault());
        }

        [Fact]
        public async Task WriteAsync_WithExistingSkyLabDevelopUser_UsesExistingUser()
        {
            // Arrange
            var fixtures = new UserDetailWriterTestFixtures();
            var existingUser = new ApplicationUser
            {
                Id = "existing-develop-user-id",
                UserName = "develop@example.com",
                Email = "develop@example.com"
            };

            fixtures.SetupExistingUser(existingUser);
            var request = fixtures.CreateValidSkyLabDevelopRequest();
            var writer = fixtures.CreateSkyLabDevelopUserDetailWriter();

            // Act
            var result = await writer.WriteAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.operationResult.Success);
            // 檢查現有用戶是否被使用 - 驗證用戶列表中包含設置的現有用戶
            Assert.Contains(existingUser, fixtures.GetUsers());
            // 確認不調用 CreateAsync 方法創建新用戶
            fixtures.VerifyUserManagerCreateAsyncNotCalled();
        }

        [Fact]
        public async Task WriteAsync_WithSkyLabDevelopUserInSameTenant_ReturnsConflictError()
        {
            // Arrange
            var fixtures = new UserDetailWriterTestFixtures();
            var existingUser = new ApplicationUser
            {
                Id = "existing-develop-user-id",
                UserName = "develop@example.com",
                Email = "develop@example.com"
            };

            fixtures.SetupExistingUser(existingUser);
            fixtures.SetupUserExistsInTenant(existingUser.Id, "skylabdevelop");

            var request = fixtures.CreateValidSkyLabDevelopRequest();
            var writer = fixtures.CreateSkyLabDevelopUserDetailWriter();

            // Act
            var result = await writer.WriteAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.operationResult.Success);
            Assert.Equal(400, result.operationResult.StatusCode);
            Assert.Contains("帳號已存在於租戶中", result.operationResult.Messages.FirstOrDefault());
        }

        [Fact]
        public async Task WriteAsync_WithSkyLabDevelopUserCreationFailing_ReturnsError()
        {
            // Arrange
            var fixtures = new UserDetailWriterTestFixtures();

            fixtures.SetupUserCreationFailure(new IdentityError[]
            {
                new IdentityError { Description = "開發者帳號建立失敗" }
            });

            var request = fixtures.CreateValidSkyLabDevelopRequest();
            var writer = fixtures.CreateSkyLabDevelopUserDetailWriter();

            // Act
            var result = await writer.WriteAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.operationResult.Success);
            Assert.Equal(400, result.operationResult.StatusCode);
            Assert.Contains("帳號建立失敗", result.operationResult.Messages.FirstOrDefault());
        }

        #endregion




        /// <summary>
        /// 測試固定裝置，用於初始化測試場景
        /// </summary>
        private class UserDetailWriterTestFixtures
        {
            private readonly Mock<IUnitOfWork> _unitOfWorkMock;
            private readonly Mock<IUserTenantRepository> _userTenantRepoMock;
            private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
            private readonly Mock<IDataProtector> _dataProtectionServiceMock;
            private readonly Mock<ILogger<SkyLabDocUserDetailWriter>> _loggerMock;
            private readonly Mock<ILogger<SkyLabDevelopUserDetailWriter>> _developLoggerMock;
            private readonly List<ApplicationUser> _users;
            private readonly List<UserTenant> _userTenants;

            public UserDetailWriterTestFixtures()
            {
                _users = new List<ApplicationUser>();
                _userTenants = new List<UserTenant>();

                _userManagerMock = MockUserManager<ApplicationUser>();
                _unitOfWorkMock = new Mock<IUnitOfWork>();
                _userTenantRepoMock = new Mock<IUserTenantRepository>();
                _dataProtectionServiceMock = new Mock<IDataProtector>();
                _loggerMock = new Mock<ILogger<SkyLabDocUserDetailWriter>>();
                _developLoggerMock = new Mock<ILogger<SkyLabDevelopUserDetailWriter>>();

                _userTenantRepoMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);
                _userTenantRepoMock.Setup(r => r.AddAsync(It.IsAny<UserTenant>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
                _unitOfWorkMock.Setup(u => u.UserTenants).Returns(_userTenantRepoMock.Object);

                var docRepoMock = new Mock<ISkyLabDocUserDetailRepository>();
                docRepoMock.Setup(r => r.AddAsync(It.IsAny<SkyLabDocUserDetail>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
                _unitOfWorkMock.Setup(u => u.SkyLabDocUserDetails).Returns(docRepoMock.Object);

                var developRepoMock = new Mock<ISkyLabDevelopUserDetailRepository>();
                developRepoMock.Setup(r => r.AddAsync(It.IsAny<SkyLabDevelopUserDetail>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
                _unitOfWorkMock.Setup(u => u.SkyLabDevelopUserDetails).Returns(developRepoMock.Object);

                SetupDefaultBehavior();
            }

            public Mock<UserManager<ApplicationUser>> GetUserManagerMock() => _userManagerMock;

            public List<ApplicationUser> GetUsers() => _users;


            public SkyLabMgmUserRegistrationRequest CreateValidRequest()
            {

                return new SkyLabMgmUserRegistrationRequest
                {
                    FileId = "test-file-id",
                    UserName = "testuser",
                    Email = "test@example.com",
                    Password = "Password123!@#",
                    ConfirmPassword = "Password123!@#",
                    FullName = "Test User",
                    BranchCode = "Test Agency",
                    SubordinateUnit = "Test Unit",
                    JobTitle = "Test Title",
                    OfficialPhone = "0123456789",
                    TenantId = "skylabmgm"
                };
            }

            public SkyLabDocUserDetailWriter CreateSkyLabDocUserDetailWriter()
            {
                // 創建所有必要的模擬服務
                var mockJwtService = new Mock<IJwtService>();
                var mockEmailService = new Mock<IEmailService>();
                var mockSaltGenerator = new Mock<ISaltGenerator>();
                var mockMapper = SkyLabIdPMapper.Instance;
                var mockCaptchaService = new Mock<ICaptchaService>();
                var mockDistributedCache = new Mock<IDistributedCache>();
                var mockTokenService = new Mock<SkyLabIdP.Application.Common.Interfaces.ITokenStorageService>();
                var mockLoginServiceLogger = new Mock<ILogger<AbstractLoginUserInfoService>>();
                var mockLoginNotificationService = new Mock<ILoginNotificationService>();

                var loginUserInfoServiceSettings = new LoginUserInfoServiceSettings
                {
                    UnitOfWork = _unitOfWorkMock.Object,
                    UserManager = _userManagerMock.Object,
                    Dataprotectionservice = CreateDataProtectionService(),
                    Configuration = CreateConfiguration(),
                    JwtService = mockJwtService.Object,
                    Logger = mockLoginServiceLogger.Object,  // 使用正確類型的 ILogger
                    EmailService = mockEmailService.Object,
                    LoginNotificationService = mockLoginNotificationService.Object,
                    SaltGenerator = mockSaltGenerator.Object,
                    Mapper = mockMapper,
                    CaptchaService = mockCaptchaService.Object,
                    Cache = mockDistributedCache.Object,
                    TokenStorageService = mockTokenService.Object
                };

                return new SkyLabDocUserDetailWriter(loginUserInfoServiceSettings, _loggerMock.Object);
            }



            public SkyLabDevelopUserDetailWriter CreateSkyLabDevelopUserDetailWriter()
            {
                // 創建所有必要的模擬服務
                var mockJwtService = new Mock<IJwtService>();
                var mockEmailService = new Mock<IEmailService>();
                var mockSaltGenerator = new Mock<ISaltGenerator>();
                var mockMapper = SkyLabIdPMapper.Instance;
                var mockCaptchaService = new Mock<ICaptchaService>();
                var mockDistributedCache = new Mock<IDistributedCache>();
                var mockTokenService = new Mock<SkyLabIdP.Application.Common.Interfaces.ITokenStorageService>();
                var mockLoginServiceLogger = new Mock<ILogger<AbstractLoginUserInfoService>>();
                var mockLoginNotificationService = new Mock<ILoginNotificationService>();

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

                return new SkyLabDevelopUserDetailWriter(loginUserInfoServiceSettings, _developLoggerMock.Object);
            }


            public SkyLabDevelopUserRegistrationRequest CreateValidSkyLabDevelopRequest()
            {
                return new SkyLabDevelopUserRegistrationRequest
                {
                    Email = "develop@example.com",
                    FullName = "開發者測試用戶",
                    BranchCode = "測試開發機構",
                    SubordinateUnit = "開發部門",
                    JobTitle = "系統開發工程師",
                    OfficialPhone = "02-12345678",
                    UserName = "develop@example.com",
                    Password = "Password123!@#",
                    ConfirmPassword = "Password123!@#",
                    TenantId = "skylabdevelop"
                };
            }


            public void SetupExistingUser(ApplicationUser user)
            {
                _users.Clear();
                _users.Add(user);

                _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync((string email) => _users.FirstOrDefault(u => u.Email == email));
            }

            public void SetupUserExistsInTenant(string userId, string tenantId)
            {
                _userTenantRepoMock.Setup(r => r.ExistsAsync(userId, tenantId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);
            }

            public void SetupUserCreationFailure(IdentityError[] errors)
            {
                _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                    .ReturnsAsync(IdentityResult.Failed(errors));
            }

            public void VerifyUserManagerCreateAsyncNotCalled()
            {
                _userManagerMock.Verify(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
            }

            private void SetupDefaultBehavior()
            {
                _userManagerMock.Setup(um => um.FindByNameAsync(It.IsAny<string>()))
                    .ReturnsAsync((string username) => _users.FirstOrDefault(u => u.UserName == username));
                
                _userManagerMock.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync((string email) => _users.FirstOrDefault(u => u.Email == email));
                
                _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                    .ReturnsAsync(IdentityResult.Success)
                    .Callback<ApplicationUser, string>((user, _) => 
                    {
                        user.Id = Guid.NewGuid().ToString();
                        _users.Add(user);
                    });
            }

            private IDataProtectionService CreateDataProtectionService()
            {
                var dataProtectionServiceMock = new Mock<IDataProtectionService>();
                
                dataProtectionServiceMock.Setup(s => s.Protect(It.IsAny<string>()))
                    .Returns<string>(data => data);
                
                dataProtectionServiceMock.Setup(s => s.Unprotect(It.IsAny<string>()))
                    .Returns<string>(data => data);
                
                return dataProtectionServiceMock.Object;
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

            // 通用的 UserManager Mock 工廠方法
            private static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
            {
                var store = new Mock<IUserStore<TUser>>();
                var options = new Mock<IOptions<IdentityOptions>>();
                var idOptions = new IdentityOptions();
                options.Setup(o => o.Value).Returns(idOptions);
                
                var userValidators = new List<IUserValidator<TUser>>();
                var validator = new Mock<IUserValidator<TUser>>();
                userValidators.Add(validator.Object);
                
                var pwdValidators = new List<IPasswordValidator<TUser>>();
                var pwdValidator = new Mock<IPasswordValidator<TUser>>();
                pwdValidators.Add(pwdValidator.Object);
                
                var userManager = new Mock<UserManager<TUser>>(
                    store.Object,
                    options.Object,
                    new Mock<IPasswordHasher<TUser>>().Object,
                    userValidators,
                    pwdValidators,
                    new Mock<ILookupNormalizer>().Object,
                    new Mock<IdentityErrorDescriber>().Object,
                    new Mock<IServiceProvider>().Object,
                    new Mock<ILogger<UserManager<TUser>>>().Object);
                
                return userManager;
            }




        }
    }
}