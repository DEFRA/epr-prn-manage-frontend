﻿namespace PRNPortal.UI.Controllers;

using Application.Constants;
using Application.DTOs.Submission;
using Application.Enums;
using Application.Services.Interfaces;
using Constants;
using EPR.Common.Authorization.Constants;
using EPR.Common.Authorization.Sessions;
using Extensions;
using Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Sessions;
using UI.Attributes.ActionFilters;
using ViewModels;

[Authorize(Policy = PolicyConstants.EprFileUploadPolicy)]
[Route(PagePaths.FileUploadCompanyDetails)]
[SubmissionPeriodActionFilter(PagePaths.FileUploadCompanyDetailsSubLanding)]
[ComplianceSchemeIdActionFilter]
[IgnoreAntiforgeryToken]
public class FileUploadCompanyDetailsController : Controller
{
    private readonly IFileUploadService _fileUploadService;
    private readonly ISessionManager<FrontendSchemeRegistrationSession> _sessionManager;
    private readonly ISubmissionService _submissionService;

    public FileUploadCompanyDetailsController(
        ISubmissionService submissionService,
        IFileUploadService fileUploadService,
        ISessionManager<FrontendSchemeRegistrationSession> sessionManager)
    {
        _submissionService = submissionService;
        _fileUploadService = fileUploadService;
        _sessionManager = sessionManager;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        if (session is not null)
        {
            if (session.RegistrationSession.Journey.Any() && !session.RegistrationSession.Journey.Contains<string>(PagePaths.FileUploadCompanyDetailsSubLanding))
            {
                return RedirectToAction("Get", "FileUploadCompanyDetailsSubLanding");
            }

            var organisationRole = session.UserData.Organisations.FirstOrDefault()?.OrganisationRole;

            if (organisationRole is not null)
            {
                if (Guid.TryParse(Request.Query["SubmissionId"], out var submissionId))
                {
                    var submission = await _submissionService.GetSubmissionAsync<RegistrationSubmission>(submissionId);
                    if (submission != null && submission.Errors.Any())
                    {
                        ModelStateHelpers.AddFileUploadExceptionsToModelState(submission.Errors.Distinct().ToList(), ModelState);
                    }
                }

                ViewBag.BackLinkToDisplay = Url.Content($"~{PagePaths.FileUploadCompanyDetailsSubLanding}");
                return View(
                    "FileUploadCompanyDetails",
                    new FileUploadCompanyDetailsViewModel
                    {
                        SubmissionDeadline = session.RegistrationSession.SubmissionDeadline,
                        OrganisationRole = organisationRole
                    });
            }
        }

        return RedirectToAction("Get", "FileUploadCompanyDetailsSubLanding");
    }

    [HttpPost]
    [RequestSizeLimit(FileSizeLimit.FileSizeLimitInBytes)]
    public async Task<IActionResult> Post()
    {
        Guid? submissionId = Guid.TryParse(Request.Query["submissionId"], out var value) ? value : null;
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        session.RegistrationSession.LatestRegistrationSet ??= new Dictionary<string, Guid>();

        session.RegistrationSession.LatestRegistrationSet[session.RegistrationSession.SubmissionPeriod] =
            Guid.NewGuid();

        submissionId = await _fileUploadService.ProcessUploadAsync(
            Request.ContentType,
            Request.Body,
            session.RegistrationSession.SubmissionPeriod,
            ModelState,
            submissionId,
            SubmissionType.Registration,
            SubmissionSubType.CompanyDetails,
            session.RegistrationSession.LatestRegistrationSet[session.RegistrationSession.SubmissionPeriod],
            session.RegistrationSession.SelectedComplianceScheme?.Id);

        session.RegistrationSession.Journey.AddIfNotExists(PagePaths.FileUploadCompanyDetails);
        var organisationRole = session.UserData.Organisations.FirstOrDefault()?.OrganisationRole;

        await _sessionManager.SaveSessionAsync(HttpContext.Session, session);
        ViewBag.BackLinkToDisplay = Url.Content($"~{PagePaths.FileUploadCompanyDetailsSubLanding}");

        return !ModelState.IsValid
            ? View(
                "FileUploadCompanyDetails",
                new FileUploadCompanyDetailsViewModel
                {
                    SubmissionDeadline = session.RegistrationSession.SubmissionDeadline,
                    OrganisationRole = organisationRole
                })
            : RedirectToAction(
                "Get",
                "FileUploadingCompanyDetails",
                new RouteValueDictionary
                {
                    {
                        "submissionId", submissionId
                    }
                });
    }
}