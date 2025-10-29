// Copyright June Rhodes. All Rights Reserved.

using UnrealBuildTool;

public class RedpointEOSPlatformIntegrationOSSv1 : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSPlatformIntegrationOSSv1(ReadOnlyTargetRules Target) : base("REPIOSS", Target)
    {
        PublicDependencyModuleNames.AddRange(new[]
        {
            "CoreOnline",
            "RedpointEOSAPI",
            "RedpointEOSAsync",
            "RedpointEOSCore",
            "RedpointEOSInterfaces",
            "RedpointEOSPlatform",
            "RedpointEOSPresence",
            "RedpointEOSUserCache",
        });

        PrivateDependencyModuleNames.AddRange(new[]
        {
            "Core",
            "CoreUObject",
            "Engine",
            "OnlineSubsystem",
            "OnlineSubsystemUtils",
            "RedpointEOSAuth",
            "RedpointEOSCompat",
            "RedpointEOSIdentity",
            "RedpointEOSRooms",
        });
    }
}