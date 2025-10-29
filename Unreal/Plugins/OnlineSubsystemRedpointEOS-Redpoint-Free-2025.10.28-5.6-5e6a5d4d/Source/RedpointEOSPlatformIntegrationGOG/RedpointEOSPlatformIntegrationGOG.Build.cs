// Copyright June Rhodes. All Rights Reserved.

using System;
using UnrealBuildTool;

public class RedpointEOSPlatformIntegrationGOG : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSPlatformIntegrationGOG(ReadOnlyTargetRules Target) : base("REPIG", Target)
    {
        PrivateDependencyModuleNames.AddRange(new[]
        {
            "Core",
            "CoreUObject",
            "RedpointEOSPlatform",
            "RedpointEOSPlatformIntegrationOSSv1",
            "RedpointEOSAuth",
            "RedpointEOSCore",
        });

    }
}