using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using StayFit.Application.Interfaces;
using StayFit.Application.Services;
using StayFit.Domain.Entities;
using Xunit;

namespace StayFit.Tests.Services
{
    public class EmailBroadcastServiceTests
    {
        [Fact]
        public async Task SendBroadcastAsync_SendsToAll_SavesHistory()
        {
            // Arrange
            var repoMock = new Mock<IEmailBroadcastRepository>();
            var emailSenderMock = new Mock<IEmailSender>();
            var userRepoMock = new Mock<IUserRepository>();
            var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<EmailBroadcastService>>();
            userRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User> { new User { Email = "a@a.com" } });
            var service = new EmailBroadcastService(repoMock.Object, emailSenderMock.Object, userRepoMock.Object, loggerMock.Object);

            // Act
            await service.SendBroadcastAsync("admin", "subject", "body", "All");

            // Assert
            emailSenderMock.Verify(s => s.SendAsync("a@a.com", "subject", "body", It.IsAny<CancellationToken>()), Times.Once);
            repoMock.Verify(r => r.AddAsync(It.IsAny<EmailBroadcast>()), Times.Once);
        }

        [Fact]
        public async Task SendBroadcastAsync_EmptyAudience_NoSend()
        {
            var repoMock = new Mock<IEmailBroadcastRepository>();
            var emailSenderMock = new Mock<IEmailSender>();
            var userRepoMock = new Mock<IUserRepository>();
            var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<EmailBroadcastService>>();
            userRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User>());
            var service = new EmailBroadcastService(repoMock.Object, emailSenderMock.Object, userRepoMock.Object, loggerMock.Object);

            await service.SendBroadcastAsync("admin", "subject", "body", "All");

            emailSenderMock.Verify(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            repoMock.Verify(r => r.AddAsync(It.IsAny<EmailBroadcast>()), Times.Once);
        }
    }
}
