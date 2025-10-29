// Copyright June Rhodes. All Rights Reserved.

using UnrealBuildTool;

public class RedpointEOSPlatformIntegrationItchIo : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSPlatformIntegrationItchIo(ReadOnlyTargetRules Target) : base("REPII", Target)
    {
        PrivateDependencyModuleNames.AddRange(new[]
        {
            "Core",
            "CoreUObject",
            "Engine",
            "Json",
            "HTTP",
            "RedpointEOSAPI",
            "RedpointEOSAsync",
            "RedpointEOSAuth",
            "RedpointEOSCompat",
            "RedpointEOSCore",
            "RedpointEOSPlatform",
            "RedpointEOSSDK",
            "RedpointEOSUserCache",
        });

        if (Target.Platform == UnrealTargetPlatform.Win64 ||
            Target.Platform == UnrealTargetPlatform.Mac ||
            Target.Platform == UnrealTargetPlatform.Linux)
        {
            PrivateDefinitions.Add("REDPOINT_EOS_ITCH_IO_ENABLED=1");
        }
        else
        {
            PrivateDefinitions.Add("REDPOINT_EOS_ITCH_IO_ENABLED=0");
        }
    }
}