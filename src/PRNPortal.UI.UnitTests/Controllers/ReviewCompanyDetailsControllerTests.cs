﻿namespace PRNPortal.UI.UnitTests.Controllers;

using System.Security.Claims;
using System.Text.Json;
using Application.DTOs.Submission;
using Application.DTOs.UserAccount;
using Application.Services.Interfaces;
using Constants;
using EPR.Common.Authorization.Constants;
using EPR.Common.Authorization.Models;
using EPR.Common.Authorization.Sessions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Moq;
using UI.Controllers;
using UI.Extensions;
using UI.Sessions;
using UI.ViewModels;
using Organisation = EPR.Common.Authorization.Models.Organisation;

public class ReviewCompanyDetailsControllerTests
{
    private static readonly string SubmissionId = Guid.NewGuid().ToString();
    private static readonly DateTime SubmissionDeadline = DateTime.UtcNow.Date;
    private ReviewCompanyDetailsController _systemUnderTest;
    private Mock<ISubmissionService> _submissionService;
    private Mock<ISessionManager<FrontendSchemeRegistrationSession>> _sessionManagerMock;
    private Mock<IUserAccountService> _userAccountServiceMock;
    private Mock<ClaimsPrincipal> _claimsPrincipalMock;

    [SetUp]
    public void SetUp()
    {
        _submissionService = new Mock<ISubmissionService>();
        _userAccountServiceMock = new Mock<IUserAccountService>();
        _claimsPrincipalMock = new Mock<ClaimsPrincipal>();
        _sessionManagerMock = new();
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(new FrontendSchemeRegistrationSession
            {
                RegistrationSession = new RegistrationSession
                {
                    SubmissionDeadline = SubmissionDeadline,
                },
                UserData = new UserData { Organisations = { new() { OrganisationRole = OrganisationRoles.ComplianceScheme } }, ServiceRole = ServiceRoles.ApprovedPerson }
            });
        _systemUnderTest = new ReviewCompanyDetailsController(
            _submissionService.Object,
            _userAccountServiceMock.Object,
            _sessionManagerMock.Object);
        _systemUnderTest.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                Request =
                {
                    Query = new QueryCollection(new Dictionary<string, StringValues>
                    {
                        { "submissionId", SubmissionId }
                    })
                },
                Session = new Mock<ISession>().Object,
                User = _claimsPrincipalMock.Object
            }
        };
    }

    [TestCase(ServiceRoles.ApprovedPerson, EnrolmentStatuses.Approved, true)]
    [TestCase(ServiceRoles.ApprovedPerson, EnrolmentStatuses.Invited, false)]
    [TestCase(ServiceRoles.ApprovedPerson, EnrolmentStatuses.Pending, false)]
    [TestCase(ServiceRoles.ApprovedPerson, EnrolmentStatuses.NotSet, false)]
    [TestCase(ServiceRoles.ApprovedPerson, EnrolmentStatuses.Rejected, false)]
    [TestCase(ServiceRoles.DelegatedPerson, EnrolmentStatuses.Approved, true)]
    [TestCase(ServiceRoles.DelegatedPerson, EnrolmentStatuses.Invited, false)]
    [TestCase(ServiceRoles.DelegatedPerson, EnrolmentStatuses.Pending, false)]
    [TestCase(ServiceRoles.DelegatedPerson, EnrolmentStatuses.NotSet, false)]
    [TestCase(ServiceRoles.DelegatedPerson, EnrolmentStatuses.Rejected, false)]
    [TestCase(ServiceRoles.BasicUser, EnrolmentStatuses.Approved, false)]
    [TestCase(ServiceRoles.BasicUser, EnrolmentStatuses.Invited, false)]
    [TestCase(ServiceRoles.BasicUser, EnrolmentStatuses.Pending, false)]
    [TestCase(ServiceRoles.BasicUser, EnrolmentStatuses.NotSet, false)]
    [TestCase(ServiceRoles.BasicUser, EnrolmentStatuses.Rejected, false)]
    public async Task Get_ReturnsCorrectView(string serviceRole, string enrolmentStatus, bool expectedIsApprovedUser)
    {
        // Arrange
        const string firstName = "First Name";
        const string lastName = "Last Name";
        _submissionService
            .Setup(x => x.GetSubmissionAsync<RegistrationSubmission>(It.IsAny<Guid>()))
            .ReturnsAsync(GenerateRegistrationSubmission());
        _userAccountServiceMock.Setup(x => x.GetPersonByUserId(It.IsAny<Guid>()))
            .ReturnsAsync(new PersonDto
            {
                FirstName = firstName,
                LastName = lastName,
                ContactEmail = "email@email.com"
            });
        var claims = CreateUserDataClaim(serviceRole, enrolmentStatus, OrganisationRoles.Producer);
        _claimsPrincipalMock.Setup(x => x.Claims).Returns(claims);

        // Act
        var result = await _systemUnderTest.Get() as ViewResult;

        // Assert
        result.ViewName.Should().Be("ReviewCompanyDetails");
        var model = result.Model.As<ReviewCompanyDetailsViewModel>();
        model.OrganisationDetailsUploadedBy.Should().BeEquivalentTo($"{firstName} {lastName}");
        model.RegistrationSubmissionDeadline.Should().BeEquivalentTo(SubmissionDeadline.ToReadableDate());
        model.IsApprovedUser.Should().Be(expectedIsApprovedUser);
    }

    [Test]
    public async Task Get_RedirectsToFileUploadCompanyDetailsSubLandingGet_WhenSubmissionIsNull()
    {
        // Arrange
        _submissionService
            .Setup(x => x.GetSubmissionAsync<RegistrationSubmission>(It.IsAny<Guid>()))
            .ReturnsAsync((RegistrationSubmission)null);

        var claims = CreateUserDataClaim(ServiceRoles.ApprovedPerson, EnrolmentStatuses.Approved, OrganisationRoles.Producer);
        _claimsPrincipalMock.Setup(x => x.Claims).Returns(claims);

        // Act
        var result = await _systemUnderTest.Get() as RedirectToActionResult;

        // Assert
        result.ActionName.Should().Be("Get");
        result.ControllerName.Should().Be("FileUploadCompanyDetailsSubLanding");
    }

    [Test]
    public async Task Post_RedirectsToDeclarationWithFullName_WhenDirectProducerSubmits()
    {
        // Arrange
        var model = GenerateReviewCompanyDetailsModel(OrganisationRoles.Producer, true, true);

        var claims = CreateUserDataClaim(ServiceRoles.ApprovedPerson, EnrolmentStatuses.Approved, OrganisationRoles.Producer);
        _claimsPrincipalMock.Setup(x => x.Claims).Returns(claims);

        // Act
        var result = await _systemUnderTest.Post(model) as RedirectToActionResult;

        // Assert
        result?.ActionName.Should().Be("Get");
        result?.ControllerName.Should().Be("DeclarationWithFullName");
    }

    [Test]
    public async Task Post_RedirectsToCompanyDetailsConfirmation_WhenComplianceSchemeSubmits()
    {
        // Arrange
        _submissionService.Setup(x => x.SubmitAsync(It.IsAny<Guid>(), It.IsAny<Guid>()));
        var model = GenerateReviewCompanyDetailsModel(OrganisationRoles.ComplianceScheme, true, true);

        var claims = CreateUserDataClaim(ServiceRoles.ApprovedPerson, EnrolmentStatuses.Approved, OrganisationRoles.Producer);
        _claimsPrincipalMock.Setup(x => x.Claims).Returns(claims);

        // Act
        var result = await _systemUnderTest.Post(model) as RedirectToActionResult;

        // Assert
        result?.ActionName.Should().Be("Get");
        result?.ControllerName.Should().Be("CompanyDetailsConfirmation");
    }

    [TestCase(ServiceRoles.ApprovedPerson, EnrolmentStatuses.Invited)]
    [TestCase(ServiceRoles.ApprovedPerson, EnrolmentStatuses.Pending)]
    [TestCase(ServiceRoles.ApprovedPerson, EnrolmentStatuses.NotSet)]
    [TestCase(ServiceRoles.ApprovedPerson, EnrolmentStatuses.Rejected)]
    [TestCase(ServiceRoles.DelegatedPerson, EnrolmentStatuses.Invited)]
    [TestCase(ServiceRoles.DelegatedPerson, EnrolmentStatuses.Pending)]
    [TestCase(ServiceRoles.DelegatedPerson, EnrolmentStatuses.NotSet)]
    [TestCase(ServiceRoles.DelegatedPerson, EnrolmentStatuses.Rejected)]
    [TestCase(ServiceRoles.BasicUser, EnrolmentStatuses.Approved)]
    [TestCase(ServiceRoles.BasicUser, EnrolmentStatuses.Invited)]
    [TestCase(ServiceRoles.BasicUser, EnrolmentStatuses.Pending)]
    [TestCase(ServiceRoles.BasicUser, EnrolmentStatuses.NotSet)]
    [TestCase(ServiceRoles.BasicUser, EnrolmentStatuses.Rejected)]
    public async Task Post_RedirectsToGet_WhenUserDoesNotHavePermissionToSubmit(string serviceRole, string enrolmentStatus)
    {
        // Arrange
        var model = new ReviewCompanyDetailsViewModel();
        var claims = CreateUserDataClaim(serviceRole, enrolmentStatus, OrganisationRoles.Producer);
        _claimsPrincipalMock.Setup(x => x.Claims).Returns(claims);

        // Act
        var result = await _systemUnderTest.Post(model) as RedirectToActionResult;

        // Assert
        result!.ActionName.Should().Be("Get");
    }

    [Test]
    public async Task Post_RedirectsToFileUploadCompanyDetailsSubLanding_WhenUserDoesNotSubmit()
    {
        // Arrange
        var model = GenerateReviewCompanyDetailsModel(OrganisationRoles.ComplianceScheme, true, false);

        var claims = CreateUserDataClaim(ServiceRoles.ApprovedPerson, EnrolmentStatuses.Approved, OrganisationRoles.Producer);
        _claimsPrincipalMock.Setup(x => x.Claims).Returns(claims);

        // Act
        var result = await _systemUnderTest.Post(model) as RedirectToActionResult;

        // Assert
        result?.ActionName.Should().Be("Get");
        result?.ControllerName.Should().Be("FileUploadCompanyDetailsSubLanding");
    }

    private static ReviewCompanyDetailsViewModel GenerateReviewCompanyDetailsModel(string role, bool isApprovedUser, bool? submitOrgDetails)
    {
        return new ReviewCompanyDetailsViewModel
        {
            OrganisationRole = role,
            SubmissionId = Guid.NewGuid(),
            OrganisationDetailsFileName = "Organisation Details File Name.csv",
            OrganisationDetailsUploadedBy = "User",
            OrganisationDetailsFileUploadDate = DateTime.UtcNow.ToReadableDate(),
            OrganisationDetailsFileId = Guid.NewGuid().ToString(),
            BrandFileName = "Brand File Name.csv",
            BrandUploadedBy = "null",
            BrandFileUploadDate = DateTime.UtcNow.ToReadableDate(),
            PartnerFileName = "Partner File Name.csv",
            PartnerUploadedBy = "null",
            PartnerFileUploadDate = DateTime.UtcNow.ToReadableDate(),
            RegistrationSubmissionDeadline = DateTime.UtcNow.AddDays(1).ToReadableDate(),
            BrandsRequired = true,
            PartnersRequired = true,
            IsApprovedUser = isApprovedUser,
            HasPreviousSubmission = false,
            SubmittedCompanyDetailsFileName = "SubmittedCompanyDetailsFileName",
            SubmittedCompanyDetailsDateTime = "SubmittedCompanyDetailsDateTime",
            SubmittedBrandsFileName = "SubmittedBrandsFileName",
            SubmittedBrandsDateTime = "SubmittedBrandsDateTime",
            SubmittedPartnersFileName = "SubmittedPartnersFileName",
            SubmittedPartnersDateTime = "SubmittedPartnersDateTime",
            SubmittedDateTime = "SubmittedDateTime",
            SubmittedBy = "SubmittedBy",
            SubmitOrganisationDetailsResponse = submitOrgDetails
        };
    }

    private static RegistrationSubmission GenerateRegistrationSubmission()
    {
        return new RegistrationSubmission
        {
            Id = Guid.NewGuid(),
            SubmissionPeriod = "SubmissionPeriod",
            ValidationPass = true,
            HasValidFile = true,
            Errors = new List<string>(),
            IsSubmitted = false,
            BrandsDataComplete = true,
            BrandsFileName = "BrandsFileName",
            BrandsUploadedBy = Guid.NewGuid(),
            BrandsUploadedDate = DateTime.UtcNow,
            CompanyDetailsFileName = "CompanyDetailsFileName",
            CompanyDetailsUploadedBy = Guid.NewGuid(),
            CompanyDetailsUploadedDate = DateTime.UtcNow,
            CompanyDetailsDataComplete = false,
            PartnershipsDataComplete = true,
            PartnershipsFileName = "PartnershipsFileName",
            PartnershipsUploadedBy = Guid.NewGuid(),
            PartnershipsUploadedDate = DateTime.UtcNow,
            RequiresBrandsFile = true,
            RequiresPartnershipsFile = true,
            LastSubmittedFiles = new SubmittedRegistrationFilesInformation
            {
                CompanyDetailsFileName = "CompanyDetailsFileName",
                BrandsFileName = "BrandsFileName",
                PartnersFileName = "PartnersFileName",
                SubmittedDateTime = DateTime.UtcNow,
                SubmittedBy = Guid.NewGuid()
            },
            LastUploadedValidFiles = new UploadedRegistrationFilesInformation
            {
                CompanyDetailsFileName = "CompanyDetailsFileName",
                BrandsFileName = "BrandsFileName",
                PartnershipsFileName = "PartnersFileName",
                CompanyDetailsFileId = Guid.NewGuid(),
                CompanyDetailsUploadedBy = Guid.NewGuid(),
                CompanyDetailsUploadDatetime = DateTime.UtcNow,
                BrandsUploadedBy = Guid.NewGuid(),
                BrandsUploadDatetime = DateTime.UtcNow,
                PartnershipsUploadedBy = Guid.NewGuid(),
                PartnershipsUploadDatetime = null
            }
        };
    }

    private static List<Claim> CreateUserDataClaim(string serviceRole, string enrolmentStatus, string organisationRole)
    {
        var userData = new UserData
        {
            ServiceRole = serviceRole,
            EnrolmentStatus = enrolmentStatus,
            Organisations = new List<Organisation>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    OrganisationRole = organisationRole,
                    Name = "Some org"
                }
            }
        };

        return new List<Claim>
        {
            new(ClaimTypes.UserData, JsonSerializer.Serialize(userData))
        };
    }
}