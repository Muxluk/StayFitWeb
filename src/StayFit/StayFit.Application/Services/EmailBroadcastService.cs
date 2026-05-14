using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StayFit.Application.Interfaces;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;

namespace StayFit.Application.Services
{
    public class EmailBroadcastService : IEmailBroadcastService
    {
        private readonly IEmailBroadcastRepository _broadcastRepository;
        private readonly IEmailSender _emailSender;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<EmailBroadcastService> _logger;

        public EmailBroadcastService(
            IEmailBroadcastRepository broadcastRepository,
            IEmailSender emailSender,
            IUserRepository userRepository,
            ILogger<EmailBroadcastService> logger)
        {
            _broadcastRepository = broadcastRepository;
            _emailSender = emailSender;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task SendBroadcastAsync(string adminId, string subject, string body, string audience)
        {
            var recipients = await GetRecipientsByAudience(audience);
            int.TryParse(adminId, out var adminUserId);
            foreach (var user in recipients)
            {
                try
                {
                    await _emailSender.SendAsync(user.Email, subject, body);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to send email to {user.Email}");
                }
            }
            var broadcast = new EmailBroadcast
            {
                AdminUserId = adminUserId,
                Subject = subject,
                HtmlBody = body,
                SentAt = DateTime.UtcNow,
                RecipientCount = recipients.Count,
                Status = "Sent"
            };
            await _broadcastRepository.AddAsync(broadcast);
        }

        public async Task<IEnumerable<EmailBroadcast>> GetHistoryAsync()
        {
            return await _broadcastRepository.GetAllAsync();
        }

        private async Task<List<User>> GetRecipientsByAudience(string audience)
        {
            if (audience == "All")
                return (await _userRepository.GetAllAsync()).ToList();
            if (audience == "Active")
                return (await _userRepository.GetActiveAsync()).ToList();
            if (audience.StartsWith("Role:"))
            {
                var role = audience.Substring(5);
                return (await _userRepository.GetByRoleAsync(role)).ToList();
            }
            return new List<User>();
        }
    }
}
