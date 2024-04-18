using EPR.Common.Authorization.Sessions;
using FluentAssertions;
using PRNPortal.Application.Options;
using PRNPortal.Application.Services.Interfaces;
using PRNPortal.UI.Controllers;
using PRNPortal.UI.Sessions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace PRNPortal.UI.UnitTests.Controllers;

[TestFixture]
public class FrontendSchemeRegistrationControllerTests
{
    private FrontendSchemeRegistrationController _systemUnderTest;
    private FrontendSchemeRegistrationSession _session;

    [SetUp]
    public void SetUp()
    {
        _session = new FrontendSchemeRegistrationSession();
        var mockSessionManager = new Mock<ISessionManager<FrontendSchemeRegistrationSession>>();
        mockSessionManager.Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(_session);
        var mockLogger = new Mock<ILogger<FrontendSchemeRegistrationController>>();
        var mockCss = new Mock<IComplianceSchemeService>();
        var mockAs = new Mock<IAuthorizationService>();
        var mockNs = new Mock<INotificationService>();
        var mockGv = new Mock<IOptions<GlobalVariables>>();
        mockGv.Setup(m => m.Value).Returns(new GlobalVariables());
        _systemUnderTest = new FrontendSchemeRegistrationController(
            mockSessionManager.Object,
            mockLogger.Object,
            mockCss.Object,
            mockAs.Object,
            mockNs.Object,
            mockGv.Object);
        _systemUnderTest.ControllerContext = new ControllerContext
        {
            HttpContext = new Mock<HttpContext>().Object
        };
    }

    [Test]
    public void ApprovedPersonCreated_ReturnsCorrectViewAndModel()
    {
        // Arrange
        var message = "some_new_message";

        // Act
        var result = _systemUnderTest.ApprovedPersonCreated(message).Result;

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        _session.RegistrationSession.NotificationMessage.Should().Be(message);
    }
}