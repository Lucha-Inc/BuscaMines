// Copyright June Rhodes. All Rights Reserved.

using System;
using UnrealBuildTool;

public class RedpointEOSFriends : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSFriends(ReadOnlyTargetRules Target) : base("REF", Target)
    {
        PublicDependencyModuleNames.AddRange(new string[]
        {
            "Core",
            "CoreOnline",
            "RedpointEOSAPI",
            "RedpointEOSIdentity",
            "RedpointEOSPresence",
        });

        PrivateDependencyModuleNames.AddRange(new string[]
        {
            "CoreUObject",
            "Engine",
            "Json",
            "RedpointEOSCompat",
            "RedpointEOSCore",
            "RedpointEOSStorage",
            "RedpointEOSPlatform",
            "RedpointEOSUserCache",
        });
    }
}