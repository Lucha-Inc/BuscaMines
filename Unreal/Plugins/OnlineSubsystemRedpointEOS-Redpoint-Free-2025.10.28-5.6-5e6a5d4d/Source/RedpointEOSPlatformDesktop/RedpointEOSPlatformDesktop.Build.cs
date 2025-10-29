// Copyright June Rhodes. All Rights Reserved.

using UnrealBuildTool;

public class RedpointEOSPlatformDesktop : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSPlatformDesktop(ReadOnlyTargetRules Target) : base("REPDT", Target)
    {
        PrivateDependencyModuleNames.AddRange(new[]
        {
            "Core",
            "CoreUObject",
            "Projects",
            "RedpointEOSCore",
            "RedpointEOSPlatform",
        });
    }
}