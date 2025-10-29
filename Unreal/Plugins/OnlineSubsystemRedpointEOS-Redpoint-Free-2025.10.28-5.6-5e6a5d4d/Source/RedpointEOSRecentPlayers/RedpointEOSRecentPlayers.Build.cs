// Copyright June Rhodes. All Rights Reserved.

using System;
using UnrealBuildTool;

public class RedpointEOSRecentPlayers : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSRecentPlayers(ReadOnlyTargetRules Target) : base("RERP", Target)
    {
        PrivateDependencyModuleNames.AddRange(new string[]
        {
            "Core",
            "CoreOnline",
            "CoreUObject",
            "Engine",
            "RedpointEOSAPI",
            "RedpointEOSCore",
            "RedpointEOSFriends",
        });
    }
}