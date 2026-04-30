using Microsoft.Extensions.Logging;
using Moq;
using StayFit.Application.DTOs;
using StayFit.Application.Services;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;

namespace StayFit.Tests.Services;

public sealed class SupportServiceTests
{
    private readonly Mock<ISupportRepository> _supportRepositoryMock;
    private readonly Mock<ILogger<SupportService>> _loggerMock;

    public SupportServiceTests()
    {
        _supportRepositoryMock = new Mock<ISupportRepository>();
        _loggerMock = new Mock<ILogger<SupportService>>();
    }

    private SupportService CreateSut()
    {
        return new SupportService(_supportRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateTicketAsync_WhenRequestIsValid_ReturnsNewTicketId()
    {
        // Arrange
        var userId = 42;
        var request = new CreateSupportTicketRequestDto
        {
            Subject = "Login issue",
            Message = "I cannot log in to my account"
        };

        SupportTicket? capturedTicket = null;
        _supportRepositoryMock
            .Setup(r => r.AddTicketAsync(It.IsAny<SupportTicket>()))
            .Callback<SupportTicket>(ticket =>
            {
                capturedTicket = ticket;
                ticket.Id = 123;
            })
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        // Act
        var result = await sut.CreateTicketAsync(userId, request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(123, result.Value);
        Assert.NotNull(capturedTicket);
        Assert.Equal(userId, capturedTicket!.UserId);
        Assert.Equal("Login issue", capturedTicket.Subject);
        Assert.Equal("I cannot log in to my account", capturedTicket.Message);
        Assert.Equal("New", capturedTicket.Status);
        _supportRepositoryMock.Verify(r => r.AddTicketAsync(It.IsAny<SupportTicket>()), Times.Once);
    }

    [Fact]
    public async Task CreateTicketAsync_WhenSubjectIsEmpty_ReturnsFailure()
    {
        // Arrange
        var request = new CreateSupportTicketRequestDto
        {
            Subject = "   ",
            Message = "Valid message"
        };

        var sut = CreateSut();

        // Act
        var result = await sut.CreateTicketAsync(1, request);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Тема звернення обов'язкова", result.Errors);
        _supportRepositoryMock.Verify(r => r.AddTicketAsync(It.IsAny<SupportTicket>()), Times.Never);
    }

    [Fact]
    public async Task CreateTicketAsync_WhenMessageIsEmpty_ReturnsFailure()
    {
        // Arrange
        var request = new CreateSupportTicketRequestDto
        {
            Subject = "Valid subject",
            Message = ""
        };

        var sut = CreateSut();

        // Act
        var result = await sut.CreateTicketAsync(1, request);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Текст звернення обов'язковий", result.Errors);
        _supportRepositoryMock.Verify(r => r.AddTicketAsync(It.IsAny<SupportTicket>()), Times.Never);
    }

    [Fact]
    public async Task CreateTicketAsync_WhenUserIdIsInvalid_ReturnsFailure()
    {
        // Arrange
        var request = new CreateSupportTicketRequestDto
        {
            Subject = "Subject",
            Message = "Message"
        };

        var sut = CreateSut();

        // Act
        var result = await sut.CreateTicketAsync(0, request);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Некоректний користувач", result.Errors);
        _supportRepositoryMock.Verify(r => r.AddTicketAsync(It.IsAny<SupportTicket>()), Times.Never);
    }

    [Fact]
    public async Task CreateTicketAsync_WhenRepositoryThrows_ReturnsFailure()
    {
        // Arrange
        var request = new CreateSupportTicketRequestDto
        {
            Subject = "Subject",
            Message = "Message"
        };

        _supportRepositoryMock
            .Setup(r => r.AddTicketAsync(It.IsAny<SupportTicket>()))
            .ThrowsAsync(new Exception("Database error"));

        var sut = CreateSut();

        // Act
        var result = await sut.CreateTicketAsync(1, request);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Помилка при створенні звернення", result.Errors);
    }

    [Fact]
    public async Task GetMyTicketsAsync_WhenTicketsExist_ReturnsDtos()
    {
        // Arrange
        var userId = 10;
        var tickets = new List<SupportTicket>
        {
            new()
            {
                Id = 1,
                UserId = userId,
                Subject = "First",
                Message = "First message",
                Status = "New",
                CreatedAt = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = 2,
                UserId = userId,
                Subject = "Second",
                Message = "Second message",
                Status = "Closed",
                CreatedAt = new DateTime(2026, 4, 2, 10, 0, 0, DateTimeKind.Utc)
            }
        };

        _supportRepositoryMock
            .Setup(r => r.GetTicketsByUserIdAsync(userId))
            .ReturnsAsync(tickets);

        var sut = CreateSut();

        // Act
        var result = await sut.GetMyTicketsAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        var dtoList = result.Value.ToList();
        Assert.Equal(2, dtoList.Count);
        Assert.Equal("First", dtoList[0].Subject);
        Assert.Equal("New", dtoList[0].Status);
        Assert.Equal("Second", dtoList[1].Subject);
        Assert.Equal("Closed", dtoList[1].Status);
        _supportRepositoryMock.Verify(r => r.GetTicketsByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetMyTicketsAsync_WhenRepositoryReturnsEmptyList_ReturnsEmptyDtos()
    {
        // Arrange
        _supportRepositoryMock
            .Setup(r => r.GetTicketsByUserIdAsync(10))
            .ReturnsAsync(new List<SupportTicket>());

        var sut = CreateSut();

        // Act
        var result = await sut.GetMyTicketsAsync(10);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetMyTicketsAsync_WhenRepositoryThrows_ReturnsFailure()
    {
        // Arrange
        _supportRepositoryMock
            .Setup(r => r.GetTicketsByUserIdAsync(10))
            .ThrowsAsync(new Exception("Database error"));

        var sut = CreateSut();

        // Act
        var result = await sut.GetMyTicketsAsync(10);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Помилка при отриманні звернень", result.Errors);
    }

    [Fact]
    public async Task GetTicketRepliesAsync_WhenRepliesExist_ReturnsDtos()
    {
        // Arrange
        var userId = 10;
        var ticketId = 15;
        var replies = new List<SupportTicketReply>
        {
            new()
            {
                Id = 1,
                TicketId = ticketId,
                Message = "Please try again",
                CreatedAt = new DateTime(2026, 4, 3, 8, 0, 0, DateTimeKind.Utc),
                IsAdminReply = true
            },
            new()
            {
                Id = 2,
                TicketId = ticketId,
                Message = "I tried it",
                CreatedAt = new DateTime(2026, 4, 3, 9, 0, 0, DateTimeKind.Utc),
                IsAdminReply = false
            }
        };

        _supportRepositoryMock
            .Setup(r => r.GetTicketByIdAsync(ticketId, userId))
            .ReturnsAsync(new SupportTicket { Id = ticketId, Subject = "Subject", Message = "Message", Status = "New" });

        _supportRepositoryMock
            .Setup(r => r.GetRepliesByTicketIdAsync(ticketId, userId))
            .ReturnsAsync(replies);

        var sut = CreateSut();

        // Act
        var result = await sut.GetTicketRepliesAsync(userId, ticketId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        var dtoList = result.Value.Replies!.ToList();
        Assert.Equal(2, dtoList.Count);
        Assert.Equal("Please try again", dtoList[0].Message);
        Assert.True(dtoList[0].IsAdminReply);
        Assert.Equal("I tried it", dtoList[1].Message);
        Assert.False(dtoList[1].IsAdminReply);
        _supportRepositoryMock.Verify(r => r.GetRepliesByTicketIdAsync(ticketId, userId), Times.Once);
    }

    [Fact]
    public async Task GetTicketRepliesAsync_WhenRepositoryReturnsEmptyList_ReturnsEmptyDtos()
    {
        // Arrange
        _supportRepositoryMock
            .Setup(r => r.GetTicketByIdAsync(15, 10))
            .ReturnsAsync(new SupportTicket { Id = 15, Subject = "Subject", Message = "Message", Status = "New" });

        _supportRepositoryMock
            .Setup(r => r.GetRepliesByTicketIdAsync(15, 10))
            .ReturnsAsync(new List<SupportTicketReply>());

        var sut = CreateSut();

        // Act
        var result = await sut.GetTicketRepliesAsync(10, 15);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value.Replies!);
    }

    [Fact]
    public async Task GetTicketRepliesAsync_WhenRepositoryThrows_ReturnsFailure()
    {
        // Arrange
        _supportRepositoryMock
            .Setup(r => r.GetTicketByIdAsync(15, 10))
            .ReturnsAsync(new SupportTicket { Id = 15, Subject = "Subject", Message = "Message", Status = "New" });

        _supportRepositoryMock
            .Setup(r => r.GetRepliesByTicketIdAsync(15, 10))
            .ThrowsAsync(new Exception("Database error"));

        var sut = CreateSut();

        // Act
        var result = await sut.GetTicketRepliesAsync(10, 15);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Помилка при отриманні деталей звернення", result.Errors);
    }
}
