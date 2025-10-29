// Copyright June Rhodes. All Rights Reserved.

using UnrealBuildTool;

public class RedpointEOSPlatform : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSPlatform(ReadOnlyTargetRules Target) : base("REP", Target)
    {
        PublicDependencyModuleNames.AddRange(new[]
        {
            "Core",
            "CoreUObject",
            "Engine",
            "RedpointEOSSDK",
            "RedpointEOSAPI",
            "RedpointEOSConfig",
        });

        if (Target.Platform == UnrealTargetPlatform.Android)
        {
            PrivateDependencyModuleNames.Add("Launch");
        }
    }
}