using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos;
using SkyLabIdP.Application.Dtos.User.Authentication;
using SkyLabIdP.Application.SystemApps.Users.Commands.LoginUser;
using SkyLabIdP.Application.SystemApps.Users.Commands.LoginUser.Services;
using SkyLabIdP.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Application.UnitTests.SystemApps.Users.Commands.LoginUser
{
    public class LoginUserCommandHandlerTests : IDisposable
    {
        private readonly Mock<ITenantUserServiceFactory> _loginServiceFactoryMock;
        private readonly Mock<ILogger<LoginUserCommandHandler>> _loggerMock;
        private readonly Mock<IUserService> _loginServiceMock;
        private readonly LoginUserCommandHandler _handler;

        public LoginUserCommandHandlerTests()
        {
            _loginServiceFactoryMock = new Mock<ITenantUserServiceFactory>();
            _loggerMock = new Mock<ILogger<LoginUserCommandHandler>>();
            _loginServiceMock = new Mock<IUserService>();

            // Setup factory to return our mock login service
            _loginServiceFactoryMock
                .Setup(f => f.GetServiceByTenantId(It.IsAny<string>()))
                .Returns(_loginServiceMock.Object);

            _handler = new LoginUserCommandHandler(_loginServiceFactoryMock.Object, _loggerMock.Object);
        }

        public void Dispose()
        {
            // Clean up resources if needed
        }

        // MockUserManager helper method from UserDetailWriterIntegrationTests.cs
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

        [Fact]
        public async Task Handle_InvalidPassword_ReturnsAuthenticateResponseWithForbiddenStatus()
        {
            // Arrange
            var command = new LoginUserCommand
            {
                UserName = "existinguser",
                Password = "wrongpassword",
                TenantId = "test-tenant"
            };

            var expectedResponse = new AuthenticateResponse
            {
                OperationResult = new OperationResult(false, "密碼錯誤", StatusCodes.Status403Forbidden)
            };

            _loginServiceMock
                .Setup(s => s.HandleLoginUserCommandAsync(It.IsAny<LoginUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.OperationResult.Success);
            Assert.Equal(StatusCodes.Status403Forbidden, result.OperationResult.StatusCode);
            
            _loginServiceFactoryMock.Verify(f => f.GetServiceByTenantId(command.TenantId), Times.Once);
            _loginServiceMock.Verify(s => s.HandleLoginUserCommandAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_AccountNotApproved_ReturnsAuthenticateResponseWithUnauthorizedStatus()
        {
            // Arrange
            var command = new LoginUserCommand
            {
                UserName = "notapproveduser",
                Password = "password",
                TenantId = "test-tenant"
            };

            var expectedResponse = new AuthenticateResponse
            {
                OperationResult = new OperationResult(false, "帳號未經審核", StatusCodes.Status401Unauthorized)
            };

            _loginServiceMock
                .Setup(s => s.HandleLoginUserCommandAsync(It.IsAny<LoginUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.OperationResult.Success);
            Assert.Equal(StatusCodes.Status401Unauthorized, result.OperationResult.StatusCode);
            
            _loginServiceFactoryMock.Verify(f => f.GetServiceByTenantId(command.TenantId), Times.Once);
            _loginServiceMock.Verify(s => s.HandleLoginUserCommandAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_AccountLocked_ReturnsAuthenticateResponseWithUnauthorizedStatus()
        {
            // Arrange
            var command = new LoginUserCommand
            {
                UserName = "lockeduser",
                Password = "password",
                TenantId = "test-tenant"
            };

            var expectedResponse = new AuthenticateResponse
            {
                OperationResult = new OperationResult(false, "帳號已被鎖定", StatusCodes.Status401Unauthorized)
            };

            _loginServiceMock
                .Setup(s => s.HandleLoginUserCommandAsync(It.IsAny<LoginUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.OperationResult.Success);
            Assert.Equal(StatusCodes.Status401Unauthorized, result.OperationResult.StatusCode);
            
            _loginServiceFactoryMock.Verify(f => f.GetServiceByTenantId(command.TenantId), Times.Once);
            _loginServiceMock.Verify(s => s.HandleLoginUserCommandAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_AccountInactive_ReturnsAuthenticateResponseWithUnauthorizedStatus()
        {
            // Arrange
            var command = new LoginUserCommand
            {
                UserName = "inactiveuser",
                Password = "password",
                TenantId = "test-tenant"
            };

            var expectedResponse = new AuthenticateResponse
            {
                OperationResult = new OperationResult(false, "帳號已停用", StatusCodes.Status401Unauthorized)
            };

            _loginServiceMock
                .Setup(s => s.HandleLoginUserCommandAsync(It.IsAny<LoginUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.OperationResult.Success);
            Assert.Equal(StatusCodes.Status401Unauthorized, result.OperationResult.StatusCode);
            
            _loginServiceFactoryMock.Verify(f => f.GetServiceByTenantId(command.TenantId), Times.Once);
            _loginServiceMock.Verify(s => s.HandleLoginUserCommandAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_PasswordExpired_ReturnsAuthenticateResponseWithForbiddenStatus()
        {
            // Arrange
            var command = new LoginUserCommand
            {
                UserName = "expiredpassworduser",
                Password = "password",
                TenantId = "test-tenant"
            };

            var expectedResponse = new AuthenticateResponse
            {
                OperationResult = new OperationResult(false, "密碼已過期", StatusCodes.Status403Forbidden)
            };

            _loginServiceMock
                .Setup(s => s.HandleLoginUserCommandAsync(It.IsAny<LoginUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.OperationResult.Success);
            Assert.Equal(StatusCodes.Status403Forbidden, result.OperationResult.StatusCode);
            
            _loginServiceFactoryMock.Verify(f => f.GetServiceByTenantId(command.TenantId), Times.Once);
            _loginServiceMock.Verify(s => s.HandleLoginUserCommandAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidLogin_ReturnsAuthenticateResponseWithSuccessStatus()
        {
            // Arrange
            var command = new LoginUserCommand
            {
                UserName = "validuser",
                Password = "password",
                TenantId = "test-tenant"
            };

            var expectedResponse = new AuthenticateResponse
            {
                OperationResult = new OperationResult(true, "登入成功", StatusCodes.Status200OK),
                AccessToken = "sample-jwt-token",
                RefreshToken = "sample-refresh-token"
            };

            _loginServiceMock
                .Setup(s => s.HandleLoginUserCommandAsync(It.IsAny<LoginUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.OperationResult.Success);
            Assert.Equal(StatusCodes.Status200OK, result.OperationResult.StatusCode);
            Assert.Equal("sample-jwt-token", result.AccessToken);
            Assert.Equal("sample-refresh-token", result.RefreshToken);
            
            _loginServiceFactoryMock.Verify(f => f.GetServiceByTenantId(command.TenantId), Times.Once);
            _loginServiceMock.Verify(s => s.HandleLoginUserCommandAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ServiceThrowsException_RethrowsException()
        {
            // Arrange
            var command = new LoginUserCommand
            {
                UserName = "testuser",
                Password = "password",
                TenantId = "test-tenant"
            };

            var expectedException = new InvalidOperationException("Service error");

            _loginServiceMock
                .Setup(s => s.HandleLoginUserCommandAsync(It.IsAny<LoginUserCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _handler.Handle(command, CancellationToken.None));
            
            Assert.Equal("Service error", exception.Message);
            
            _loginServiceFactoryMock.Verify(f => f.GetServiceByTenantId(command.TenantId), Times.Once);
            _loginServiceMock.Verify(s => s.HandleLoginUserCommandAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_FactoryReturnsCorrectServiceForTenantId()
        {
            // Arrange
            var tenantId = "specific-tenant";
            var command = new LoginUserCommand
            {
                UserName = "testuser",
                Password = "password",
                TenantId = tenantId
            };

            var expectedResponse = new AuthenticateResponse
            {
                OperationResult = new OperationResult(true, "登入成功", StatusCodes.Status200OK)
            };

            _loginServiceMock
                .Setup(s => s.HandleLoginUserCommandAsync(It.IsAny<LoginUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.OperationResult.Success);
            
            _loginServiceFactoryMock.Verify(f => f.GetServiceByTenantId(tenantId), Times.Once);
            _loginServiceMock.Verify(s => s.HandleLoginUserCommandAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_SkyLabCaedpLoginUser_UsesCorrectTenantService()
        {
            // Arrange
            var command = new LoginUserCommand
            {
                UserName = "caedpuser",
                Password = "password",
                TenantId = "skylabcaedp"
            };

            var expectedResponse = new AuthenticateResponse
            {
                OperationResult = new OperationResult(true, "登入成功", StatusCodes.Status200OK),
                AccessToken = "caedp-jwt-token",
                RefreshToken = "caedp-refresh-token"
            };

            _loginServiceMock
                .Setup(s => s.HandleLoginUserCommandAsync(It.IsAny<LoginUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.OperationResult.Success);
            Assert.Equal(StatusCodes.Status200OK, result.OperationResult.StatusCode);
            Assert.Equal("caedp-jwt-token", result.AccessToken);
            Assert.Equal("caedp-refresh-token", result.RefreshToken);
            
            // Verify that the factory was called with the correct tenant ID
            _loginServiceFactoryMock.Verify(f => f.GetServiceByTenantId("skylabcaedp"), Times.Once);
            _loginServiceMock.Verify(s => s.HandleLoginUserCommandAsync(
                It.Is<LoginUserCommand>(c => c.TenantId == "skylabcaedp" && c.UserName == "caedpuser"), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_SkyLabCommitteeLoginUser_UsesCorrectTenantService()
        {
            // Arrange
            var command = new LoginUserCommand
            {
                UserName = "committeeuser",
                Password = "password",
                TenantId = "skylabcommittee"
            };

            var expectedResponse = new AuthenticateResponse
            {
                OperationResult = new OperationResult(true, "登入成功", StatusCodes.Status200OK),
                AccessToken = "committee-jwt-token",
                RefreshToken = "committee-refresh-token"
            };

            _loginServiceMock
                .Setup(s => s.HandleLoginUserCommandAsync(It.IsAny<LoginUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.OperationResult.Success);
            Assert.Equal(StatusCodes.Status200OK, result.OperationResult.StatusCode);
            Assert.Equal("committee-jwt-token", result.AccessToken);
            Assert.Equal("committee-refresh-token", result.RefreshToken);
            
            // Verify that the factory was called with the correct tenant ID
            _loginServiceFactoryMock.Verify(f => f.GetServiceByTenantId("skylabcommittee"), Times.Once);
            _loginServiceMock.Verify(s => s.HandleLoginUserCommandAsync(
                It.Is<LoginUserCommand>(c => c.TenantId == "skylabcommittee" && c.UserName == "committeeuser"), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_SkyLabDevelopLoginUser_UsesCorrectTenantService()
        {
            // Arrange
            var command = new LoginUserCommand
            {
                UserName = "developuser",
                Password = "password",
                TenantId = "skylabdevelop"
            };

            var expectedResponse = new AuthenticateResponse
            {
                OperationResult = new OperationResult(true, "登入成功", StatusCodes.Status200OK),
                AccessToken = "develop-jwt-token",
                RefreshToken = "develop-refresh-token"
            };

            _loginServiceMock
                .Setup(s => s.HandleLoginUserCommandAsync(It.IsAny<LoginUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.OperationResult.Success);
            Assert.Equal(StatusCodes.Status200OK, result.OperationResult.StatusCode);
            Assert.Equal("develop-jwt-token", result.AccessToken);
            Assert.Equal("develop-refresh-token", result.RefreshToken);
            
            // Verify that the factory was called with the correct tenant ID
            _loginServiceFactoryMock.Verify(f => f.GetServiceByTenantId("skylabdevelop"), Times.Once);
            _loginServiceMock.Verify(s => s.HandleLoginUserCommandAsync(
                It.Is<LoginUserCommand>(c => c.TenantId == "skylabdevelop" && c.UserName == "developuser"), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_SkyLabExternalAgencyLoginUser_UsesCorrectTenantService()
        {
            // Arrange
            var command = new LoginUserCommand
            {
                UserName = "external@gov.tw",
                Password = "password",
                TenantId = "SkyLabExternalAgenc"
            };

            var expectedResponse = new AuthenticateResponse
            {
                OperationResult = new OperationResult(true, "登入成功", StatusCodes.Status200OK),
                AccessToken = "external-agency-jwt-token",
                RefreshToken = "external-agency-refresh-token"
            };

            _loginServiceMock
                .Setup(s => s.HandleLoginUserCommandAsync(It.IsAny<LoginUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.OperationResult.Success);
            Assert.Equal(StatusCodes.Status200OK, result.OperationResult.StatusCode);
            Assert.Equal("external-agency-jwt-token", result.AccessToken);
            Assert.Equal("external-agency-refresh-token", result.RefreshToken);
            
            // Verify that the factory was called with the correct tenant ID
            _loginServiceFactoryMock.Verify(f => f.GetServiceByTenantId("SkyLabExternalAgenc"), Times.Once);
            _loginServiceMock.Verify(s => s.HandleLoginUserCommandAsync(
                It.Is<LoginUserCommand>(c => c.TenantId == "SkyLabExternalAgenc" && c.UserName == "external@gov.tw"), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_SkyLabExternalAgencyInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var command = new LoginUserCommand
            {
                UserName = "external@gov.tw",
                Password = "wrongpassword",
                TenantId = "SkyLabExternalAgenc"
            };

            var expectedResponse = new AuthenticateResponse
            {
                OperationResult = new OperationResult(false, "帳號或密碼錯誤", StatusCodes.Status401Unauthorized)
            };

            _loginServiceMock
                .Setup(s => s.HandleLoginUserCommandAsync(It.IsAny<LoginUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.OperationResult.Success);
            Assert.Equal(StatusCodes.Status401Unauthorized, result.OperationResult.StatusCode);
            Assert.Contains("帳號或密碼錯誤", result.OperationResult.Messages.FirstOrDefault());
            
            // Verify that the factory was called with the correct tenant ID
            _loginServiceFactoryMock.Verify(f => f.GetServiceByTenantId("SkyLabExternalAgenc"), Times.Once);
            _loginServiceMock.Verify(s => s.HandleLoginUserCommandAsync(
                It.Is<LoginUserCommand>(c => c.TenantId == "SkyLabExternalAgenc" && c.UserName == "external@gov.tw"), 
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}