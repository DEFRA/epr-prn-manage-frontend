﻿namespace PRNPortal.UI.Controllers;

using Application.Constants;
using Application.DTOs.Submission;
using Application.Services.Interfaces;
using EPR.Common.Authorization.Constants;
using EPR.Common.Authorization.Sessions;
using Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Sessions;
using UI.Attributes.ActionFilters;
using ViewModels;

[Authorize(Policy = PolicyConstants.EprFileUploadPolicy)]
[Route(PagePaths.ReviewOrganisationData)]
public class ReviewCompanyDetailsController : Controller
{
    private readonly ISubmissionService _submissionService;
    private readonly IUserAccountService _accountService;
    private readonly ISessionManager<FrontendSchemeRegistrationSession> _sessionManager;

    public ReviewCompanyDetailsController(
        ISubmissionService submissionService,
        IUserAccountService accountService,
        ISessionManager<FrontendSchemeRegistrationSession> sessionManager)
    {
        _submissionService = submissionService;
        _accountService = accountService;
        _sessionManager = sessionManager;
    }

    [HttpGet]
    [SubmissionIdActionFilter("/error")]
    public async Task<IActionResult> Get()
    {
        var submissionId = Guid.Parse(Request.Query["submissionId"]);
        var userData = User.GetUserData();

        var submission = await _submissionService.GetSubmissionAsync<RegistrationSubmission>(submissionId);

        if (submission is null)
        {
            return RedirectToAction("Get", "FileUploadCompanyDetailsSubLanding");
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        return View(
            "ReviewCompanyDetails",
            new ReviewCompanyDetailsViewModel
            {
                SubmissionId = submissionId,
                OrganisationDetailsFileName = submission.LastUploadedValidFiles.CompanyDetailsFileName,
                OrganisationDetailsUploadedBy = await GetUsersName(submission.LastUploadedValidFiles.CompanyDetailsUploadedBy),
                OrganisationDetailsFileUploadDate =
                    submission.LastUploadedValidFiles.CompanyDetailsUploadDatetime.ToReadableDate(),
                OrganisationDetailsFileId = submission.LastUploadedValidFiles.CompanyDetailsFileId.ToString(),
                BrandFileName = submission.LastUploadedValidFiles?.BrandsFileName,
                BrandUploadedBy = submission.LastUploadedValidFiles?.BrandsUploadedBy != null
                    ? await GetUsersName(submission.LastUploadedValidFiles.BrandsUploadedBy.Value)
                    : string.Empty,
                BrandFileUploadDate = submission.LastUploadedValidFiles?.BrandsUploadDatetime?.ToReadableDate(),
                PartnerFileName = submission.LastUploadedValidFiles?.PartnershipsFileName,
                PartnerUploadedBy = submission.LastUploadedValidFiles?.PartnershipsUploadedBy != null
                    ? await GetUsersName(submission.LastUploadedValidFiles.PartnershipsUploadedBy.Value)
                    : string.Empty,
                PartnerFileUploadDate = submission.LastUploadedValidFiles?.PartnershipsUploadDatetime?.ToReadableDate(),
                RegistrationSubmissionDeadline = session.RegistrationSession.SubmissionDeadline.ToReadableLongMonthDeadlineDate(),
                BrandsRequired = submission.LastUploadedValidFiles is not null && !submission.LastUploadedValidFiles.BrandsFileName.IsNullOrEmpty(),
                PartnersRequired = submission.LastUploadedValidFiles is not null && !submission.LastUploadedValidFiles.PartnershipsFileName.IsNullOrEmpty(),
                OrganisationRole = userData.Organisations.FirstOrDefault()!.OrganisationRole,
                IsApprovedUser = userData.CanSubmit(),
                HasPreviousSubmission = submission.LastSubmittedFiles is not null,
                HasPreviousBrandsSubmission = submission.LastSubmittedFiles is not null && !submission.LastSubmittedFiles.BrandsFileName.IsNullOrEmpty(),
                HasPreviousPartnersSubmission = submission.LastSubmittedFiles is not null && !submission.LastSubmittedFiles.PartnersFileName.IsNullOrEmpty(),
                SubmittedCompanyDetailsFileName = submission.LastSubmittedFiles?.CompanyDetailsFileName,
                SubmittedCompanyDetailsDateTime = submission.LastSubmittedFiles?.SubmittedDateTime?.ToReadableDate(),
                SubmittedBrandsFileName = submission.LastSubmittedFiles?.BrandsFileName,
                SubmittedBrandsDateTime = submission.LastSubmittedFiles?.SubmittedDateTime?.ToReadableDate(),
                SubmittedPartnersFileName = submission.LastSubmittedFiles?.PartnersFileName,
                SubmittedPartnersDateTime = submission.LastSubmittedFiles?.SubmittedDateTime?.ToReadableDate(),
                SubmittedDateTime = submission.LastSubmittedFiles?.SubmittedDateTime?.ToReadableDate(),
                SubmittedBy = submission.LastSubmittedFiles?.SubmittedBy != null
                    ? await GetUsersName(submission.LastSubmittedFiles.SubmittedBy.Value)
                    : string.Empty,
                SubmissionStatus = submission.GetSubmissionStatus()
            });

        return RedirectToAction("HandleThrownExceptions", "Error");
    }

    [HttpPost]
    public async Task<IActionResult> Post(ReviewCompanyDetailsViewModel model)
    {
        var submissionId = Guid.Parse(Request.Query["submissionId"]);
        var userData = User.GetUserData();

        if (!userData.CanSubmit())
        {
            var routeValues = new RouteValueDictionary { { "submissionId", submissionId.ToString() } };
            return RedirectToAction("Get", routeValues);
        }

        if (!ModelState.IsValid)
        {
            return View("ReviewCompanyDetails", model);
        }

        if (model.SubmitOrganisationDetailsResponse.HasValue && !model.SubmitOrganisationDetailsResponse.Value)
        {
            return RedirectToAction("Get", "FileUploadCompanyDetailsSubLanding");
        }

        if (!model.IsApprovedUser)
        {
            return new UnauthorizedResult();
        }

        try
        {
            if (model.IsComplianceScheme)
            {
                await _submissionService.SubmitAsync(model.SubmissionId, new Guid(model.OrganisationDetailsFileId));
                return RedirectToAction("Get", "CompanyDetailsConfirmation", new
                {
                    model.SubmissionId
                });
            }
            else
            {
                return RedirectToAction("Get", "DeclarationWithFullName", new
                {
                    model.SubmissionId
                });
            }
        }
        catch (Exception)
        {
            return RedirectToAction("HandleThrownSubmissionException", "Error");
        }
    }

    private async Task<string> GetUsersName(Guid userId)
    {
        var person = await _accountService.GetPersonByUserId(userId);
        return $"{person.FirstName} {person.LastName}";
    }
}