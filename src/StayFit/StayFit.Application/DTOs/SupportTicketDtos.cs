namespace StayFit.Application.DTOs;

public sealed class CreateSupportTicketRequestDto
{
    public string Subject { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public sealed class SupportTicketDto
{
    public int Id { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public sealed class SupportTicketReplyDto
{
    public int Id { get; init; }
    public int TicketId { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public bool IsAdminReply { get; init; }
}
