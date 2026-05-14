using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace StayFit.Web.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    // Клієнти підключаються сюди автоматично.
    // SignalR прив'язує з'єднання до UserId через ClaimTypes.NameIdentifier.
    // BackgroundService надсилає повідомлення через IHubContext<NotificationHub>.
}
