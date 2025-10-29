// Copyright June Rhodes. All Rights Reserved.

using System;
using UnrealBuildTool;

public class RedpointEOSUserCache : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSUserCache(ReadOnlyTargetRules Target) : base("REUC", Target)
    {
        PublicDependencyModuleNames.AddRange(new string[]
        {
            "Core",
            "CoreOnline",
            "RedpointEOSAPI",
            "RedpointEOSCore",
            "RedpointEOSCompat",
        });

        PrivateDependencyModuleNames.AddRange(new string[]
        {
            "Engine",
            "CoreUObject",
        });
    }
}