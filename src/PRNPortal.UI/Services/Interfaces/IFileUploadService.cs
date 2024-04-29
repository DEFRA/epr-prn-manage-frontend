﻿namespace PRNPortal.UI.Services.Interfaces;

using Application.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding;

public interface IFileUploadService
{
    Task<Guid> ProcessUploadAsync(
        string? contentType,
        Stream fileStream,
        string submissionPeriod,
        ModelStateDictionary modelState,
        Guid? submissionId,
        SubmissionType submissionType,
        SubmissionSubType? submissionSubType = null,
        Guid? registrationSetId = null,
        Guid? complianceSchemeId = null);
}