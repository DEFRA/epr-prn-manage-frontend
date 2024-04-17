using System.Diagnostics.CodeAnalysis;

namespace PRNPortal.Application.DTOs.Submission;

[ExcludeFromCodeCoverage]
public class PomDecision : AbstractDecision
{
    public string Comments { get; set; } = string.Empty;

    public string Decision { get; set; } = string.Empty;

    public bool IsResubmissionRequired { get; set; }
}