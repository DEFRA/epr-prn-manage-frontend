using PRNPortal.Application.DTOs.ComplianceScheme;
using PRNPortal.Application.DTOs.ComplianceSchemeMember;

namespace PRNPortal.Application.Services.Interfaces;

public interface IComplianceSchemeMemberService
{
    Task<ComplianceSchemeMembershipResponse> GetComplianceSchemeMembers(
        Guid organisationId, Guid complianceSchemeId, int pageSize, string searchQuery, int page);

    Task<ComplianceSchemeMemberDetails?> GetComplianceSchemeMemberDetails(Guid organisationId, Guid selectedSchemeId);

    Task<IReadOnlyCollection<ComplianceSchemeReasonsRemovalDto>?> GetReasonsForRemoval();

    Task<RemovedComplianceSchemeMember> RemoveComplianceSchemeMember(Guid organisationId, Guid complianceSchemeId, Guid selectedSchemeId, string reasonCode, string? tellUsMore);
}
