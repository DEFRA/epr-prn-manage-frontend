using System.Diagnostics.CodeAnalysis;

namespace PRNPortal.Application.Attributes;

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Field)]
public class LocalizedNameAttribute : Attribute
{
    public LocalizedNameAttribute(string name)
    {
        Name = name;
    }

    public string? Name { get; }
}