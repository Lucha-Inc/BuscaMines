// Copyright June Rhodes. All Rights Reserved.

using UnrealBuildTool;

public class RedpointEOSUtils : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSUtils(ReadOnlyTargetRules Target) : base("REU", Target)
    {
        PrivateDependencyModuleNames.AddRange(new[]
        {
            "Core",
            "Engine",
            "CoreUObject",
            "OnlineSubsystem",
            "OnlineSubsystemUtils",
            "OnlineSubsystemRedpointEOS",
            "RedpointEOSFramework",
        });
    }
}