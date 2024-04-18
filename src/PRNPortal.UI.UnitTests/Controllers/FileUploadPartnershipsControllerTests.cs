﻿namespace PRNPortal.UI.UnitTests.Controllers;

using Application.Constants;
using Application.DTOs.Submission;
using Application.Enums;
using Application.Services.Interfaces;
using Constants;
using EPR.Common.Authorization.Models;
using EPR.Common.Authorization.Sessions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using Moq;
using UI.Controllers;
using UI.Services.Interfaces;
using UI.Sessions;

[TestFixture]
public class FileUploadPartnershipsControllerTests
{
    private static readonly string SubmissionId = Guid.NewGuid().ToString();
    private static readonly Guid _registrationSetId = Guid.NewGuid();
    private static readonly string _submissionPeriod = "subPeriod";
    private Mock<ISubmissionService> _submissionServiceMock;
    private Mock<IFileUploadService> _fileUploadServiceMock;
    private Mock<ISessionManager<FrontendSchemeRegistrationSession>> _sessionManagerMock;
    private FileUploadPartnershipsController _systemUnderTest;

    [SetUp]
    public void SetUp()
    {
        _submissionServiceMock = new Mock<ISubmissionService>();
        _sessionManagerMock = new Mock<ISessionManager<FrontendSchemeRegistrationSession>>();
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(new FrontendSchemeRegistrationSession
            {
                UserData = new UserData
                {
                    Organisations = new List<Organisation>
                    {
                        new()
                        {
                            OrganisationRole = OrganisationRoles.Producer
                        }
                    }
                },
                RegistrationSession = new RegistrationSession
                {
                    Journey = new List<string>
                    {
                        PagePaths.FileUploadCompanyDetailsSubLanding,
                        PagePaths.FileUploadCompanyDetails,
                        PagePaths.FileUploadBrands,
                        PagePaths.FileUploadPartnerships
                    },
                    LatestRegistrationSet = new Dictionary<string, Guid>
                    {
                        [_submissionPeriod] = _registrationSetId
                    },
                    SubmissionPeriod = _submissionPeriod
                },
            });

        _fileUploadServiceMock = new Mock<IFileUploadService>();

        _systemUnderTest = new FileUploadPartnershipsController(_submissionServiceMock.Object, _fileUploadServiceMock.Object, _sessionManagerMock.Object);
        _systemUnderTest.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                Request =
                {
                    Query = new QueryCollection(new Dictionary<string, StringValues>
                    {
                        { "submissionId", SubmissionId }
                    }),
                },
                Session = new Mock<ISession>().Object
            }
        };
    }

    [Test]
    public async Task Get_ReturnsFileUploadPartnershipsView_WhenRequiresPartnershipsFile()
    {
        // Arrange
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(new FrontendSchemeRegistrationSession
            {
                UserData = new UserData { Organisations = { new() { OrganisationRole = OrganisationRoles.ComplianceScheme } } },
                RegistrationSession = new RegistrationSession
                {
                    Journey = new List<string>
                    {
                        PagePaths.FileUploadCompanyDetailsSubLanding,
                        PagePaths.FileUploadCompanyDetails,
                        PagePaths.FileUploadBrands,
                        PagePaths.FileUploadPartnerships
                    }
                }
            });
        _submissionServiceMock.Setup(x => x.GetSubmissionAsync<RegistrationSubmission>(It.IsAny<Guid>()))
            .ReturnsAsync(new RegistrationSubmission { RequiresPartnershipsFile = true });

        // Act
        var result = await _systemUnderTest.Get() as ViewResult;

        // Assert
        result.ViewName.Should().Be("FileUploadPartnerships");
    }

    [Test]
    public async Task Get_ReturnsFileUploadCompanyDetailsView_WhenRequiresPartnershipsFileIsFalse()
    {
        // Arrange
        _submissionServiceMock.Setup(x => x.GetSubmissionAsync<RegistrationSubmission>(It.IsAny<Guid>()))
            .ReturnsAsync(new RegistrationSubmission { RequiresPartnershipsFile = false });

        // Act
        var result = await _systemUnderTest.Get() as RedirectToActionResult;

        // Assert
        result.ControllerName.Should().Be("FileUploadCompanyDetails");
    }

    [Test]
    public async Task Post_ReturnsFileUploadPartnershipsView_WhenTheModelStateIsInvalid()
    {
        // Arrange
        const string contentType = "content-type";
        _systemUnderTest.ControllerContext.HttpContext.Request.ContentType = contentType;
        _systemUnderTest.ModelState.AddModelError("file", "Some error");

        // Act
        var result = await _systemUnderTest.Post() as ViewResult;

        // Assert
        _fileUploadServiceMock.Verify(
            x => x.ProcessUploadAsync(
                contentType,
                It.IsAny<Stream>(),
                _submissionPeriod,
                It.IsAny<ModelStateDictionary>(),
                new Guid(SubmissionId),
                SubmissionType.Registration,
                SubmissionSubType.Partnerships,
                _registrationSetId,
                null),
            Times.Once);
        result.ViewName.Should().Be("FileUploadPartnerships");
    }

    [Test]
    public async Task Post_RedirectsToFileUploadingPartnerships_WhenTheModelStateIsValid()
    {
        // Arrange
        const string contentType = "content-type";
        var submissionId = Guid.NewGuid();
        _systemUnderTest.ControllerContext.HttpContext.Request.ContentType = contentType;
        _systemUnderTest.ControllerContext.HttpContext.Request.Query =
            new QueryCollection(new Dictionary<string, StringValues> { { "submissionId", submissionId.ToString() } });

        _fileUploadServiceMock
            .Setup(x => x.ProcessUploadAsync(
                contentType,
                It.IsAny<Stream>(),
                _submissionPeriod,
                It.IsAny<ModelStateDictionary>(),
                submissionId,
                SubmissionType.Registration,
                SubmissionSubType.Partnerships,
                _registrationSetId,
                null))
            .ReturnsAsync(submissionId);

        // Act
        var result = await _systemUnderTest.Post() as RedirectToActionResult;

        // Assert
        result.ControllerName.Should().Contain("FileUploadingPartnerships");
        result.RouteValues.Should().ContainKey("submissionId").WhoseValue.Should().Be(submissionId);
    }

    [Test]
    public async Task Get_RedirectsToFileUploadCompanyDetails_WhenOrganisationRolesIsNull()
    {
        // Arrange
        _submissionServiceMock.Setup(x => x.GetSubmissionAsync<RegistrationSubmission>(It.IsAny<Guid>()))
            .ReturnsAsync(new RegistrationSubmission { RequiresPartnershipsFile = true });
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(new FrontendSchemeRegistrationSession
            {
                UserData = new UserData { Organisations = { new() { OrganisationRole = null } } },
                RegistrationSession = new RegistrationSession
                {
                    Journey = new List<string>
                    {
                        PagePaths.FileUploadCompanyDetailsSubLanding,
                        PagePaths.FileUploadCompanyDetails,
                        PagePaths.FileUploadBrands,
                        PagePaths.FileUploadPartnerships
                    }
                }
            });
        // Act
        var result = await _systemUnderTest.Get() as RedirectToActionResult;

        // Assert
        result.ActionName.Should().Be("Get");
        result.ControllerName.Should().Be("FileUploadCompanyDetails");

        _submissionServiceMock.Verify(x => x.GetSubmissionAsync<RegistrationSubmission>(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task Get_RedirectsToFileUploadCompanyDetails_WhenSubmissionIsNull()
    {
        // Arrange
        _systemUnderTest.ControllerContext.HttpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
        {
            { "submissionId", Guid.NewGuid().ToString() }
        });
        _submissionServiceMock.Setup(x => x.GetSubmissionAsync<RegistrationSubmission>(It.IsAny<Guid>()))
            .ReturnsAsync((RegistrationSubmission?)null);
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(new FrontendSchemeRegistrationSession
            {
                UserData = new UserData { Organisations = { new() { OrganisationRole = OrganisationRoles.Producer } } }
            });
        // Act
        var result = await _systemUnderTest.Get();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
    }
}