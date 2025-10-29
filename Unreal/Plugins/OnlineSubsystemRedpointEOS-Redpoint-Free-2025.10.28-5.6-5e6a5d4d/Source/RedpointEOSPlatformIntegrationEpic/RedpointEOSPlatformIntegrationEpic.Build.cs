// Copyright June Rhodes. All Rights Reserved.

using UnrealBuildTool;

public class RedpointEOSPlatformIntegrationEpic : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSPlatformIntegrationEpic(ReadOnlyTargetRules Target) : base("REPIE", Target)
    {
        PrivateDependencyModuleNames.AddRange(new[]
        {
            "Core",
            "CoreUObject",
            "Engine",
            "RedpointEOSAPI",
            "RedpointEOSAuth",
            "RedpointEOSCompat",
            "RedpointEOSCore",
            "RedpointEOSIdentity",
            "RedpointEOSPlatform",
            "RedpointEOSPresence",
            "RedpointEOSRooms",
            "RedpointEOSSDK",
            "RedpointEOSUserCache",
        });
    }
}