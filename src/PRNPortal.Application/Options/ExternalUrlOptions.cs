﻿namespace PRNPortal.Application.Options;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class ExternalUrlOptions
{
    public const string ConfigSection = "ExternalUrls";

    public string LandingPage { get; set; }

    public string GovUkHome { get; set; }

    public string PrivacyScottishEnvironmentalProtectionAgency { get; set; }

    public string PrivacyNationalResourcesWales { get; set; }

    public string PrivacyNorthernIrelandEnvironmentAgency { get; set; }

    public string PrivacyEnvironmentAgency { get; set; }

    public string PrivacyDataProtectionPublicRegister { get; set; }

    public string PrivacyDefrasPersonalInformationCharter { get; set; }

    public string PrivacyInformationCommissioner { get; set; }
}