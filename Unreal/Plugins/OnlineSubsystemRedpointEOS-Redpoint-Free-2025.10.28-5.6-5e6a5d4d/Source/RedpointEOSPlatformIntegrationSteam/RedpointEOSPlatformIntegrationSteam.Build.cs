// Copyright June Rhodes. All Rights Reserved.

using UnrealBuildTool;

public class RedpointEOSPlatformIntegrationSteam : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSPlatformIntegrationSteam(ReadOnlyTargetRules Target) : base("REPIS", Target)
    {
        PrivateDependencyModuleNames.AddRange(new[]
        {
            "Core",
            "CoreUObject",
            "Engine",
            "RedpointEOSPlatform",
            "RedpointEOSPlatformIntegrationOSSv1",
            "RedpointEOSCore",
            "RedpointEOSAuth",
            "RedpointEOSSDK",
            "RedpointEOSAPI",
        });

    }
}