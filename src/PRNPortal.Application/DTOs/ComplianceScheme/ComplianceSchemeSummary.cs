using System.Text.Json.Serialization;
using PRNPortal.Application.Enums;

namespace PRNPortal.Application.DTOs.ComplianceScheme;

public record ComplianceSchemeSummary
{
    public string Name { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Nation? Nation { get; init; }

    public DateTimeOffset CreatedOn { get; init; }

    public DateTimeOffset? MembersLastUpdatedOn { get; init; }

    public int MemberCount { get; init; }
}
