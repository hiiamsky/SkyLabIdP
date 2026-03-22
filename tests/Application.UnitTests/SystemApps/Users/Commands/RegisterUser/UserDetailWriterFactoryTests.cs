using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos.User.Registration;
using SkyLabIdP.Application.SystemApps.Users.Commands.RegisterUser;
using SkyLabIdP.Application.SystemApps.Users.Commands.Writers;
using SkyLabIdP.Domain.Entities;
using SkyLabIdP.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using Xunit;

namespace Application.UnitTests.SystemApps.Users.Commands.RegisterUser
{
    public class UserDetailWriterFactoryTests
    {
        [Fact]
        public void GetWriter_WithValidTenantId_ReturnsCorrectWriter()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var skylabDocUserDetailWriter = new Mock<IUserDetailWriter<BaseUserRegistrationRequest, BaseUserRegistrationResponse>>();
            
            serviceProviderMock
                .Setup(x => x.GetService(typeof(SkyLabDocUserDetailWriter)))
                .Returns(skylabDocUserDetailWriter.Object);

            var factory = new UserDetailWriterFactory(serviceProviderMock.Object);

            // Act
            var writer = factory.GetWriter<BaseUserRegistrationRequest, BaseUserRegistrationResponse>(Tenants.SkyLabmgm.ToString());

            // Assert
            Assert.NotNull(writer);
            Assert.Same(skylabDocUserDetailWriter.Object, writer);
        }



        [Fact]
        public void GetWriter_WithSkyLabDevelopTenantId_ReturnsSkyLabDevelopUserDetailWriter()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var skylabDevelopUserDetailWriter = new Mock<IUserDetailWriter<SkyLabDevelopUserRegistrationRequest, SkyLabDevelopUserRegistrationResponse>>();
            
            serviceProviderMock
                .Setup(x => x.GetService(typeof(SkyLabDevelopUserDetailWriter)))
                .Returns(skylabDevelopUserDetailWriter.Object);

            var factory = new UserDetailWriterFactory(serviceProviderMock.Object);

            // Act
            var writer = factory.GetWriter<SkyLabDevelopUserRegistrationRequest, SkyLabDevelopUserRegistrationResponse>(Tenants.SkyLabdevelop.ToString());

            // Assert
            Assert.NotNull(writer);
            Assert.Same(skylabDevelopUserDetailWriter.Object, writer);
        }



        [Fact]
        public void GetWriter_WithInvalidTenantId_ThrowsException()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var factory = new UserDetailWriterFactory(serviceProviderMock.Object);

            // Act & Assert
            var exception = Assert.Throws<NotSupportedException>(() => 
                factory.GetWriter<BaseUserRegistrationRequest, BaseUserRegistrationResponse>("InvalidTenantId"));
            
            Assert.Contains("找不到租戶", exception.Message);
        }

        [Fact]
        public void GetWriter_WithEmptyTenantId_ThrowsArgumentNullException()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var factory = new UserDetailWriterFactory(serviceProviderMock.Object);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                factory.GetWriter<BaseUserRegistrationRequest, BaseUserRegistrationResponse>(string.Empty));
        }

        [Fact]
        public void GetWriter_WithNullTenantId_ThrowsArgumentNullException()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var factory = new UserDetailWriterFactory(serviceProviderMock.Object);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                factory.GetWriter<BaseUserRegistrationRequest, BaseUserRegistrationResponse>(tenantId: null!));
        }

        [Fact]
        public void GetWriter_WithInvalidWriterType_ThrowsInvalidOperationException()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            
            // 故意返回錯誤類型的服務，以模擬無法轉換為預期介面的情況
            serviceProviderMock
                .Setup(x => x.GetService(typeof(SkyLabDocUserDetailWriter)))
                .Returns(new object());

            var factory = new UserDetailWriterFactory(serviceProviderMock.Object);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => 
                factory.GetWriter<BaseUserRegistrationRequest, BaseUserRegistrationResponse>(Tenants.SkyLabmgm.ToString()));
            
            Assert.Contains("無法將租戶", exception.Message);
        }




    }
}