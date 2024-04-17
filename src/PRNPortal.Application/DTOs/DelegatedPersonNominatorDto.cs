using System.Diagnostics.CodeAnalysis;

namespace PRNPortal.Application.DTOs;

[ExcludeFromCodeCoverage]
public class DelegatedPersonNominatorDto
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string OrganisationName { get; set; }
}