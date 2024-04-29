namespace PRNPortal.UI.ViewModels;

using Application.DTOs.Submission;

public class HomePageSelfManagedViewModel
{
    public string OrganisationName { get; set; } =  string.Empty;

    public string OrganisationNumber { get; set; } = string.Empty;

    public bool CanSelectComplianceScheme { get; set; }


    public List<SubmissionPeriod> SubmissionPeriods { get; set; } = new List<SubmissionPeriod>();
}