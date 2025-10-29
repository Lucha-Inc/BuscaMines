// Copyright June Rhodes. All Rights Reserved.

using System;
using UnrealBuildTool;

public class RedpointEOSLicenseValidation : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSLicenseValidation(ReadOnlyTargetRules Target) : base("RELV", Target)
    {
        PublicDependencyModuleNames.AddRange(new string[]
        {
            "Core",
            "CoreOnline",
            "RedpointEOSBuild",
            "RedpointEOSConfig",
        });

        PrivateDependencyModuleNames.AddRange(new string[]
        {
            "Engine",
            "CoreUObject",
            "HTTP",
            "RedpointEOSAPI",
            "RedpointEOSCore",
            "RedpointEOSIdentity",
        });

        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("BUILD_VERSION_NAME")))
        {
            PublicDefinitions.Add("REDPOINT_EOS_BUILD_VERSION_NAME=\"" + Environment.GetEnvironmentVariable("BUILD_VERSION_NAME") + "\"");
        }
    }
}