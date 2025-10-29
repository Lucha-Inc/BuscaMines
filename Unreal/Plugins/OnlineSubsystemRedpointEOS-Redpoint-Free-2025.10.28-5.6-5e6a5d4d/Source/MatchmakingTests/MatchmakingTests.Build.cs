// Copyright June Rhodes. All Rights Reserved.

using UnrealBuildTool;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;

public class MatchmakingTests : RedpointEOSCPlusPlusModuleRules
{
    public MatchmakingTests(ReadOnlyTargetRules Target) : base("RMMT", Target)
    {
        PrivateDependencyModuleNames.AddRange(new[]
        {
            "Core",
            "Engine",
            "CoreUObject",
            "OnlineSubsystem",
            "OnlineSubsystemUtils",
            "Matchmaking"
        });
    }
}