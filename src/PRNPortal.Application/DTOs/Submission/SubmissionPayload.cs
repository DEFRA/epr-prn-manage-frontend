using System.Diagnostics.CodeAnalysis;

namespace PRNPortal.Application.DTOs.Submission;

[ExcludeFromCodeCoverage]
public class SubmissionPayload
{
    public Guid FileId { get; set; }

    public string? SubmittedBy { get; set; }
}