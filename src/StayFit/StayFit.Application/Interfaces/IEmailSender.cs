namespace StayFit.Application.Interfaces;

/// <summary>
/// Абстракція для відправки email-повідомлень.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default);
}
