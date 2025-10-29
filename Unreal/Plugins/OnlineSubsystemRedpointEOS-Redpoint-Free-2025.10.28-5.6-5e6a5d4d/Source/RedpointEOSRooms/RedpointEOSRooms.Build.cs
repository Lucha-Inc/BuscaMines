// Copyright June Rhodes. All Rights Reserved.

using System;
using UnrealBuildTool;

public class RedpointEOSRooms : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSRooms(ReadOnlyTargetRules Target) : base("RER", Target)
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
            "CoreUObject",
            "Json",
            "RedpointEOSUserCache",
            "RedpointEOSIdentity",
            "RedpointEOSPlatform",
        });
    }
}