using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Common.Security;
using SkyLabIdP.Application.Dtos;
using SkyLabIdP.Application.Dtos.Email;
using SkyLabIdP.Application.SystemApps.Users.Commands.ForgotPassword;
using SkyLabIdP.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.UnitTests.SystemApps.Users.Commands.ForgotPassword
{
    public class ForgotPasswordCommandHandlerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IDataProtectionService> _dataProtectionServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IUrlWhitelistValidator> _urlValidatorMock;
        private readonly Mock<ILogger<ForgotPasswordCommandHandler>> _loggerMock;
        private readonly ForgotPasswordCommandHandler _handler;

        public ForgotPasswordCommandHandlerTests()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            _dataProtectionServiceMock = new Mock<IDataProtectionService>();
            _emailServiceMock = new Mock<IEmailService>();
            _configurationMock = new Mock<IConfiguration>();
            _urlValidatorMock = new Mock<IUrlWhitelistValidator>();
            _loggerMock = new Mock<ILogger<ForgotPasswordCommandHandler>>();

            // URL validator 預設允許所有 URL
            _urlValidatorMock.Setup(x => x.ValidateUrl(It.IsAny<string>()))
                .Returns((true, (string?)null));

            // 設定預設配置
            var configSection = new Mock<IConfigurationSection>();
            _configurationMock.Setup(x => x["ResetPasswordUrl"]).Returns("https://default.skylab.com.tw/reset-password");
            _configurationMock.Setup(x => x.GetSection("TenantResetPasswordUrls")).Returns(configSection.Object);

            _handler = new ForgotPasswordCommandHandler(
                _userManagerMock.Object,
                _emailServiceMock.Object,
                _configurationMock.Object,
                _dataProtectionServiceMock.Object,
                _urlValidatorMock.Object,
                _loggerMock.Object);
        }

        [Theory]
        [InlineData("SkyLabmgm", "https://skylab.skylab.com.tw/skylabmgm/reset-password")]
        [InlineData("SkyLabcommittee", "https://skylab.skylab.com.tw/skylabmeeting/reset-password")]
        [InlineData("SkyLabcaedp", "https://skylab.skylab.com.tw/skylabcaedp/reset-password")]
        [InlineData("SkyLabdevelop", "https://develop.skylab.com.tw:3701/skylabdevelop/reset-password")]
        [InlineData("SkyLabCollaborativeAgency", "https://skylab.skylab.com.tw/collaborative/reset-password")]
        public async Task Handle_WithValidTenant_ShouldGenerateCorrectResetPasswordUrl(string tenantId, string expectedUrl)
        {
            // Arrange
            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser", Email = "test@example.com" };
            var command = new ForgotPasswordCommand
            {
                Username = "testuser",
                Email = "test@example.com",
                TenantId = tenantId
            };

            _userManagerMock.Setup(x => x.FindByNameAsync(command.Username))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("reset-token");
            _dataProtectionServiceMock.Setup(x => x.Protect("reset-token"))
                .Returns("encrypted-token");
            _dataProtectionServiceMock.Setup(x => x.Protect(user.Id))
                .Returns("encrypted-user-id");

            // 設定租戶特定的 URL
            var tenantSection = new Mock<IConfigurationSection>();
            tenantSection.Setup(x => x[tenantId]).Returns(expectedUrl);
            _configurationMock.Setup(x => x.GetSection("TenantResetPasswordUrls")).Returns(tenantSection.Object);

            _emailServiceMock.Setup(x => x.SendAsync(It.IsAny<EmailDto>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

            // 驗證電子郵件是否發送並包含正確的 URL
            _emailServiceMock.Verify(x => x.SendAsync(It.Is<EmailDto>(email => 
                email.Body.Contains(expectedUrl) && 
                email.Body.Contains("tenantId=" + tenantId))), Times.Once);
        }

        [Fact]
        public async Task Handle_WithUnknownTenant_ShouldUseDefaultResetPasswordUrl()
        {
            // Arrange
            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser", Email = "test@example.com" };
            var command = new ForgotPasswordCommand
            {
                Username = "testuser",
                Email = "test@example.com",
                TenantId = "UnknownTenant"
            };

            _userManagerMock.Setup(x => x.FindByNameAsync(command.Username))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("reset-token");
            _dataProtectionServiceMock.Setup(x => x.Protect("reset-token"))
                .Returns("encrypted-token");
            _dataProtectionServiceMock.Setup(x => x.Protect(user.Id))
                .Returns("encrypted-user-id");

            // 設定租戶配置為空（未知租戶）
            var tenantSection = new Mock<IConfigurationSection>();
            tenantSection.Setup(x => x["UnknownTenant"]).Returns((string?)null);
            _configurationMock.Setup(x => x.GetSection("TenantResetPasswordUrls")).Returns(tenantSection.Object);

            _emailServiceMock.Setup(x => x.SendAsync(It.IsAny<EmailDto>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

            // 驗證電子郵件是否發送並包含預設 URL
            _emailServiceMock.Verify(x => x.SendAsync(It.Is<EmailDto>(email => 
                email.Body.Contains("https://default.skylab.com.tw/reset-password") && 
                email.Body.Contains("tenantId=UnknownTenant"))), Times.Once);
        }

        [Fact]
        public async Task Handle_WithEmptyTenantId_ShouldUseDefaultResetPasswordUrl()
        {
            // Arrange
            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser", Email = "test@example.com" };
            var command = new ForgotPasswordCommand
            {
                Username = "testuser",
                Email = "test@example.com",
                TenantId = ""
            };

            _userManagerMock.Setup(x => x.FindByNameAsync(command.Username))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("reset-token");
            _dataProtectionServiceMock.Setup(x => x.Protect("reset-token"))
                .Returns("encrypted-token");
            _dataProtectionServiceMock.Setup(x => x.Protect(user.Id))
                .Returns("encrypted-user-id");

            _emailServiceMock.Setup(x => x.SendAsync(It.IsAny<EmailDto>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

            // 驗證電子郵件是否發送並包含預設 URL
            _emailServiceMock.Verify(x => x.SendAsync(It.Is<EmailDto>(email => 
                email.Body.Contains("https://default.skylab.com.tw/reset-password") && 
                email.Body.Contains("tenantId="))), Times.Once);
        }

        [Fact]
        public async Task Handle_WithInvalidUser_ShouldReturnFailure()
        {
            // Arrange
            var command = new ForgotPasswordCommand
            {
                Username = "nonexistentuser",
                Email = "test@example.com",
                TenantId = "SkyLabmgm"
            };

            _userManagerMock.Setup(x => x.FindByNameAsync(command.Username))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
            Assert.Contains("使用者資訊錯誤，帳號或是email錯誤", result.Messages);

            // 驗證沒有發送電子郵件
            _emailServiceMock.Verify(x => x.SendAsync(It.IsAny<EmailDto>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithMismatchedEmail_ShouldReturnFailure()
        {
            // Arrange
            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser", Email = "user@example.com" };
            var command = new ForgotPasswordCommand
            {
                Username = "testuser",
                Email = "different@example.com", // 不同的電子郵件
                TenantId = "SkyLabmgm"
            };

            _userManagerMock.Setup(x => x.FindByNameAsync(command.Username))
                .ReturnsAsync(user);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
            Assert.Contains("使用者資訊錯誤，帳號或是email錯誤", result.Messages);

            // 驗證沒有發送電子郵件
            _emailServiceMock.Verify(x => x.SendAsync(It.IsAny<EmailDto>()), Times.Never);
        }
    }
}
