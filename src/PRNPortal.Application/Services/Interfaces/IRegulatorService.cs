using PRNPortal.Application.RequestModels;

namespace PRNPortal.Application.Services.Interfaces
{
    public interface IRegulatorService
    {
        Task<string> SendRegulatorResubmissionEmail(ResubmissionEmailRequestModel input);
    }
}