// Copyright June Rhodes. All Rights Reserved.

using EpicGames.Core;
using System;
using UnrealBuildTool;

public class RedpointEOSPlatformIntegrationApple : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSPlatformIntegrationApple(ReadOnlyTargetRules Target) : base("REPIA", Target)
    {
        PrivateDependencyModuleNames.AddRange(new[]
        {
            "Core",
            "CoreUObject",
            "Projects",
            "RedpointEOSAuth",
            "RedpointEOSPlatform",
        });

    }
}