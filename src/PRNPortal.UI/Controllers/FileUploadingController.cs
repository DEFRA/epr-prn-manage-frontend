﻿using EPR.Common.Authorization.Constants;
using EPR.Common.Authorization.Sessions;
using PRNPortal.Application.Constants;
using PRNPortal.Application.DTOs.Submission;
using PRNPortal.Application.Services.Interfaces;
using PRNPortal.UI.Attributes.ActionFilters;
using PRNPortal.UI.Extensions;
using PRNPortal.UI.Sessions;
using PRNPortal.UI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PRNPortal.UI.Controllers;

[Authorize(Policy = PolicyConstants.EprFileUploadPolicy)]
[Route(PagePaths.FileUploading)]
public class FileUploadingController : Controller
{
    private readonly ISubmissionService _submissionService;
    private readonly ISessionManager<FrontendSchemeRegistrationSession> _sessionManager;

    public FileUploadingController(
        ISubmissionService submissionService,
        ISessionManager<FrontendSchemeRegistrationSession> sessionManager)
    {
        _submissionService = submissionService;
        _sessionManager = sessionManager;
    }

    [HttpGet]
    [SubmissionIdActionFilter(PagePaths.FileUpload)]
    public async Task<IActionResult> Get()
    {
        var submissionId = Guid.Parse(Request.Query["submissionId"]);
        var submission = await _submissionService.GetSubmissionAsync<PomSubmission>(submissionId);
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (submission is null)
        {
            return RedirectToAction("Get", "FileUpload");
        }

        if (!session.RegistrationSession.Journey.Contains<string>(PagePaths.FileUpload))
        {
            return RedirectToAction("Get", "FileUploadSubLanding");
        }

        return submission.PomDataComplete || submission.Errors.Any()
            ? GetNextPageAsync(submission.Id, submission.ValidationPass, submission.HasWarnings, submission.Errors.Any()).Result
            : View("FileUploading", new FileUploadingViewModel { SubmissionId = submissionId.ToString() });
    }

    private async Task<RedirectToActionResult> GetNextPageAsync(Guid submissionId, bool validationPass, bool hasWarnings, bool exceptionErrorOccurred)
    {
        var routeValues = new RouteValueDictionary { { "submissionId", submissionId.ToString() } };

        if (exceptionErrorOccurred)
        {
            routeValues.Add("showErrors", true);
            return RedirectToAction("Get", "FileUpload", routeValues);
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (session is not null)
        {
            session.RegistrationSession.Journey.AddIfNotExists(PagePaths.FileUploading);
            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);
        }

        if (!validationPass)
        {
            return RedirectToAction("Get", "FileUploadFailure", routeValues);
        }

        return RedirectToAction("Get", hasWarnings ? "FileUploadWarning" : "FileUploadCheckFileAndSubmit", routeValues);
    }
}