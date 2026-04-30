using Microsoft.Extensions.Logging;
using Moq;
using StayFit.Application.DTOs;
using StayFit.Application.Services;
using StayFit.Domain.Entities;
using StayFit.Domain.Enums;
using StayFit.Domain.Interfaces;
using Xunit;

namespace StayFit.Tests.Services;

public class AdminSupportServiceTests
{
    private readonly Mock<ISupportRepository> _repositoryMock;
    private readonly Mock<ILogger<SupportService>> _loggerMock;
    private readonly SupportService _supportService;

    public AdminSupportServiceTests()
    {
        _repositoryMock = new Mock<ISupportRepository>();
        _loggerMock = new Mock<ILogger<SupportService>>();
        
        _supportService = new SupportService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAdminTicketsAsync_ReturnsPagedResult()
    {
        // Arrange
        var tickets = new List<SupportTicket>
        {
            // Додали .ToString()
            new SupportTicket { Id = 1, Status = SupportStatus.New.ToString(), User = new User { Email = "test@user.com"} }
        };

        _repositoryMock.Setup(r => r.GetTicketsCountAsync(null)).ReturnsAsync(1);
        _repositoryMock.Setup(r => r.GetAllTicketsAsync(null, 0, 10)).ReturnsAsync(tickets);

        // Act
        var result = await _supportService.GetAdminTicketsAsync(null, 1, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task ChangeTicketStatusAsync_ValidId_UpdatesStatus()
    {
        // Arrange
        // Додали .ToString()
        var ticket = new SupportTicket { Id = 1, Status = SupportStatus.New.ToString() };
        _repositoryMock.Setup(r => r.GetTicketWithRepliesByIdAsync(1)).ReturnsAsync(ticket);

        // Act
        var result = await _supportService.ChangeTicketStatusAsync(1, SupportStatus.InProgress);

        // Assert
        Assert.True(result);
        // Додали .ToString() для порівняння
        Assert.Equal(SupportStatus.InProgress.ToString(), ticket.Status);
        _repositoryMock.Verify(r => r.UpdateTicketAsync(ticket), Times.Once);
    }

    [Fact]
    public async Task ReplyToTicketAsync_AddsReplyAndClosesTicket()
    {
        // Arrange
        // Додали .ToString()
        var ticket = new SupportTicket { Id = 1, Status = SupportStatus.New.ToString() };
        var replyDto = new SupportReplyDto { TicketId = 1, ReplyMessage = "Admin reply" };

        _repositoryMock.Setup(r => r.GetTicketWithRepliesByIdAsync(1)).ReturnsAsync(ticket);

        // Act
        var result = await _supportService.ReplyToTicketAsync(replyDto);

        // Assert
        Assert.True(result);
        // Додали .ToString() для порівняння
        Assert.Equal(SupportStatus.Closed.ToString(), ticket.Status);
        _repositoryMock.Verify(r => r.AddReplyAsync(It.Is<SupportTicketReply>(r => r.Message == "Admin reply" && r.IsAdminReply)), Times.Once);
        _repositoryMock.Verify(r => r.UpdateTicketAsync(ticket), Times.Once);
    }
}