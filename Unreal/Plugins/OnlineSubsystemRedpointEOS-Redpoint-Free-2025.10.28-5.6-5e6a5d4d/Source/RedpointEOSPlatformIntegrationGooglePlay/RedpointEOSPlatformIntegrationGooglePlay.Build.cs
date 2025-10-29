// Copyright June Rhodes. All Rights Reserved.

using UnrealBuildTool;

public class RedpointEOSPlatformIntegrationGooglePlay : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSPlatformIntegrationGooglePlay(ReadOnlyTargetRules Target) : base("REPIGP", Target)
    {
        PrivateDependencyModuleNames.AddRange(new[]
        {
            "Core",
            "CoreUObject",
            "RedpointEOSCore",
            "RedpointEOSAuth",
            "RedpointEOSPlatform",
            "RedpointEOSPlatformIntegrationOSSv1",
            "OnlineSubsystem",
            "OnlineSubsystemUtils",
        });

    }
}