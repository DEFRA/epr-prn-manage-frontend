using PRNPortal.Application.DTOs.Submission;
using PRNPortal.UI.Constants;

namespace PRNPortal.UI.ViewModels;

public class FileUploadSubLandingViewModel
{
    public string? ComplianceSchemeName { get; set; }

    public List<SubmissionPeriodDetail> SubmissionPeriodDetails { get; set; }

    public string? OrganisationRole { get; set; }

    public bool IsComplianceScheme => OrganisationRole == OrganisationRoles.ComplianceScheme;

    public string ServiceRole { get; set; } = "Basic User";
}