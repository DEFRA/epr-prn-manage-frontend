using System.Diagnostics.CodeAnalysis;

namespace PRNPortal.Application.DTOs.UserAccount;

[ExcludeFromCodeCoverage]
public class PersonDto
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string ContactEmail { get; set; }
}