using System.Diagnostics.CodeAnalysis;

namespace PRNPortal.Application.RequestModels;

[ExcludeFromCodeCoverage]
public class ComplianceSchemeServiceAddRequestModel
{
    public Guid OrganisationId { get; set; }

    public Guid ComplianceSchemeId { get; set; }
}