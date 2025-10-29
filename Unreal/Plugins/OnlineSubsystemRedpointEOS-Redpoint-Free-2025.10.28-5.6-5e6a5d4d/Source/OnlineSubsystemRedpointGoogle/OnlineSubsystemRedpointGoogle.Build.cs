// Copyright June Rhodes. All Rights Reserved.

using UnrealBuildTool;

public class OnlineSubsystemRedpointGoogle : RedpointEOSCPlusPlusModuleRules
{
    public OnlineSubsystemRedpointGoogle(ReadOnlyTargetRules Target) : base("OSRG", Target)
    {
        PublicDependencyModuleNames.AddRange(new[]
        {
            "Core",
        });

    }
}