namespace PRNPortal.Application.Services.Interfaces;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}