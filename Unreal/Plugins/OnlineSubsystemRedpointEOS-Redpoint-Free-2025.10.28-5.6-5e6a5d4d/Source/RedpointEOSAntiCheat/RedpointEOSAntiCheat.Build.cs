// Copyright June Rhodes. All Rights Reserved.

using UnrealBuildTool;

public class RedpointEOSAntiCheat : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSAntiCheat(ReadOnlyTargetRules Target) : base("REAC", Target)
    {
        PublicDependencyModuleNames.AddRange(new string[]
        {
            "Core",
            "CoreOnline",
            "RedpointEOSAPI",
        });

        PrivateDependencyModuleNames.AddRange(new string[]
        {
            "CoreUObject",
            "Engine",
            "RedpointEOSConfig",
            "RedpointEOSCore",
            "RedpointEOSSDK",
        });
    }
}