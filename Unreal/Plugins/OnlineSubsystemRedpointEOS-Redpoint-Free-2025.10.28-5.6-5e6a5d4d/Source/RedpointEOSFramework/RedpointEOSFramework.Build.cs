// Copyright June Rhodes. All Rights Reserved.

using System;
using UnrealBuildTool;

public class RedpointEOSFramework : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSFramework(ReadOnlyTargetRules Target) : base("REFR", Target)
    {
        PublicDependencyModuleNames.AddRange(new string[]
        {
            "Core",
            "CoreOnline",
            "CoreUObject",
            "EnhancedInput",
            "OnlineSubsystemBlueprints",
            "RedpointEOSFriends",
            "RedpointEOSIdentity",
            "RedpointEOSPlatform",
            "RedpointEOSPresence",
            "RedpointEOSRooms",
            "Slate",
            "SlateCore",
            "UMG",
        });

        PrivateDependencyModuleNames.AddRange(new string[]
        {
            "Engine",
            "EngineSettings",
            "InputCore",
            "NetCore",
            "RedpointEOSAPI",
            "RedpointEOSAsync",
            "RedpointEOSConfig",
            "RedpointEOSCore",
            "RedpointEOSInterfaces",
            "RedpointEOSUserCache",
            "OnlineSubsystem",
            "OnlineSubsystemUtils",
        });
    }
}