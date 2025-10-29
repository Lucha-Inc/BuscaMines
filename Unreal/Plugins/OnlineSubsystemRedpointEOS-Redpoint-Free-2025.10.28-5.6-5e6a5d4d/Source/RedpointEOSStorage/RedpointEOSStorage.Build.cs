// Copyright June Rhodes. All Rights Reserved.

using System;
using UnrealBuildTool;

public class RedpointEOSStorage : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSStorage(ReadOnlyTargetRules Target) : base("RESO", Target)
    {
        PublicDependencyModuleNames.AddRange(new string[]
        {
            "Core",
            "CoreOnline",
        });

        PrivateDependencyModuleNames.AddRange(new string[]
        {
            "Engine",
            "CoreUObject",
            "RedpointEOSAPI",
            "RedpointEOSCore",
            "RedpointEOSCompat",
        });
    }
}