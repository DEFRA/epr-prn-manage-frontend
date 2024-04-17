﻿namespace PRNPortal.Application.UnitTests.Services;

using System.Net;
using Application.Services;
using Application.Services.Interfaces;
using AutoFixture;
using AutoFixture.AutoMoq;
using DTOs.ComplianceSchemeMember;
using DTOs.UserAccount;
using FluentAssertions;
using PRNPortal.Application.DTOs.ComplianceScheme;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Moq;
using Moq.Protected;
using Options;
using RequestModels;
using UI.Constants;
using UI.Extensions;

[TestFixture]
public class ComplianceSchemeMemberServiceTests : ServiceTestBase<IComplianceSchemeMemberService>
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private readonly Mock<ILogger<ComplianceSchemeMemberService>> _loggerMock = new();
    private readonly Mock<ICorrelationIdProvider> _correlationIdProvider = new();
    private readonly Mock<ITokenAcquisition> _tokenAcquisitionMock = new();
    private readonly Mock<IComplianceSchemeService> _complianceSchemeServiceMock = new ();
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private IComplianceSchemeMemberService _complianceSchemeMemberService;
    private Organisation _organisation;

    [SetUp]
    public void Setup()
    {
        _organisation = UserAccount.Organisations.SingleOrDefault(x => x.OrganisationRole == OrganisationRoles.Producer);
    }

    [Test]
    public async Task GetComplianceSchemeMembers_RecordExists_ReturnResult()
    {
        var response = _fixture.Create<ComplianceSchemeMembershipResponse>();
        var stringContent = response.ToJsonContent();
        _complianceSchemeMemberService = MockService(HttpStatusCode.OK, stringContent);

        var result = await _complianceSchemeMemberService.GetComplianceSchemeMembers(
            Guid.NewGuid(), Guid.NewGuid(), 50, string.Empty, 1);
        result.Should().NotBeNull();
        result.Should().BeOfType<ComplianceSchemeMembershipResponse>();
    }

    [Test]
    public async Task GetComplianceSchemeMembers_ApiCallFails_ThrowsError()
    {
        var schemeId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();

        _complianceSchemeMemberService = MockService(HttpStatusCode.InternalServerError, string.Empty.ToJsonContent());

        Assert.ThrowsAsync<HttpRequestException>(() => _complianceSchemeMemberService.GetComplianceSchemeMembers(
            organisationId, schemeId, 50, string.Empty, 1));
        _loggerMock.VerifyLog(
            logger => logger.LogError(
                "Failed to get Scheme Members for scheme {schemeId} in organisation {organisationId}", schemeId, organisationId), Times.Once);
    }

    [Test]
    public async Task GetComplianceSchemeMemberDetailsAsync_WhenMemberDetailsExistsForUser_ReturnOkResult()
    {
        // Arrange
        var complianceSchemes = new ComplianceSchemeMemberDetails
        {
            OrganisationName = "ComplianceScheme1",
            OrganisationNumber = "947841",
            CompanyHouseNumber = "684684"
        };
        var stringContent = complianceSchemes.ToJsonContent();

        _complianceSchemeMemberService = MockService(HttpStatusCode.OK, stringContent);

        // Act
        var result = await _complianceSchemeMemberService.GetComplianceSchemeMemberDetails(
            Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.OrganisationName.Should().Be(complianceSchemes.OrganisationName);
        result.OrganisationNumber.Should().Be(complianceSchemes.OrganisationNumber);
        result.CompanyHouseNumber.Should().Be(complianceSchemes.CompanyHouseNumber);
    }

    [Test]
    public async Task GetComplianceSchemeMemberDetailsAsync_WhenMemberDetailsNotFoundForUser_ReturnsNotFound()
    {
        // Arrange
        _complianceSchemeMemberService = MockService(HttpStatusCode.NotFound, null);

        // Act Assert
        Assert.ThrowsAsync<HttpRequestException>(() => _complianceSchemeMemberService.GetComplianceSchemeMemberDetails(
            Guid.NewGuid(), Guid.NewGuid()));
    }

    [Test]
    public async Task GetReasonsForRemoval_ReturnsReasons()
    {
        var reasonForRemoval = new List<ComplianceSchemeReasonsRemovalDto>
        {
            new()
            {
                Code = "A",
                RequiresReason = false
            },
            new()
            {
                Code = "B",
                RequiresReason = false
            }
        };

        var stringContent = reasonForRemoval.ToJsonContent();
        _complianceSchemeMemberService = MockService(HttpStatusCode.OK, stringContent);

        var result = await _complianceSchemeMemberService.GetReasonsForRemoval();
        var reasonsForRemoval = result.ToList();
        reasonsForRemoval.Count().Should().Be(2);
    }

    [Test]
    public async Task RemoveComplianceSchemeMember_WhenRemovesSchemeMember_ReturnsOkResult()
    {
        // Arrange
        var requestModel = new ReasonForRemovalRequestModel
        {
            Code = "A",
            TellUsMore = "Test"
        };

        var complianceSchemes = new RemovedComplianceSchemeMember
        {
            OrganisationName = "BiffPack"
        };

        var stringContent = complianceSchemes.ToJsonContent();

        _complianceSchemeMemberService = MockService(HttpStatusCode.OK, stringContent);

        var organisationId = Guid.NewGuid();
        var complianceSchemeId = Guid.NewGuid();

        // Act
        var result =
            await _complianceSchemeMemberService.RemoveComplianceSchemeMember(
                organisationId,
                complianceSchemeId,
                Guid.NewGuid(),
                requestModel.Code,
                requestModel.TellUsMore);

        // Assert
        result.OrganisationName.Should().Be(complianceSchemes.OrganisationName);
        _complianceSchemeServiceMock.Verify(service => service.ClearSummaryCache(organisationId, complianceSchemeId), Times.Once);

        _correlationIdProvider.Verify(service => service.GetCurrentCorrelationIdOrNew(), Times.Once);
    }

    protected override IComplianceSchemeMemberService MockService(
        HttpStatusCode expectedStatusCode, HttpContent expectedContent, bool raiseServiceException = false)
    {
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = expectedStatusCode,
                Content = expectedContent,
            });

        var client = new HttpClient(_httpMessageHandlerMock.Object);
        client.BaseAddress = new Uri("https://mock/api/test/");
        client.Timeout = TimeSpan.FromSeconds(30);

        var facadeOptions = Options.Create(new AccountsFacadeApiOptions { DownstreamScope = "https://mock/test" });
        var accountServiceApiClient = new AccountServiceApiClient(client, _tokenAcquisitionMock.Object, facadeOptions);

        return new ComplianceSchemeMemberService(_complianceSchemeServiceMock.Object, accountServiceApiClient, _correlationIdProvider.Object, _loggerMock.Object);
    }
}
