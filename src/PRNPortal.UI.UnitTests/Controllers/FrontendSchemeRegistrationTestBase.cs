﻿namespace PRNPortal.UI.UnitTests.Controllers;

using System.Security.Claims;
using Application.DTOs.ComplianceScheme;
using Application.DTOs.Submission;
using Application.DTOs.UserAccount;
using Application.Services.Interfaces;
using EPR.Common.Authorization.Models;
using EPR.Common.Authorization.Sessions;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using Application.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using UI.Controllers;
using UI.Sessions;

using Organisation = Application.DTOs.UserAccount.Organisation;

public abstract class FrontendSchemeRegistrationTestBase
{
    protected const string ModelErrorKey = "Error";
    private const string BackLinkViewDataKey = "BackLinkToDisplay";
    private static readonly Guid _complianceSchemeId = new Guid("00000000-0000-0000-0000-000000000002");
    private static readonly Guid _selectedSchemeId = new Guid("00000000-0000-0000-0000-000000000003");
    private static readonly Guid _complianceSchemeOperatorId = new Guid("00000000-0000-0000-0000-000000000004");
    private readonly Guid _userOid = Guid.NewGuid();
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Mock<HttpContext> _httpContextMock = new();
    private readonly Mock<ClaimsPrincipal> _userMock = new();
    private readonly List<SubmissionPeriod> _submissionPeriods = new()
    {
        new SubmissionPeriod
        {
            DataPeriod = "Data period 1",
            Deadline = DateTime.Today
        },
        new SubmissionPeriod
        {
            DataPeriod = "Data period 2",
            Deadline = DateTime.Today.AddDays(5)
        }
    };

    protected static ProducerComplianceSchemeDto CurrentComplianceScheme => new()
    {
        ComplianceSchemeId = _complianceSchemeId,
        ComplianceSchemeName = "currentTestScheme",
        ComplianceSchemeOperatorName = "testOperator",
        SelectedSchemeId = _selectedSchemeId,
        ComplianceSchemeOperatorId = _complianceSchemeOperatorId,
    };

    protected static ComplianceSchemeDto SelectedComplianceScheme => new()
    {
        Id = _complianceSchemeId,
        Name = "newTestScheme",
    };

    protected static SelectedSchemeDto CommittedSelectedScheme => new()
    {
        Id = _complianceSchemeId,
    };

    protected UserAccountDto UserAccount => new()
    {
        User = new User
        {
            Id = _userOid,
            FirstName = "Joe",
            LastName = "Test",
            Email = "JoeTest@something.com",
            RoleInOrganisation = "Test Role",
            EnrolmentStatus = "Enrolled",
            ServiceRole = "Test service role",
            Service = "Test service",
            Organisations = new List<Organisation>
            {
               new()
               {
                   Id = _organisationId,
                   OrganisationName = "TestCo",
                   OrganisationRole = "Producer",
                   OrganisationType = "test type",
               },
            },
        },
    };

    protected Mock<ISessionManager<FrontendSchemeRegistrationSession>> SessionManagerMock { get; private set; }

    protected FrontendSchemeRegistrationController SystemUnderTest { get; set; }

    protected Mock<INotificationService> NotificationService { get; private set; }

    protected Mock<IComplianceSchemeService> ComplianceSchemeService { get; private set; }

    protected Mock<IUserAccountService> UserAccountService { get; private set; }

    protected Mock<IAuthorizationService> AuthorizationService { get; private set; }

    protected FrontendSchemeRegistrationSession FrontEndSchemeRegistrationSession { get; set; }

    protected static void AssertBackLink(ViewResult viewResult, string expectedBackLink)
    {
        var hasBackLinkKey = viewResult.ViewData.TryGetValue(BackLinkViewDataKey, out var gotBackLinkObject);
        hasBackLinkKey.Should().BeTrue();
        (gotBackLinkObject as string)?.Should().Be(expectedBackLink);
    }

    protected void SetupBase(UserData userData)
    {
        var claims = new List<Claim>();
        if (userData != null)
        {
            claims.Add(new(ClaimTypes.UserData, JsonConvert.SerializeObject(userData)));
        }

        _userMock.Setup(x => x.Claims).Returns(claims);
        _httpContextMock.Setup(x => x.User).Returns(_userMock.Object);

        var tempDataDictionaryMock = new Mock<ITempDataDictionary>();

        SessionManagerMock = new Mock<ISessionManager<FrontendSchemeRegistrationSession>>();
        SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .Returns(Task.FromResult(new FrontendSchemeRegistrationSession()));

        var loggerMock = new Mock<ILogger<FrontendSchemeRegistrationController>>();
        ComplianceSchemeService = new Mock<IComplianceSchemeService>();
        UserAccountService = new Mock<IUserAccountService>();
        AuthorizationService = new Mock<IAuthorizationService>();
        NotificationService = new Mock<INotificationService>();

        SystemUnderTest = new FrontendSchemeRegistrationController(
            SessionManagerMock.Object,
            loggerMock.Object,
            ComplianceSchemeService.Object,
            AuthorizationService.Object,
            NotificationService.Object,
            Options.Create(new GlobalVariables { SubmissionPeriods = _submissionPeriods }));
        SystemUnderTest.ControllerContext.HttpContext = _httpContextMock.Object;
        SystemUnderTest.TempData = tempDataDictionaryMock.Object;
    }
}
