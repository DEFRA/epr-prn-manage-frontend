using System.Diagnostics.CodeAnalysis;

namespace PRNPortal.Application.DTOs;

[ExcludeFromCodeCoverage]
public class UserDto
{
    public Guid CustomerId { get; set; }

    public bool PrivacyPolicyAccepted { get; set; }

    public DateTime PrivacyPolicyAcceptedDateTime { get; set; }

    public bool? DeclarationPolicyAccepted { get; set; }

    public DateTime? DeclarationAcceptedDateTime { get; set; }
}