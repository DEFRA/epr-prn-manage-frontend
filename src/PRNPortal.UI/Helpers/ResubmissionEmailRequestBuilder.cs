using EPR.Common.Authorization.Models;
using PRNPortal.Application.DTOs.Submission;
using PRNPortal.Application.RequestModels;
using PRNPortal.UI.Constants;
using PRNPortal.UI.Sessions;

namespace PRNPortal.UI.Helpers
{
    public static class ResubmissionEmailRequestBuilder
    {
        public static ResubmissionEmailRequestModel BuildResubmissionEmail(UserData userData, PomSubmission? submission, FrontendSchemeRegistrationSession? session)
        {
            var organisation = userData.Organisations.FirstOrDefault();

            var input = new ResubmissionEmailRequestModel
            {
                OrganisationNumber = organisation.OrganisationNumber,
                ProducerOrganisationName = organisation.Name,
                SubmissionPeriod = submission.SubmissionPeriod,
                NationId = (int)organisation.NationId,
                IsComplianceScheme = organisation.OrganisationRole == OrganisationRoles.ComplianceScheme,
            };

            if (input.IsComplianceScheme)
            {
                input.ProducerOrganisationName = session.RegistrationSession.SelectedComplianceScheme?.Name;
                input.ComplianceSchemeName = organisation.Name;
                input.ComplianceSchemePersonName = $"{userData.FirstName} {userData.LastName}";
                input.NationId = (int)session.RegistrationSession.SelectedComplianceScheme?.NationId;
            }

            return input;
        }
    }
}
