// Copyright June Rhodes. All Rights Reserved.

using System;
using UnrealBuildTool;

public class RedpointEOSPresence : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSPresence(ReadOnlyTargetRules Target) : base("REPR", Target)
    {
        PublicDependencyModuleNames.AddRange(new string[]
        {
            "Core",
            "CoreOnline",
            "RedpointEOSAPI",
            "RedpointEOSIdentity",
            "RedpointEOSRooms",
            "RedpointEOSUserCache",
        });

        PrivateDependencyModuleNames.AddRange(new string[]
        {
            "Engine",
            "CoreUObject",
            "RedpointEOSCore",
            "RedpointEOSPlatform",
            "RedpointEOSCompat",
            "RedpointEOSSDK",
            "Json",
        });
    }
}