// Copyright June Rhodes. All Rights Reserved.

using UnrealBuildTool;

public class RedpointEOSPlatformAndroid : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSPlatformAndroid(ReadOnlyTargetRules Target) : base("REPA", Target)
    {
        PrivateDependencyModuleNames.AddRange(new[]
        {
            "Core",
            "CoreUObject",
            "RedpointEOSCore",
            "RedpointEOSAuth",
            "RedpointEOSPlatform",
            "RedpointEOSPlatformIntegrationMeta",
            "RedpointEOSPlatformIntegrationGooglePlay",
        });
    }
}