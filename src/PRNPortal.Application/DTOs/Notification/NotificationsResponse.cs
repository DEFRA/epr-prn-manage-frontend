using System.Diagnostics.CodeAnalysis;

namespace PRNPortal.Application.DTOs.Notification
{
    [ExcludeFromCodeCoverage]
    public class NotificationsResponse
    {
        public List<NotificationDto> Notifications { get; set; }
    }
}
