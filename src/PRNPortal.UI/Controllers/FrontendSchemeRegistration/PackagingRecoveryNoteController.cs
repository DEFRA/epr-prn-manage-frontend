using EPR.Common.Authorization.Constants;
using EPR.Common.Authorization.Sessions;
using PRNPortal.Application.Constants;
using PRNPortal.Application.DTOs.ComplianceScheme;
using PRNPortal.Application.DTOs.Submission;
using PRNPortal.Application.Options;
using PRNPortal.Application.Services.Interfaces;
using PRNPortal.UI.Constants;
using PRNPortal.UI.Controllers.Attributes;
using PRNPortal.UI.Controllers.ControllerExtensions;
using PRNPortal.UI.Extensions;
using PRNPortal.UI.Resources.Views;
using PRNPortal.UI.Sessions;
using PRNPortal.UI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using PRNPortal.UI.Resources.Views.Compliance;

namespace PRNPortal.UI.Controllers;

public class PackagingRecoveryNoteController : Controller
{
    private readonly ISessionManager<FrontendSchemeRegistrationSession> _sessionManager;
    private readonly IComplianceSchemeService _complianceSchemeService;
    private readonly IAuthorizationService _authorizationService;
    private readonly INotificationService _notificationService;
    private readonly List<SubmissionPeriod> _submissionPeriods;
    private readonly ILogger<PackagingRecoveryNoteController> _logger;

    public PackagingRecoveryNoteController(
        ISessionManager<FrontendSchemeRegistrationSession> sessionManager,
        ILogger<PackagingRecoveryNoteController> logger,
        IComplianceSchemeService complianceSchemeService,
        IAuthorizationService authorizationService,
        INotificationService notificationService,
        IOptions<GlobalVariables> globalVariables)
    {
        _sessionManager = sessionManager;
        _complianceSchemeService = complianceSchemeService;
        _authorizationService = authorizationService;
        _notificationService = notificationService;
        _submissionPeriods = globalVariables.Value.SubmissionPeriods;
        _logger = logger;
    }

    [HttpGet]
    [Route(PagePaths.LandingPage)]
    [AuthorizeForScopes(ScopeKeySection = "FacadeAPI:DownstreamScope")]
    public async Task<IActionResult> LandingPage()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new FrontendSchemeRegistrationSession();
        var userData = User.GetUserData();

        var organisation = userData.Organisations.First(x => x.OrganisationRole == OrganisationRoles.Producer);
        var producerComplianceScheme = await _complianceSchemeService.GetProducerComplianceScheme(organisation.Id.Value);

        if (producerComplianceScheme is not null && _authorizationService.AuthorizeAsync(User, HttpContext, PolicyConstants.EprSelectSchemePolicy).Result.Succeeded)
        {
            session.RegistrationSession.CurrentComplianceScheme = producerComplianceScheme;
            return await SaveSessionAndRedirect(session, nameof(ComplianceSchemeMemberLandingController.Get),
                nameof(ComplianceSchemeMemberLandingController).RemoveControllerFromName(), PagePaths.LandingPage,
                PagePaths.ComplianceSchemeMemberLanding);
        }

        var viewModel = new LandingPageViewModel
        {
            OrganisationName = organisation.Name,
            OrganisationId = organisation.Id.Value,
            OrganisationNumber = organisation.OrganisationNumber.ToReferenceNumberFormat()
        };
        var notificationsList = await _notificationService.GetCurrentUserNotifications(organisation.Id.Value, userData.Id.Value);
        if (notificationsList != null)
        {
            try
            {
                viewModel.Notification.BuildFromNotificationList(notificationsList);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("{message} for user '{userID}' in organisation '{organisationId}'", ex.Message, userData.Id.Value, organisation.Id.Value);
            }
        }

        return View(nameof(LandingPage), viewModel);
    }

    [HttpPost]
    [Authorize(Policy = PolicyConstants.EprSelectSchemePolicy)]
    [Route(PagePaths.LandingPage)]
    public async Task<IActionResult> LandingPage(LandingPageViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var userData = User.GetUserData();
            var organisation = userData.Organisations.First(x => x.OrganisationRole == OrganisationRoles.Producer);

            var notificationsList = await _notificationService.GetCurrentUserNotifications(organisation.Id.Value, userData.Id.Value);
            if (notificationsList != null)
            {
                try
                {
                    model.Notification.BuildFromNotificationList(notificationsList);
                }
                catch (ArgumentException ex)
                {
                    _logger.LogError("{message} for user '{userID}' in organisation '{organisationId}'", ex.Message, userData.Id.Value, organisation.Id.Value);
                }
            }

            return View(model);
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        session.RegistrationSession.Journey = new List<string> { PagePaths.LandingPage };
        session.RegistrationSession.IsUpdateJourney = false;
        return await SaveSessionAndRedirect(session, nameof(PrnHomeViewModel), PagePaths.LandingPage, PagePaths.UsingAComplianceScheme);
    }

    [HttpGet]
    [Authorize(Policy = PolicyConstants.EprFileUploadPolicy)]
    [Route(PagePaths.PrnHome)]
    public async Task<IActionResult> VisitHomePageSelfManaged()
    {
        var userData = User.GetUserData();
        var organisation = userData.Organisations.First(x => x.OrganisationRole == OrganisationRoles.Producer);
        var producerComplianceScheme = await _complianceSchemeService.GetProducerComplianceScheme(organisation.Id.Value);

        if (producerComplianceScheme is not null && _authorizationService.AuthorizeAsync(User, HttpContext, PolicyConstants.EprSelectSchemePolicy).Result.Succeeded)
        {
            return RedirectToAction(nameof(ComplianceSchemeMemberLandingController.Get), nameof(ComplianceSchemeMemberLandingController).RemoveControllerFromName());
        }

        var viewModel = new PrnHomeViewModel
        {
            OrganisationName = organisation.Name,
            OrganisationNumber = organisation.OrganisationNumber.ToReferenceNumberFormat(),


        };



        return View(nameof(PrnHome), viewModel);
    }

    [HttpPost]
    [Authorize(Policy = PolicyConstants.EprSelectSchemePolicy)]
    [Route(PagePaths.PrnHome)]
    public async Task<IActionResult> PrnHome()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new FrontendSchemeRegistrationSession();
        session.RegistrationSession.Journey = new List<string> { PagePaths.PrnHome };
        return await SaveSessionAndRedirect(session, nameof(PrnHomeViewModel), PagePaths.PrnHome, PagePaths.PrnCreate);
    }

    private static void ClearRestOfJourney(FrontendSchemeRegistrationSession session, string currentPagePath)
    {
        var index = session.RegistrationSession.Journey.IndexOf(currentPagePath);

        // this also cover if current page not found (index = -1) then it clears all pages
        session.RegistrationSession.Journey = session.RegistrationSession.Journey.Take(index + 1).ToList();
    }

    private async Task<RedirectToActionResult> SaveSessionAndRedirect(
        FrontendSchemeRegistrationSession session,
        string actionName,
        string currentPagePath,
        string? nextPagePath)
    {
        await SaveSession(session, currentPagePath, nextPagePath);

        return RedirectToAction(actionName);
    }

    private async Task<RedirectToActionResult> SaveSessionAndRedirect(
        FrontendSchemeRegistrationSession session,
        string actionName,
        string controllerName,
        string currentPagePath,
        string? nextPagePath)
    {
        await SaveSession(session, currentPagePath, nextPagePath);

        return RedirectToAction(actionName, controllerName);
    }

    private async Task SaveSession(FrontendSchemeRegistrationSession session, string currentPagePath, string? nextPagePath)
    {
        ClearRestOfJourney(session, currentPagePath);

        session.RegistrationSession.Journey.AddIfNotExists(nextPagePath);

        await _sessionManager.SaveSessionAsync(HttpContext.Session, session);
    }

    private void SetBackLink(FrontendSchemeRegistrationSession session, string currentPagePath)
    {
        ViewBag.BackLinkToDisplay = session.RegistrationSession.Journey.PreviousOrDefault(currentPagePath) ?? string.Empty;
    }

}