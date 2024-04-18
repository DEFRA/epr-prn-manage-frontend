using System.ComponentModel.DataAnnotations;
using PRNPortal.Application.DTOs.ComplianceScheme;

namespace PRNPortal.UI.ViewModels;

public class ReasonForRemovalViewModel
{
    public IReadOnlyCollection<ComplianceSchemeReasonsRemovalDto> ReasonForRemoval { get; set; }

    public string OrganisationName { get; set; }

    [Required(ErrorMessage = "ReasonForRemoval.Error")]
    public string SelectedReasonForRemoval { get; set; }

    public bool IsApprovedUser { get; set; }
}
