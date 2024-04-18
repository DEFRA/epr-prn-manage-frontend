﻿using EPR.Common.Authorization.Sessions;
using FluentAssertions;
using PRNPortal.Application.Constants;
using PRNPortal.Application.DTOs.Submission;
using PRNPortal.Application.Options;
using PRNPortal.Application.Services.Interfaces;
using PRNPortal.UI.Controllers;
using PRNPortal.UI.Controllers.ControllerExtensions;
using PRNPortal.UI.Sessions;
using PRNPortal.UI.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;

namespace PRNPortal.UI.UnitTests.Controllers;

[TestFixture]
public class FileUploadWarningControllerTests
{
    private static readonly Guid SubmissionId = Guid.NewGuid();
    private Mock<ISubmissionService> _submissionServiceMock;
    private FileUploadWarningController _systemUnderTest;
    private Mock<ISessionManager<FrontendSchemeRegistrationSession>> _sessionManagerMock;
    private ValidationOptions _validationOptions;

    [SetUp]
    public void SetUp()
    {
        _sessionManagerMock = new Mock<ISessionManager<FrontendSchemeRegistrationSession>>();
        _validationOptions = new ValidationOptions { MaxIssuesToProcess = 100 };
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(new FrontendSchemeRegistrationSession
            {
                RegistrationSession = new RegistrationSession
                {
                    Journey = new List<string> { PagePaths.FileUploadSubLanding, PagePaths.FileUploading }
                }
            });

        _submissionServiceMock = new Mock<ISubmissionService>();
        _systemUnderTest = new FileUploadWarningController(
            _submissionServiceMock.Object,
            _sessionManagerMock.Object,
            Options.Create(_validationOptions));

        _systemUnderTest.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                Request =
                {
                    Query = new QueryCollection(new Dictionary<string, StringValues>
                    {
                        { "submissionId", SubmissionId.ToString() },
                    }),
                },
                Session = new Mock<ISession>().Object
            },
        };
    }

    [Test]
    public async Task Get_RedirectsToFileUploadGet_WhenGetSubmissionAsyncReturnsNull()
    {
        // Arrange
        _submissionServiceMock.Setup(x => x.GetSubmissionAsync<PomSubmission>(It.IsAny<Guid>())).ReturnsAsync((PomSubmission)null);

        // Act
        var result = await _systemUnderTest.Get() as RedirectToActionResult;

        // Assert
        result.ActionName.Should().Be("Get");
        result.ControllerName.Should().Be("FileUpload");

        _submissionServiceMock.Verify(x => x.GetSubmissionAsync<PomSubmission>(It.IsAny<Guid>()), Times.Once);
    }

    [Test]
    public async Task Get_RedirectsToFileUploadGet_WhenGetSubmissionAsyncReturnsSubmissionWithDataCompleteFalse()
    {
        // Arrange
        _submissionServiceMock.Setup(x => x.GetSubmissionAsync<PomSubmission>(It.IsAny<Guid>())).ReturnsAsync(new PomSubmission
        {
            PomDataComplete = false
        });

        // Act
        var result = await _systemUnderTest.Get() as RedirectToActionResult;

        // Assert
        result.ActionName.Should().Be("Get");
        result.ControllerName.Should().Be("FileUpload");

        _submissionServiceMock.Verify(x => x.GetSubmissionAsync<PomSubmission>(It.IsAny<Guid>()), Times.Once);
    }

    [Test]
    public async Task Get_ReturnsFileUploadWarningView_WhenGetSubmissionAsyncReturnsCompletedValidSubmissionWithOnlyWarnings()
    {
        // Arrange
        const string fileName = "example.csv";
        _submissionServiceMock.Setup(x => x.GetSubmissionAsync<PomSubmission>(It.IsAny<Guid>())).ReturnsAsync(new PomSubmission
        {
            PomFileName = fileName,
            PomDataComplete = true,
            ValidationPass = true,
            HasWarnings = true
        });

        // Act
        var result = await _systemUnderTest.Get() as ViewResult;

        // Assert
        result.ViewName.Should().Be("FileUploadWarning");
        result.Model.Should().BeEquivalentTo(new FileUploadWarningViewModel()
        {
            FileName = fileName,
            SubmissionId = SubmissionId,
            MaxWarningsToProcess = 100
        });

        _submissionServiceMock.Verify(x => x.GetSubmissionAsync<PomSubmission>(It.IsAny<Guid>()), Times.Once);
    }

    [Test]
    public async Task Get_RedirectsToFileUploadSubLanding_WhenJourneyDoesNotContainFileUploadingPath()
    {
        // Arrange
        const string fileName = "example.csv";
        _submissionServiceMock.Setup(x => x.GetSubmissionAsync<PomSubmission>(It.IsAny<Guid>())).ReturnsAsync(new PomSubmission
        {
            PomFileName = fileName,
            PomDataComplete = true,
            ValidationPass = true
        });

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(new FrontendSchemeRegistrationSession
            {
                RegistrationSession = new RegistrationSession
                {
                    Journey = new List<string> { }
                }
            });

        // Act
        var result = await _systemUnderTest.Get() as RedirectToActionResult;

        // Assert
        result?.ControllerName.Should().Be(nameof(FileUploadSubLandingController).RemoveControllerFromName());
        result?.ActionName.Should().Be(nameof(FileUploadSubLandingController.Get));
    }

    [Test]
    public async Task Post_WhenUserOptsToUploadNew_RedirectsToFileUploadGet()
    {
        // Arrange
        var viewModel = new FileUploadWarningViewModel
        {
            SubmissionId = SubmissionId,
            UploadNewFile = true
        };

        // Act
        var result = await _systemUnderTest.FileUploadDecision(viewModel) as RedirectToActionResult;

        // Assert
        result?.ControllerName.Should().Be(nameof(FileUploadController).RemoveControllerFromName());
        result?.ActionName.Should().Be(nameof(FileUploadController.Get));
    }

    [Test]
    public async Task Post_WhenUserOptsNotToUploadNew_RedirectsToFileUploadCheckFileAndSubmitGet()
    {
        // Arrange
        var viewModel = new FileUploadWarningViewModel
        {
            SubmissionId = SubmissionId,
            UploadNewFile = false
        };

        // Act
        var result = await _systemUnderTest.FileUploadDecision(viewModel) as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        result?.ControllerName.Should().Be(nameof(FileUploadCheckFileAndSubmitController).RemoveControllerFromName());
        result?.ActionName.Should().Be(nameof(FileUploadCheckFileAndSubmitController.Get));
        result.RouteValues["submissionId"].Should().Be(SubmissionId);
    }
}