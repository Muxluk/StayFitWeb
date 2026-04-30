using StayFit.Domain.Enums;

namespace StayFit.Application.DTOs;

public class SupportTicketAdminDto
{
    public int Id { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public SupportStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Текст самого звернення для сторінки деталей
    public string Message { get; set; } = string.Empty;
    
    // Історія повідомлень (відповіді)
    public List<SupportTicketReplyAdminDto> Replies { get; set; } = new();
}

public class SupportTicketReplyAdminDto
{
    public string Message { get; set; } = string.Empty;
    public bool IsAdminReply { get; set; }
    public DateTime CreatedAt { get; set; }
}