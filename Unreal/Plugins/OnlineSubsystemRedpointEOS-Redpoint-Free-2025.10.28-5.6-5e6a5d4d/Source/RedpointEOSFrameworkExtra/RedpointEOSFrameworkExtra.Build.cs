// Copyright June Rhodes. All Rights Reserved.

using System;
using UnrealBuildTool;

public class RedpointEOSFrameworkExtra : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSFrameworkExtra(ReadOnlyTargetRules Target) : base("REFRE", Target)
    {
        PublicDependencyModuleNames.AddRange(new string[]
        {
            "Core",
            "CoreOnline",
            "CoreUObject",
            "EnhancedInput",
            "GameplayAbilities",
            "GameplayTasks",
            "GameplayTags",
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