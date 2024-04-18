using System.Diagnostics.CodeAnalysis;

namespace PRNPortal.Application.DTOs.ComplianceScheme;

[ExcludeFromCodeCoverage]
public class ComplianceSchemeDto
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    public int NationId { get; set; }
}