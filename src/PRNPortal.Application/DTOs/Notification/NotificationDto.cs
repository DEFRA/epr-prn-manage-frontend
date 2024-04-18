using System.Diagnostics.CodeAnalysis;

namespace PRNPortal.Application.DTOs.Notification
{
    [ExcludeFromCodeCoverage]
    public class NotificationDto
    {
        public string Type { get; set; }

        public ICollection<KeyValuePair<string, string>> Data { get; set; }
    }
}
