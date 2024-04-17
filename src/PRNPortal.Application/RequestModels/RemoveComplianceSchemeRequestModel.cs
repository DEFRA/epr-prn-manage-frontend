using System.Diagnostics.CodeAnalysis;

namespace PRNPortal.Application.RequestModels;

[ExcludeFromCodeCoverage]
public class RemoveComplianceSchemeRequestModel
{
    public Guid SelectedSchemeId { get; set; }

    public Guid OrganisationId { get; set; }
}