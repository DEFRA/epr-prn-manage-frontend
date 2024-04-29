﻿using System.Diagnostics.CodeAnalysis;
using PRNPortal.Application.Enums;

namespace PRNPortal.Application.DTOs.Submission;

[ExcludeFromCodeCoverage]
public class SubmissionPeriodDetail
{
    public string DataPeriod { get; set; }

    public DateTime Deadline { get; set; }

    public SubmissionPeriodStatus Status { get; set; }

    public bool IsResubmissionRequired { get; set; }

    public string? Decision { get; set; }

    public string? Comments { get; set; } = string.Empty;
}