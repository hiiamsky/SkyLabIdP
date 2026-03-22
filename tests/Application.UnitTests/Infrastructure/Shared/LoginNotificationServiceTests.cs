using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos.Email;
using SkyLabIdP.Domain.Enums;
using SkyLabIdP.Domain.Settings;
using SkyLabIdP.Shared.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Application.UnitTests.Infrastructure.Shared
{
    /// <summary>
    /// LoginNotificationService 單元測試
    /// </summary>
    public class LoginNotificationServiceTests
    {
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<IOptions<LoginNotificationSettings>> _mockOptions;
        private readonly Mock<IOptions<MailSettings>> _mockMailSettings;
        private readonly Mock<IHostEnvironment> _mockHostEnvironment;
        private readonly Mock<ILogger<LoginNotificationService>> _mockLogger;
        private readonly LoginNotificationService _service;
        private readonly LoginNotificationSettings _settings;

        public LoginNotificationServiceTests()
        {
            _mockEmailService = new Mock<IEmailService>();
            _mockOptions = new Mock<IOptions<LoginNotificationSettings>>();
            _mockMailSettings = new Mock<IOptions<MailSettings>>();
            _mockHostEnvironment = new Mock<IHostEnvironment>();
            _mockLogger = new Mock<ILogger<LoginNotificationService>>();

            // 預設測試設定
            _settings = new LoginNotificationSettings
            {
                EnableNotification = true,
                DevelopmentEmailOverride = new DevelopmentEmailOverride
                {
                    Enabled = false,
                    Recipients = new List<string> { "dev@test.com" }
                },
                TenantConfigurations = new Dictionary<string, TenantLoginNotificationConfig>
                {
                    {
                        nameof(Tenants.SkyLabmgm), 
                        new TenantLoginNotificationConfig
                        {
                            Subject = "SKYLAB管理系統登入通知",
                            EnableSuccessNotification = true,
                            EnableFailureNotification = true
                        }
                    },
                    {
                        "SkyLabcommittee", 
                        new TenantLoginNotificationConfig
                        {
                            Subject = "SkyLab委員系統登入通知",
                            EnableSuccessNotification = true,
                            EnableFailureNotification = true
                        }
                    }
                }
            };

            var mailSettings = new MailSettings
            {
                EmailFrom = "system@test.com"
            };

            _mockOptions.Setup(x => x.Value).Returns(_settings);
            _mockMailSettings.Setup(x => x.Value).Returns(mailSettings);
            _mockHostEnvironment.Setup(x => x.EnvironmentName).Returns("Development");

            _service = new LoginNotificationService(
                _mockEmailService.Object,
                _mockOptions.Object,
                _mockMailSettings.Object,
                _mockLogger.Object,
                _mockHostEnvironment.Object);
        }

        [Fact]
        public async Task SendLoginNotificationAsync_Should_Not_Send_When_Notification_Disabled()
        {
            // Arrange
            _settings.EnableNotification = false;

            // Act
            await _service.SendLoginNotificationAsync(
                tenantId: nameof(Tenants.SkyLabmgm),
                userName: "testuser",
                officialEmail: "user@test.com",
                isSuccess: true,
                failureReason: null,
                ipAddress: "192.168.1.1");

            // Assert
            _mockEmailService.Verify(x => x.SendAsync(It.IsAny<EmailDto>()), Times.Never);
        }

        [Fact]
        public async Task SendLoginNotificationAsync_Should_Use_Development_Override_In_Development()
        {
            // Arrange
            _settings.DevelopmentEmailOverride.Enabled = true;
            _mockHostEnvironment.Setup(x => x.EnvironmentName).Returns("Development");

            // Act
            await _service.SendLoginNotificationAsync(
                tenantId: nameof(Tenants.SkyLabmgm),
                userName: "testuser",
                officialEmail: "user@test.com",
                isSuccess: true,
                failureReason: null,
                ipAddress: "192.168.1.1");

            // Assert
            _mockEmailService.Verify(x => x.SendAsync(It.Is<EmailDto>(dto => 
                dto.To.Contains("dev@test.com"))), Times.Once);
        }

        [Fact]
        public async Task SendLoginNotificationAsync_Should_Use_Official_Email_In_Production()
        {
            // Arrange
            _settings.DevelopmentEmailOverride.Enabled = true; // 即使開啟，在生產環境也不應該生效
            _mockHostEnvironment.Setup(x => x.EnvironmentName).Returns("Production");

            // Act
            await _service.SendLoginNotificationAsync(
                tenantId: nameof(Tenants.SkyLabmgm),
                userName: "testuser",
                officialEmail: "user@test.com",
                isSuccess: true,
                failureReason: null,
                ipAddress: "192.168.1.1");

            // Assert
            _mockEmailService.Verify(x => x.SendAsync(It.Is<EmailDto>(dto => 
                dto.To.Contains("user@test.com"))), Times.Once);
        }

        [Fact]
        public async Task SendLoginNotificationAsync_Should_Generate_Correct_Success_Email_For_SkyLabmgm()
        {
            // Arrange
            EmailDto? capturedEmail = null;
            _mockEmailService.Setup(x => x.SendAsync(It.IsAny<EmailDto>()))
                           .Callback<EmailDto>(dto => capturedEmail = dto);

            // Act
            await _service.SendLoginNotificationAsync(
                tenantId: nameof(Tenants.SkyLabmgm),
                userName: "testuser",
                officialEmail: "user@test.com",
                isSuccess: true,
                failureReason: null,
                ipAddress: "192.168.1.1");

            // Assert
            Assert.NotNull(capturedEmail);
            Assert.Contains("user@test.com", capturedEmail.To);
            Assert.Equal("SKYLAB管理系統登入通知 - 登入成功", capturedEmail.Subject);
            Assert.Contains("testuser", capturedEmail.Body);
            Assert.Contains("成功", capturedEmail.Body);
            Assert.Contains("192.168.1.1", capturedEmail.Body);
        }

        [Fact]
        public async Task SendLoginNotificationAsync_Should_Generate_Correct_Failure_Email_For_SkyLabmgm()
        {
            // Arrange
            EmailDto? capturedEmail = null;
            _mockEmailService.Setup(x => x.SendAsync(It.IsAny<EmailDto>()))
                           .Callback<EmailDto>(dto => capturedEmail = dto);

            // Act
            await _service.SendLoginNotificationAsync(
                tenantId: nameof(Tenants.SkyLabmgm),
                userName: "testuser",
                officialEmail: "user@test.com",
                isSuccess: false,
                failureReason: "密碼錯誤",
                ipAddress: "192.168.1.1");

            // Assert
            Assert.NotNull(capturedEmail);
            Assert.Contains("user@test.com", capturedEmail.To);
            Assert.Equal("SKYLAB管理系統登入通知 - 登入失敗", capturedEmail.Subject);
            Assert.Contains("testuser", capturedEmail.Body);
            Assert.Contains("失敗", capturedEmail.Body);
            Assert.Contains("密碼錯誤", capturedEmail.Body);
            Assert.Contains("192.168.1.1", capturedEmail.Body);
        }

        [Fact]
        public async Task SendLoginNotificationAsync_Should_Generate_Correct_Email_For_SkyLabCommittee()
        {
            // Arrange
            EmailDto? capturedEmail = null;
            _mockEmailService.Setup(x => x.SendAsync(It.IsAny<EmailDto>()))
                           .Callback<EmailDto>(dto => capturedEmail = dto);

            // Act
            await _service.SendLoginNotificationAsync(
                tenantId: "SkyLabcommittee",
                userName: "committee_user",
                officialEmail: "committee@test.com",
                isSuccess: true,
                failureReason: null,
                ipAddress: "10.0.0.1");

            // Assert
            Assert.NotNull(capturedEmail);
            Assert.Contains("committee@test.com", capturedEmail.To);
            Assert.Equal("SkyLab委員系統登入通知 - 登入成功", capturedEmail.Subject);
            Assert.Contains("committee_user", capturedEmail.Body);
            Assert.Contains("成功", capturedEmail.Body);
            Assert.Contains("10.0.0.1", capturedEmail.Body);
        }

        [Fact]
        public async Task SendLoginNotificationAsync_Should_Use_Default_Template_For_Unknown_Tenant()
        {
            // Arrange
            EmailDto? capturedEmail = null;
            _mockEmailService.Setup(x => x.SendAsync(It.IsAny<EmailDto>()))
                           .Callback<EmailDto>(dto => capturedEmail = dto);

            // Act
            await _service.SendLoginNotificationAsync(
                tenantId: "UnknownTenant",
                userName: "testuser",
                officialEmail: "user@test.com",
                isSuccess: true,
                failureReason: null,
                ipAddress: "192.168.1.1");

            // Assert
            Assert.NotNull(capturedEmail);
            Assert.Contains("user@test.com", capturedEmail.To);
            Assert.Equal("系統登入通知 - 登入成功", capturedEmail.Subject);
            Assert.Contains("testuser", capturedEmail.Body);
            Assert.Contains("登入", capturedEmail.Body);
            Assert.Contains("192.168.1.1", capturedEmail.Body);
        }

        [Fact]
        public async Task SendLoginNotificationAsync_Should_Skip_When_Official_Email_Is_Null()
        {
            // Act
            await _service.SendLoginNotificationAsync(
                tenantId: nameof(Tenants.SkyLabmgm),
                userName: "testuser",
                officialEmail: null,
                isSuccess: true,
                failureReason: null,
                ipAddress: "192.168.1.1");

            // Assert
            _mockEmailService.Verify(x => x.SendAsync(It.IsAny<EmailDto>()), Times.Never);
        }

        [Fact]
        public async Task SendLoginNotificationAsync_Should_Handle_Email_Service_Exception_Gracefully()
        {
            // Arrange
            _mockEmailService.Setup(x => x.SendAsync(It.IsAny<EmailDto>()))
                           .ThrowsAsync(new InvalidOperationException("SMTP 服務無法連接"));

            // Act & Assert - 不應該拋出異常
            var exception = await Record.ExceptionAsync(async () =>
                await _service.SendLoginNotificationAsync(
                    tenantId: nameof(Tenants.SkyLabmgm),
                    userName: "testuser",
                    officialEmail: "user@test.com",
                    isSuccess: true,
                    failureReason: null,
                    ipAddress: "192.168.1.1"));

            Assert.Null(exception);
        }

        [Fact]
        public async Task SendLoginNotificationAsync_Should_Generate_HTML_Body_With_Correct_Structure()
        {
            // Arrange
            EmailDto? capturedEmail = null;
            _mockEmailService.Setup(x => x.SendAsync(It.IsAny<EmailDto>()))
                           .Callback<EmailDto>(dto => capturedEmail = dto);

            // Act
            await _service.SendLoginNotificationAsync(
                tenantId: nameof(Tenants.SkyLabmgm),
                userName: "testuser",
                officialEmail: "user@test.com",
                isSuccess: true,
                failureReason: null,
                ipAddress: "192.168.1.1");

            // Assert
            Assert.NotNull(capturedEmail);
            Assert.Contains("<html>", capturedEmail.Body);
            Assert.Contains("<body style=", capturedEmail.Body);
            Assert.Contains("</html>", capturedEmail.Body);
        }

        [Theory]
        [InlineData(nameof(Tenants.SkyLabmgm), "SKYLAB管理系統登入通知 - 登入成功")]
        [InlineData("SkyLabcommittee", "SkyLab委員系統登入通知 - 登入成功")]
        [InlineData("UnknownTenant", "系統登入通知 - 登入成功")]
        public async Task SendLoginNotificationAsync_Should_Use_Correct_Subject_For_Each_Tenant(string tenantId, string expectedSubject)
        {
            // Arrange
            EmailDto? capturedEmail = null;
            _mockEmailService.Setup(x => x.SendAsync(It.IsAny<EmailDto>()))
                           .Callback<EmailDto>(dto => capturedEmail = dto);

            // Act
            await _service.SendLoginNotificationAsync(
                tenantId: tenantId,
                userName: "testuser",
                officialEmail: "user@test.com",
                isSuccess: true,
                failureReason: null,
                ipAddress: "192.168.1.1");

            // Assert
            Assert.NotNull(capturedEmail);
            Assert.Equal(expectedSubject, capturedEmail.Subject);
        }
    }
}
