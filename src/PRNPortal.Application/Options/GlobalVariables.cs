namespace PRNPortal.Application.Options;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class GlobalVariables
{
    public int SchemeYear { get; set; }

    public string BasePath { get; set; }

    public bool UseLocalSession { get; set; }
}
