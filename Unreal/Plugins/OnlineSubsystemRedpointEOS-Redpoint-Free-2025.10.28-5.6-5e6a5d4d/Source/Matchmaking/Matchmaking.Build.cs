// Copyright June Rhodes. All Rights Reserved.

using UnrealBuildTool;

public class Matchmaking : RedpointEOSCPlusPlusModuleRules
{
    public Matchmaking(ReadOnlyTargetRules Target) : base("RMM", Target)
    {
        PublicDependencyModuleNames.AddRange(new[]
        {
            "Core",
            "Json",
            "OnlineSubsystem",
            "OnlineSubsystemUtils",
            "RedpointEOSInterfaces",
        });

        PrivateDependencyModuleNames.AddRange(new[]
        {
            "MatchmakingStats",
            "CoreUObject",
            "CoreOnline",
            "Engine",
            "RedpointEOSCore",
        });
    }
}