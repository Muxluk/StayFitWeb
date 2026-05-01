using System;

namespace StayFit.Domain.Entities
{
    public class EmailBroadcast
    {
        public int Id { get; set; }
        public string AdminId { get; set; } = string.Empty; // Id адміністратора, який ініціював розсилку
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty; // Наприклад: "All", "Active", "Role:User"
        public DateTime SentAt { get; set; }
        public int RecipientCount { get; set; }
    }
}
