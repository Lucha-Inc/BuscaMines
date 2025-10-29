// Copyright June Rhodes. All Rights Reserved.

using UnrealBuildTool;

public class RedpointEOSPlatformIOS : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSPlatformIOS(ReadOnlyTargetRules Target) : base("REPI", Target)
    {
        PrivateDependencyModuleNames.AddRange(new[]
        {
            "Core",
            "CoreUObject",
            "Projects",
            "RedpointEOSAuth",
            "RedpointEOSPlatform",
            "RedpointEOSPlatformIntegrationApple",
        });
    }
}