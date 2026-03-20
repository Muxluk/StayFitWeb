using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using StayFit.Application.Interfaces;

namespace StayFit.Infrastructure.Services;

/// <summary>
/// Реалізація відправки email через SMTP (MailKit).
/// Налаштовується через appsettings.json → секція "Smtp".
/// </summary>
public sealed class SmtpEmailSender(
    IConfiguration configuration,
    ILogger<SmtpEmailSender> logger)
    : IEmailSender
{
    public async Task SendAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        var smtpSection = configuration.GetSection("Smtp");

        var host = smtpSection["Host"] ?? throw new InvalidOperationException("Smtp:Host is not configured.");
        var port = int.Parse(smtpSection["Port"] ?? "587");
        var user = smtpSection["User"] ?? throw new InvalidOperationException("Smtp:User is not configured.");
        var password = smtpSection["Password"] ?? throw new InvalidOperationException("Smtp:Password is not configured.");
        var fromName = smtpSection["FromName"] ?? "StayFit";
        var fromAddress = smtpSection["FromAddress"] ?? user;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromAddress));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();

        logger.LogInformation("Sending email to {ToEmail} via {Host}:{Port}", toEmail, host, port);

        await client.ConnectAsync(host, port, SecureSocketOptions.StartTls, cancellationToken);
        await client.AuthenticateAsync(user, password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(quit: true, cancellationToken);

        logger.LogInformation("Email sent successfully to {ToEmail}. Subject: {Subject}", toEmail, subject);
    }
}
