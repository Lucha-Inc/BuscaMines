// Copyright June Rhodes. All Rights Reserved.

using UnrealBuildTool;

public class RedpointFabAuthentication : RedpointEOSCPlusPlusModuleRules
{
    public RedpointFabAuthentication(ReadOnlyTargetRules Target) : base("RFA", Target)
    {
        PrivateDependencyModuleNames.AddRange(new[]
        {
            "Core",
            "CoreUObject",
            "Engine",
            "RedpointEOSAPI",
            "RedpointEOSCompat",
            "RedpointEOSConfig",
            "RedpointEOSCore",
        });
    }
}