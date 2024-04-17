﻿namespace PRNPortal.Application.Services;

using System.Net.Http.Json;
using Constants;
using DTOs.ComplianceScheme;
using DTOs.ComplianceSchemeMember;
using Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Options;
using RequestModels;

public class ComplianceSchemeMemberService : IComplianceSchemeMemberService
{
    private const string CorrelationIdHeaderKey = "X-EPR-Correlation";

    private readonly ILogger<ComplianceSchemeMemberService> _logger;
    private readonly IAccountServiceApiClient _accountServiceApiClient;
    private readonly IComplianceSchemeService _complianceSchemeService;
    private readonly ICorrelationIdProvider _correlationIdProvider;

    public ComplianceSchemeMemberService(
        IComplianceSchemeService complianceSchemeService,
        IAccountServiceApiClient accountServiceApiClient,
        ICorrelationIdProvider correlationIdProvider,
        ILogger<ComplianceSchemeMemberService> logger)
    {
        _complianceSchemeService = complianceSchemeService;
        _accountServiceApiClient = accountServiceApiClient;
        _correlationIdProvider = correlationIdProvider;
        _logger = logger;
    }

    public async Task<ComplianceSchemeMembershipResponse> GetComplianceSchemeMembers(
        Guid organisationId, Guid complianceSchemeId, int pageSize, string searchQuery, int page)
    {
        try
        {
            var requestPath = string.Format(ComplianceSchemePaths.Members, organisationId, complianceSchemeId, pageSize, searchQuery, page);
            var result = await _accountServiceApiClient.SendGetRequest(requestPath);

            result.EnsureSuccessStatusCode();
            var content = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ComplianceSchemeMembershipResponse>(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Scheme Members for scheme {schemeId} in organisation {organisationId}",
                complianceSchemeId, organisationId);
            throw;
        }
    }

    /// <LocalBaseURL> https://localhost:7253/api/compliance-scheme/.</LocalBaseURL>
    /// <Endpoint> get/ .</Endpoint>
    /// <FullLocalUrl> https://localhost:7253/api/compliance-scheme/{organisationId}/member-details .</FullLocalUrl>
    /// <summary>Remove/stop a compliance scheme for a producer.</summary>
    /// <param name="organisationId">Organisation Identifier .</param>
    /// <param name="selectedSchemeId">Selected Scheme Identifier. </param>
    /// <returns>HttpResponseMessage.</returns>
    public async Task<ComplianceSchemeMemberDetails?> GetComplianceSchemeMemberDetails(Guid organisationId, Guid selectedSchemeId)
    {
        var endpoint = string.Format($"{ComplianceSchemePaths.GetMemberDetails}", organisationId, selectedSchemeId);
        var result = await _accountServiceApiClient.SendGetRequest(endpoint);
        result.EnsureSuccessStatusCode();
        return await result.Content.ReadFromJsonAsync<ComplianceSchemeMemberDetails>();
    }

    /// <LocalBaseURL> https://localhost:7253/api/compliance-scheme/ .</LocalBaseURL>
    /// <Endpoint> get/ .</Endpoint>
    /// <FullLocalUrl> https://localhost:7253/api/compliance-scheme/GetReasonsForRemoval .</FullLocalUrl>
    /// <summary>Gets all compliance active scheme member Reasons for Removal.</summary>
    /// <returns>IEnumerable.<ReasonsForRemovalCSDto>?.</returns>
    public async Task<IReadOnlyCollection<ComplianceSchemeReasonsRemovalDto>?> GetReasonsForRemoval()
    {
        var result = await _accountServiceApiClient.SendGetRequest(ComplianceSchemePaths.GetReasonsForRemoval);
        result.EnsureSuccessStatusCode();
        var content = await result.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<IReadOnlyCollection<ComplianceSchemeReasonsRemovalDto>>(content);
    }

    public async Task<RemovedComplianceSchemeMember> RemoveComplianceSchemeMember(Guid organisationId, Guid complianceSchemeId, Guid selectedSchemeId, string reasonCode, string? tellUsMore)
    {
        var endpoint = string.Format(ComplianceSchemePaths.RemoveComplianceSchemeMember, organisationId, selectedSchemeId);
        var requestModel = new ReasonForRemovalRequestModel
        {
            Code = reasonCode,
            TellUsMore = tellUsMore
        };

        _accountServiceApiClient.AddHttpClientHeader(CorrelationIdHeaderKey, _correlationIdProvider.GetCurrentCorrelationIdOrNew().ToString());

        var result = await _accountServiceApiClient.SendPostRequest(endpoint, requestModel);

        _complianceSchemeService.ClearSummaryCache(organisationId, complianceSchemeId);

        result.EnsureSuccessStatusCode();

        return await result.Content.ReadFromJsonAsync<RemovedComplianceSchemeMember>();
    }
}
