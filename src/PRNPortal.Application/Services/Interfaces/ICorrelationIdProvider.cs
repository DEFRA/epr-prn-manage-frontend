namespace PRNPortal.Application.Services.Interfaces;

public interface ICorrelationIdProvider
{
    public Guid GetCurrentCorrelationIdOrNew();
}