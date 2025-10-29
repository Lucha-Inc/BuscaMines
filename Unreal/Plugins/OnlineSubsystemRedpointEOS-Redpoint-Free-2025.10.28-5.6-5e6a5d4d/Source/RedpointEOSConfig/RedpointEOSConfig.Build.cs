// Copyright June Rhodes. All Rights Reserved.

using UnrealBuildTool;

public class RedpointEOSConfig : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSConfig(ReadOnlyTargetRules Target) : base("RECFG", Target)
    {
        PublicDependencyModuleNames.AddRange(new[]
        {
            "Core",
            "CoreUObject",
            "Projects",
            "RedpointEOSAPI",
            "RedpointEOSSDK",
        });
    }
}