// Copyright June Rhodes. All Rights Reserved.

using System.IO;
using UnrealBuildTool;

public class MatchmakingMatchmaker : RedpointEOSCPlusPlusModuleRules
{
    public MatchmakingMatchmaker(ReadOnlyTargetRules Target) : base("RMMM", Target)
    {
        PrivateDependencyModuleNames.AddRange(new[] {
            "Core",
            "CoreUObject",
            "DeveloperSettings",
            "Engine",
            "Matchmaking",
            "OnlineSubsystem",
            "OnlineSubsystemUtils",
            "RedpointEOSCore",
            "RedpointEOSFramework",
        });
    }
}